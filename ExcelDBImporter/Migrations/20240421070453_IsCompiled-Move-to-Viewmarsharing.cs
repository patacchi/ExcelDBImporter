using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class IsCompiledMovetoViewmarsharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompiled",
                table: "TQRinput");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompiled",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                comment: "集計されたかどうかを示す");

            migrationBuilder.AddColumn<string>(
                name: "StrTagBarcode",
                table: "ViewMarsharing",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompiled",
                table: "ViewMarsharing");

            migrationBuilder.DropColumn(
                name: "StrTagBarcode",
                table: "ViewMarsharing");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompiled",
                table: "TQRinput",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                comment: "集計されたかどうかを示す");
        }
    }
}
