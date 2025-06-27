using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeTP.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addAuditmennbertransactionsresponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Payment",
                table: "TblTransactionResponse",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true,
                defaultValueSql: "USER_NAME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "Payment",
                table: "TblTransactionResponse",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)");

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                schema: "Payment",
                table: "TblTransactionResponse",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                schema: "Payment",
                table: "TblTransactionResponse",
                type: "datetime2",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "TimeStamp",
                schema: "Payment",
                table: "TblTransactionResponse",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Payment",
                table: "TblTransactionResponse");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Payment",
                table: "TblTransactionResponse");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "Payment",
                table: "TblTransactionResponse");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                schema: "Payment",
                table: "TblTransactionResponse");

            migrationBuilder.DropColumn(
                name: "TimeStamp",
                schema: "Payment",
                table: "TblTransactionResponse");
        }
    }
}
