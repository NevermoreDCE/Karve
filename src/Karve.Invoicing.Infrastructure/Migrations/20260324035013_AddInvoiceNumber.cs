using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Karve.Invoicing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_CompanyId",
                table: "invoices");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceNumber",
                table: "invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                WITH numbered AS (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER (PARTITION BY CompanyId ORDER BY InvoiceDate, Id) AS InvoiceNumber
                    FROM invoices
                )
                UPDATE invoices
                SET InvoiceNumber = (
                    SELECT numbered.InvoiceNumber
                    FROM numbered
                    WHERE numbered.Id = invoices.Id
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_CompanyId_InvoiceNumber",
                table: "invoices",
                columns: new[] { "CompanyId", "InvoiceNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_CompanyId_InvoiceNumber",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "invoices");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_CompanyId",
                table: "invoices",
                column: "CompanyId");
        }
    }
}
