using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberShield.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlansWithLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentPrice",
                table: "ProtectionPlans");

            migrationBuilder.AlterColumn<string>(
                name: "PlanName",
                table: "ProtectionPlans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "ProtectionPlans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "HasAdvancedEmailVerification",
                table: "ProtectionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDedicatedAccountManager",
                table: "ProtectionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxDevicesAllowed",
                table: "ProtectionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxFilesPerMonth",
                table: "ProtectionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UnlimitedLinkScanning",
                table: "ProtectionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAdvancedEmailVerification",
                table: "ProtectionPlans");

            migrationBuilder.DropColumn(
                name: "HasDedicatedAccountManager",
                table: "ProtectionPlans");

            migrationBuilder.DropColumn(
                name: "MaxDevicesAllowed",
                table: "ProtectionPlans");

            migrationBuilder.DropColumn(
                name: "MaxFilesPerMonth",
                table: "ProtectionPlans");

            migrationBuilder.DropColumn(
                name: "UnlimitedLinkScanning",
                table: "ProtectionPlans");

            migrationBuilder.AlterColumn<string>(
                name: "PlanName",
                table: "ProtectionPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyName",
                table: "ProtectionPlans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentPrice",
                table: "ProtectionPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
