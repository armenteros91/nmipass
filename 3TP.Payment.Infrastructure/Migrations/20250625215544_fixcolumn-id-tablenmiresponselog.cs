using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeTP.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixcolumnidtablenmiresponselog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "Logging",
                table: "TblNmiTransactionResponseLog",
                newName: "NmiTransactionResponseLogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NmiTransactionResponseLogId",
                schema: "Logging",
                table: "TblNmiTransactionResponseLog",
                newName: "Id");
        }
    }
}
