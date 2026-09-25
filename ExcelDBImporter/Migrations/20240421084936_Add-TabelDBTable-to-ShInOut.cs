using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelDBTabletoShInOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StrShInOutFieldName",
                table: "TableDBcolumnNameAndExcelFieldName",
                type: "TEXT",
                nullable: true,
                comment: "入出庫履歴(CSV)のフィールド名")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.CreateTable(
                name: "ShInOut",
                columns: table => new
                {
                    ShInOutID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateInOut = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "入出庫日"),
                    StrOrderOrSeiban = table.Column<string>(type: "TEXT", nullable: true, comment: "オーダーか製番、役に立たなそう"),
                    StrKanriKa = table.Column<string>(type: "TEXT", nullable: true, comment: "管理課"),
                    StrTehaiCode = table.Column<string>(type: "TEXT", nullable: false, comment: "手配コード"),
                    DblInputNum = table.Column<double>(type: "REAL", nullable: true, comment: "入庫個数"),
                    DblDeliverNum = table.Column<double>(type: "REAL", nullable: true, comment: "出庫個数"),
                    StrKishu = table.Column<string>(type: "TEXT", nullable: true, comment: "手配機種"),
                    StrStockCode = table.Column<string>(type: "TEXT", nullable: true, comment: "手配機種"),
                    DblRemainAmount = table.Column<double>(type: "REAL", nullable: true, comment: "在庫数量")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShInOut", x => x.ShInOutID);
                },
                comment: "入出庫履歴を格納するテーブル");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShInOut");

            migrationBuilder.DropColumn(
                name: "StrShInOutFieldName",
                table: "TableDBcolumnNameAndExcelFieldName");
        }
    }
}
