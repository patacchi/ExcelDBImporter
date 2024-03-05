using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class ShShukkatablenamechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ExcelData",
                table: "ExcelData");

            migrationBuilder.RenameTable(
                name: "ExcelData",
                newName: "ShShukka");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShShukka",
                table: "ShShukka",
                column: "ShShukkaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ShShukka",
                table: "ShShukka");

            migrationBuilder.RenameTable(
                name: "ShShukka",
                newName: "ExcelData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExcelData",
                table: "ExcelData",
                column: "ShShukkaID");
        }
    }
}
