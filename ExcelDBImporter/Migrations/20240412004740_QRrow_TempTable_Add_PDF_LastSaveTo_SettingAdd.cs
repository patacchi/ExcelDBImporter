using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class QRrow_TempTable_Add_PDF_LastSaveTo_SettingAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StrLastPDFSaveToDir",
                table: "AppSetting",
                type: "TEXT",
                nullable: true,
                comment: "PDFファイル最終保存ディレクトリ")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.CreateTable(
                name: "TTempQRrowData",
                columns: table => new
                {
                    TTempQRrowDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateInputDate = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "入力日時"),
                    StrRowQRcodeData = table.Column<string>(type: "TEXT", nullable: false, comment: "QR(バーコード)の読み取り結果の生データをそのまま格納する")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TTempQRrowData", x => x.TTempQRrowDataId);
                },
                comment: "QRコードを読み取った生のデータを一時保存しておくテーブル。入力日時をキーとする。");

            migrationBuilder.CreateIndex(
                name: "IX_TTempQRrowData_DateInputDate",
                table: "TTempQRrowData",
                column: "DateInputDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TTempQRrowData");

            migrationBuilder.DropColumn(
                name: "StrLastPDFSaveToDir",
                table: "AppSetting");
        }
    }
}
