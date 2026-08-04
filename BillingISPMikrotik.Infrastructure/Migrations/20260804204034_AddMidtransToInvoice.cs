using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingISPMikrotik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMidtransToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MidtransOrderId",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapToken",
                table: "Invoices",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MidtransOrderId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SnapToken",
                table: "Invoices");
        }
    }
}
