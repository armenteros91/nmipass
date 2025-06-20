﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeTP.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Payment");

            migrationBuilder.EnsureSchema(
                name: "Tenant");

            migrationBuilder.EnsureSchema(
                name: "Logging");

            migrationBuilder.CreateTable(
                name: "PlTblTransactionsType",
                schema: "Payment",
                columns: table => new
                {
                    TypeTransactionsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)"),
                    CreatedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true, defaultValueSql: "USER_NAME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlTblTransactionsType", x => x.TypeTransactionsId);
                });

            migrationBuilder.CreateTable(
                name: "TblNmiTransactionRequestLog",
                schema: "Logging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)"),
                    CreatedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true, defaultValueSql: "USER_NAME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblNmiTransactionRequestLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TblTenant",
                schema: "Tenant",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)"),
                    CreatedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true, defaultValueSql: "USER_NAME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblTenant", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "TblTransactionResponse",
                schema: "Payment",
                columns: table => new
                {
                    TransaccionResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    Response = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    ResponseText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuthCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvsResponse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CvvResponse = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResponseCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EmvAuthResponseData = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CustomerVaultId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KountScore = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MerchantAdviceCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblTransactionResponse", x => x.TransaccionResponseId);
                });

            migrationBuilder.CreateTable(
                name: "TblNmiTransactionResponseLog",
                schema: "Logging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)"),
                    CreatedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true, defaultValueSql: "USER_NAME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblNmiTransactionResponseLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblNmiTransactionResponseLog_TblNmiTransactionRequestLog_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "Logging",
                        principalTable: "TblNmiTransactionRequestLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblApiKey",
                schema: "Tenant",
                columns: table => new
                {
                    TenantApikeyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiKeyValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)"),
                    CreatedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true, defaultValueSql: "USER_NAME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblApiKey", x => x.TenantApikeyId);
                    table.ForeignKey(
                        name: "FK_TblApiKey_TblTenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Tenant",
                        principalTable: "TblTenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblTerminalTenants",
                schema: "Tenant",
                columns: table => new
                {
                    TerminalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecretKeyEncrypted = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecretKeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)"),
                    CreatedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true, defaultValueSql: "USER_NAME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblTerminalTenants", x => x.TerminalId);
                    table.ForeignKey(
                        name: "FK_TblTerminalTenants_TblTenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Tenant",
                        principalTable: "TblTenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TblTransactions",
                schema: "Payment",
                columns: table => new
                {
                    TransactionsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeTransaction = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponseCode = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CONVERT(DATETIME2, DATEADD(HOUR, -5, GETDATE()), 120)"),
                    CreatedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true, defaultValueSql: "USER_NAME()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblTransactions", x => x.TransactionsId);
                    table.ForeignKey(
                        name: "FK_TblTransactions_PlTblTransactionsType_TypeTransaction",
                        column: x => x.TypeTransaction,
                        principalSchema: "Payment",
                        principalTable: "PlTblTransactionsType",
                        principalColumn: "TypeTransactionsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TblTransactions_TblTenant_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "Tenant",
                        principalTable: "TblTenant",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblApiKey_TenantId",
                schema: "Tenant",
                table: "TblApiKey",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TblNmiTransactionResponseLog_RequestId",
                schema: "Logging",
                table: "TblNmiTransactionResponseLog",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IDX_TblTerminalTenants_TenantId",
                schema: "Tenant",
                table: "TblTerminalTenants",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IDX_Terminal_SecretHash",
                schema: "Tenant",
                table: "TblTerminalTenants",
                column: "SecretKeyHash");

            migrationBuilder.CreateIndex(
                name: "UQ_Tenant_SecretKey",
                schema: "Tenant",
                table: "TblTerminalTenants",
                columns: new[] { "TenantId", "SecretKeyEncrypted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblTransactions_TenantId",
                schema: "Payment",
                table: "TblTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TblTransactions_TypeTransaction",
                schema: "Payment",
                table: "TblTransactions",
                column: "TypeTransaction");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblApiKey",
                schema: "Tenant");

            migrationBuilder.DropTable(
                name: "TblNmiTransactionResponseLog",
                schema: "Logging");

            migrationBuilder.DropTable(
                name: "TblTerminalTenants",
                schema: "Tenant");

            migrationBuilder.DropTable(
                name: "TblTransactionResponse",
                schema: "Payment");

            migrationBuilder.DropTable(
                name: "TblTransactions",
                schema: "Payment");

            migrationBuilder.DropTable(
                name: "TblNmiTransactionRequestLog",
                schema: "Logging");

            migrationBuilder.DropTable(
                name: "PlTblTransactionsType",
                schema: "Payment");

            migrationBuilder.DropTable(
                name: "TblTenant",
                schema: "Tenant");
        }
    }
}
