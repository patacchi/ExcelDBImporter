using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class ShShukkaAddoutputflag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAlreadyOutput",
                table: "ShShukka",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                comment: "データ出力済みフラグ")
                .Annotation("Relational:ColumnOrder", 13);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAlreadyOutput",
                table: "ShShukka");
        }
    }
}
