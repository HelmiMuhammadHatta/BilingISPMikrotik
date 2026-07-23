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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServicePlanId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_ServicePlans_ServicePlanId",
                        column: x => x.ServicePlanId,
                        principalTable: "ServicePlans",
                        principalColumn: "Id");
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
                table: "ServicePlans",
                columns: new[] { "Id", "MikrotikProfileName", "Name", "Price", "SpeedDown", "SpeedUp" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "profile_10m", "Basic 10Mbps", 150000m, 10, 10 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "profile_20m", "Standard 20Mbps", 250000m, 20, 20 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "profile_50m", "Premium 50Mbps", 400000m, 50, 50 }
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "CreatedAt", "Name", "Phone", "PppPassword", "PppUsername", "ServicePlanId", "Status" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Jl. Merdeka No 1", new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Budi Santoso", "081234567890", "passwordbudi", "budi", new Guid("11111111-1111-1111-1111-111111111111"), 0 },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Jl. Sudirman No 2", new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Siti Aminah", "082345678901", "passwordsiti", "siti", new Guid("22222222-2222-2222-2222-222222222222"), 0 },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Jl. Thamrin No 3", new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Agus Pratama", "083456789012", "passwordagus", "agus", new Guid("11111111-1111-1111-1111-111111111111"), 1 },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Jl. Gatot Subroto No 4", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dewi Lestari", "084567890123", "passworddewi", "dewi", new Guid("33333333-3333-3333-3333-333333333333"), 0 },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Jl. Ahmad Yani No 5", new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Joko Widodo", "085678901234", "passwordjoko", "joko", new Guid("22222222-2222-2222-2222-222222222222"), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ServicePlanId",
                table: "Customers",
                column: "ServicePlanId");

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
