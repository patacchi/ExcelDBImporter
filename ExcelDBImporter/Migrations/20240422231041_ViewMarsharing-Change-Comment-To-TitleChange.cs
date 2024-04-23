using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class ViewMarsharingChangeCommentToTitleChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IntShipping",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "支給品準備\r\n払出",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "支給品準備・払出");

            migrationBuilder.AlterColumn<int>(
                name: "IntPrepareReceive",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "棚入れ前\r\n準備作業",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "棚入れ前準備作業");

            migrationBuilder.AlterColumn<int>(
                name: "IntMoving",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "運搬回数\r\n倉庫内移動",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "倉庫内移動");

            migrationBuilder.AlterColumn<int>(
                name: "IntMicroWave",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "マイクロ波\r\n払出",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "マイクロ波払出");

            migrationBuilder.AlterColumn<int>(
                name: "IntFreewayData",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "Freeway\r\nデータ処理",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "Freewayデータ処理");

            migrationBuilder.AlterColumn<int>(
                name: "IntCableCut",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "ケーブル\r\n切断作業",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "ケーブル切断作業");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatePerDay",
                table: "ViewMarsharing",
                type: "TEXT",
                nullable: true,
                comment: "日付",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "集計した日付");

            migrationBuilder.AlterColumn<string>(
                name: "StrStockCode",
                table: "ShInOut",
                type: "TEXT",
                nullable: true,
                comment: "貯蔵記号",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "手配機種");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IntShipping",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "支給品準備・払出",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "支給品準備\r\n払出");

            migrationBuilder.AlterColumn<int>(
                name: "IntPrepareReceive",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "棚入れ前準備作業",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "棚入れ前\r\n準備作業");

            migrationBuilder.AlterColumn<int>(
                name: "IntMoving",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "倉庫内移動",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "運搬回数\r\n倉庫内移動");

            migrationBuilder.AlterColumn<int>(
                name: "IntMicroWave",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "マイクロ波払出",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "マイクロ波\r\n払出");

            migrationBuilder.AlterColumn<int>(
                name: "IntFreewayData",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "Freewayデータ処理",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "Freeway\r\nデータ処理");

            migrationBuilder.AlterColumn<int>(
                name: "IntCableCut",
                table: "ViewMarsharing",
                type: "INTEGER",
                nullable: true,
                comment: "ケーブル切断作業",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true,
                oldComment: "ケーブル\r\n切断作業");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatePerDay",
                table: "ViewMarsharing",
                type: "TEXT",
                nullable: true,
                comment: "集計した日付",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "日付");

            migrationBuilder.AlterColumn<string>(
                name: "StrStockCode",
                table: "ShInOut",
                type: "TEXT",
                nullable: true,
                comment: "手配機種",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "貯蔵記号");
        }
    }
}
