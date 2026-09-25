using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class ViewMarsharingTableAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompiled",
                table: "TQRinput",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                comment: "集計されたかどうかを示す");

            migrationBuilder.CreateTable(
                name: "ViewMarsharing",
                columns: table => new
                {
                    ViewMarsharingID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DatePerDay = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "集計した日付"),
                    IntPrepareReceive = table.Column<int>(type: "INTEGER", nullable: true, comment: "棚入れ前準備作業"),
                    IntFreewayData = table.Column<int>(type: "INTEGER", nullable: true, comment: "Freewayデータ処理"),
                    IntDelivery = table.Column<int>(type: "INTEGER", nullable: true, comment: "出庫作業"),
                    IntShipping = table.Column<int>(type: "INTEGER", nullable: true, comment: "支給品準備・払出"),
                    IntMicroWave = table.Column<int>(type: "INTEGER", nullable: true, comment: "マイクロ波払出"),
                    IntCableCut = table.Column<int>(type: "INTEGER", nullable: true, comment: "ケーブル切断作業"),
                    IntMoving = table.Column<int>(type: "INTEGER", nullable: true, comment: "倉庫内移動"),
                    IntOther = table.Column<int>(type: "INTEGER", nullable: true, comment: "その他作業")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewMarsharing", x => x.ViewMarsharingID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ViewMarsharing");

            migrationBuilder.DropColumn(
                name: "IsCompiled",
                table: "TQRinput");
        }
    }
}
