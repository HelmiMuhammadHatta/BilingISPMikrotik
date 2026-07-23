using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BillingISPMikrotik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    PppUsername = table.Column<string>(type: "text", nullable: false),
                    PppPassword = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SpeedUp = table.Column<int>(type: "integer", nullable: false),
                    SpeedDown = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    MikrotikProfileName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MikrotikActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MikrotikActionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MikrotikActionLogs_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_ServicePlans_ServicePlanId",
                        column: x => x.ServicePlanId,
                        principalTable: "ServicePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLogs_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "CreatedAt", "Name", "Phone", "PppPassword", "PppUsername", "Status" },
                values: new object[,]
                {
                    { new Guid("341c2af0-c515-4ab4-829b-8f469b0e66e7"), "Jl. Merdeka No 1", new DateTime(2026, 6, 23, 6, 32, 35, 640, DateTimeKind.Utc).AddTicks(3365), "Budi Santoso", "081234567890", "passwordbudi", "budi", 0 },
                    { new Guid("8873625f-dffd-46a3-acff-e4059777851f"), "Jl. Gatot Subroto No 4", new DateTime(2026, 3, 23, 6, 32, 35, 640, DateTimeKind.Utc).AddTicks(4087), "Dewi Lestari", "084567890123", "passworddewi", "dewi", 0 },
                    { new Guid("d47cd502-8e81-4c68-a1b5-404da935add5"), "Jl. Sudirman No 2", new DateTime(2026, 5, 23, 6, 32, 35, 640, DateTimeKind.Utc).AddTicks(4072), "Siti Aminah", "082345678901", "passwordsiti", "siti", 0 },
                    { new Guid("d85d406d-3056-4090-a7b4-2f0c624d2e59"), "Jl. Thamrin No 3", new DateTime(2026, 4, 23, 6, 32, 35, 640, DateTimeKind.Utc).AddTicks(4082), "Agus Pratama", "083456789012", "passwordagus", "agus", 1 },
                    { new Guid("dd202a1f-13d3-469d-afca-f7dd83a138e4"), "Jl. Ahmad Yani No 5", new DateTime(2026, 2, 23, 6, 32, 35, 640, DateTimeKind.Utc).AddTicks(4090), "Joko Widodo", "085678901234", "passwordjoko", "joko", 2 }
                });

            migrationBuilder.InsertData(
                table: "ServicePlans",
                columns: new[] { "Id", "MikrotikProfileName", "Name", "Price", "SpeedDown", "SpeedUp" },
                values: new object[,]
                {
                    { new Guid("1b9da367-44e8-4ee3-9521-4de0c768ad43"), "profile_20m", "Standard 20Mbps", 250000m, 20, 20 },
                    { new Guid("ceb16e3d-b847-445b-baaf-88712278c68e"), "profile_10m", "Basic 10Mbps", 150000m, 10, 10 },
                    { new Guid("dc527fe1-1447-4d10-8634-b5f4b3f69186"), "profile_50m", "Premium 50Mbps", 400000m, 50, 50 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ServicePlanId",
                table: "Invoices",
                column: "ServicePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MikrotikActionLogs_CustomerId",
                table: "MikrotikActionLogs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_InvoiceId",
                table: "PaymentLogs",
                column: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MikrotikActionLogs");

            migrationBuilder.DropTable(
                name: "PaymentLogs");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ServicePlans");
        }
    }
}
