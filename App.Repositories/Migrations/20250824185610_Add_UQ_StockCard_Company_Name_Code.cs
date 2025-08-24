using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Add_UQ_StockCard_Company_Name_Code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockCards_Code",
                table: "StockCards");

            migrationBuilder.DropIndex(
                name: "IX_StockCards_CompanyId",
                table: "StockCards");

            migrationBuilder.CreateIndex(
                name: "IX_StockCards_CompanyId_Code",
                table: "StockCards",
                columns: new[] { "CompanyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockCards_CompanyId_Name",
                table: "StockCards",
                columns: new[] { "CompanyId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockCards_CompanyId_Code",
                table: "StockCards");

            migrationBuilder.DropIndex(
                name: "IX_StockCards_CompanyId_Name",
                table: "StockCards");

            migrationBuilder.CreateIndex(
                name: "IX_StockCards_Code",
                table: "StockCards",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockCards_CompanyId",
                table: "StockCards",
                column: "CompanyId");
        }
    }
}
