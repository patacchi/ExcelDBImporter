using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcelData",
                columns: table => new
                {
                    ShShukkaID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateShukka = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "出荷計画"),
                    StrSeiban = table.Column<string>(type: "TEXT", nullable: false, comment: "製番"),
                    StrOrderFrom = table.Column<string>(type: "TEXT", nullable: false, comment: "注文主"),
                    StrHinmei = table.Column<string>(type: "TEXT", nullable: false, comment: "品名"),
                    IntAmount = table.Column<int>(type: "INTEGER", nullable: false, comment: "発番数量"),
                    DateShutuzu = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "出図日"),
                    DateMarshalling = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "マーシャリング日"),
                    DateAssemble = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "組立日"),
                    DateFunctionTest = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "試験日"),
                    DatePrepare = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "出荷準備"),
                    DateShippingTest = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "出荷検査日")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelData", x => x.ShShukkaID);
                },
                comment: "発番出荷物件予定表モデルクラス");

            migrationBuilder.CreateTable(
                name: "TableFieldAliasNameLists",
                columns: table => new
                {
                    TableFieldAliasNameListId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StrClassName = table.Column<string>(type: "TEXT", nullable: false),
                    StrColumnName = table.Column<string>(type: "TEXT", nullable: false),
                    StrColnmnAliasName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableFieldAliasNameLists", x => x.TableFieldAliasNameListId);
                },
                comment: "テーブル列名の別名(表示名等)格納テーブル");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelData");

            migrationBuilder.DropTable(
                name: "TableFieldAliasNameLists");
        }
    }
}
