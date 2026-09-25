using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class QRinputTable_Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TQRinput",
                columns: table => new
                {
                    TQRinputId = table.Column<int>(type: "INTEGER", nullable: false, comment: "QRテーブルのキー")
                        .Annotation("Sqlite:Autoincrement", true),
                    DateInputDate = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "入力日時、作業開始日時として使用"),
                    DateToDate = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "終了日時、基本的に自動計算で入力する"),
                    QROPcode = table.Column<uint>(type: "INTEGER", nullable: false, comment: "行程種別のenum。初期値 None")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TQRinput", x => x.TQRinputId);
                },
                comment: "工程管理用 QRコード記録テーブル");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TQRinput");
        }
    }
}
