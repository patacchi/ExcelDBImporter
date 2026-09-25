using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class TQRinputaddIsCompiled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrTagBarcode",
                table: "ViewMarsharing");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateInputDate",
                table: "TQRinput",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "入力日時、作業開始日時として使用",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "入力日時、作業開始日時として使用");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompiled",
                table: "TQRinput",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                comment: "ViewMarsharingテーブルに登録済みフラグ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompiled",
                table: "TQRinput");

            migrationBuilder.AddColumn<string>(
                name: "StrTagBarcode",
                table: "ViewMarsharing",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateInputDate",
                table: "TQRinput",
                type: "TEXT",
                nullable: true,
                comment: "入力日時、作業開始日時として使用",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "入力日時、作業開始日時として使用");
        }
    }
}
