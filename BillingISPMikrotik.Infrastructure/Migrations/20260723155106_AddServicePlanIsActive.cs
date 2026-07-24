using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingISPMikrotik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePlanIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ServicePlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "ServicePlans",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ServicePlans",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "ServicePlans",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "IsActive",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ServicePlans");
        }
    }
}
