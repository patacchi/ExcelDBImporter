using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class TQRinputTableTagandOrdernumAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StrOrderNum",
                table: "TQRinput",
                type: "TEXT",
                nullable: true,
                comment: "オーダーNo 主に入庫の時に使う");

            migrationBuilder.AddColumn<string>(
                name: "StrTagBarcode",
                table: "TQRinput",
                type: "TEXT",
                nullable: true,
                comment: "TAGの下にあるバーコード。とりあえず読み取った結果そのまま格納する");

            migrationBuilder.AlterColumn<string>(
                name: "StrOrderFrom",
                table: "ShShukka",
                type: "TEXT",
                nullable: true,
                comment: "注文主",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "注文主");

            migrationBuilder.AlterColumn<string>(
                name: "StrHinmei",
                table: "ShShukka",
                type: "TEXT",
                nullable: true,
                comment: "品名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "品名");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShukka",
                table: "ShShukka",
                type: "TEXT",
                nullable: true,
                comment: "出荷計画",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "出荷計画");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrOrderNum",
                table: "TQRinput");

            migrationBuilder.DropColumn(
                name: "StrTagBarcode",
                table: "TQRinput");

            migrationBuilder.AlterColumn<string>(
                name: "StrOrderFrom",
                table: "ShShukka",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                comment: "注文主",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "注文主");

            migrationBuilder.AlterColumn<string>(
                name: "StrHinmei",
                table: "ShShukka",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                comment: "品名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "品名");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShukka",
                table: "ShShukka",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "出荷計画",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "出荷計画");
        }
    }
}
