using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BillingISPMikrotik.API.Controllers;
using BillingISPMikrotik.API.DTOs;
using BillingISPMikrotik.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BillingISPMikrotik.Application.Tests;

public class WebhooksControllerTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<WebhooksController>> _loggerMock;
    private readonly WebhooksController _controller;
    private const string ServerKey = "SB-Mid-server-DUMMY_KEY_12345";

    public WebhooksControllerTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["PaymentGateway:ServerKey"]).Returns(ServerKey);
        
        _loggerMock = new Mock<ILogger<WebhooksController>>();
        
        _controller = new WebhooksController(_paymentServiceMock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    private string GenerateSignature(string orderId, string statusCode, string grossAmount)
    {
        var rawString = $"{orderId}{statusCode}{grossAmount}{ServerKey}";
        using var sha512 = SHA512.Create();
        var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(rawString));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    [Fact]
    public async Task PaymentGatewayWebhook_WithValidSignatureAndSettlement_CallsConfirmPayment()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var orderId = $"{invoiceId}-123456789";
        var payload = new MidtransWebhookPayload
        {
            OrderId = orderId,
            StatusCode = "200",
            GrossAmount = "150000.00",
            TransactionStatus = "settlement",
            PaymentType = "bank_transfer",
            TransactionId = "trx-123"
        };
        payload.SignatureKey = GenerateSignature(payload.OrderId, payload.StatusCode, payload.GrossAmount);

        // Act
        var result = await _controller.PaymentGatewayWebhook(payload);

        // Assert
        result.Should().BeOfType<OkResult>();
        _paymentServiceMock.Verify(s => s.ConfirmPaymentAsync(
            invoiceId, 
            "bank_transfer", 
            150000.00m, 
            "trx-123"), Times.Once);
    }

    [Fact]
    public async Task PaymentGatewayWebhook_WithInvalidSignature_Returns403()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var orderId = $"{invoiceId}-123456789";
        var payload = new MidtransWebhookPayload
        {
            OrderId = orderId,
            StatusCode = "200",
            GrossAmount = "150000.00",
            TransactionStatus = "settlement",
            SignatureKey = "invalid-signature"
        };

        // Act
        var result = await _controller.PaymentGatewayWebhook(payload) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(403);
        _paymentServiceMock.Verify(s => s.ConfirmPaymentAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PaymentGatewayWebhook_WithExpireStatus_CallsCancelPayment()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var orderId = $"{invoiceId}-123456789";
        var payload = new MidtransWebhookPayload
        {
            OrderId = orderId,
            StatusCode = "202",
            GrossAmount = "150000.00",
            TransactionStatus = "expire"
        };
        payload.SignatureKey = GenerateSignature(payload.OrderId, payload.StatusCode, payload.GrossAmount);

        // Act
        var result = await _controller.PaymentGatewayWebhook(payload);

        // Assert
        result.Should().BeOfType<OkResult>();
        _paymentServiceMock.Verify(s => s.CancelPaymentAsync(invoiceId), Times.Once);
    }
}
