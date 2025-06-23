using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeTP.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class justoneterminal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_TblTerminalTenants_TenantId",
                schema: "Tenant",
                table: "TblTerminalTenants");

            migrationBuilder.DropIndex(
                name: "UQ_Tenant_SecretKey",
                schema: "Tenant",
                table: "TblTerminalTenants");

            migrationBuilder.CreateIndex(
                name: "UQ_TblTerminalTenants_TenantId",
                schema: "Tenant",
                table: "TblTerminalTenants",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Terminal_SecretKeyEncrypted",
                schema: "Tenant",
                table: "TblTerminalTenants",
                column: "SecretKeyEncrypted",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_TblTerminalTenants_TenantId",
                schema: "Tenant",
                table: "TblTerminalTenants");

            migrationBuilder.DropIndex(
                name: "UQ_Terminal_SecretKeyEncrypted",
                schema: "Tenant",
                table: "TblTerminalTenants");

            migrationBuilder.CreateIndex(
                name: "IDX_TblTerminalTenants_TenantId",
                schema: "Tenant",
                table: "TblTerminalTenants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ_Tenant_SecretKey",
                schema: "Tenant",
                table: "TblTerminalTenants",
                columns: new[] { "TenantId", "SecretKeyEncrypted" },
                unique: true);
        }
    }
}
