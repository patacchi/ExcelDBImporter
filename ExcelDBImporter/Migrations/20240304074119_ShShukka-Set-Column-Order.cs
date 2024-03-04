using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class ShShukkaSetColumnOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StrSeiban",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "製番",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "製番")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "StrOrderFrom",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "注文主",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "注文主")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "StrKishu",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "機種",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "機種")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "StrHinmei",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "品名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "品名")
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "IntAmount",
                table: "ExcelData",
                type: "INTEGER",
                nullable: false,
                comment: "発番数量",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "発番数量")
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShutuzu",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "出図日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "出図日")
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShukka",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "出荷計画",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "出荷計画")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShippingTest",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "出荷検査日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "出荷検査日")
                .Annotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatePrepare",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "出荷準備",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "出荷準備")
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateMarshalling",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "マーシャリング日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "マーシャリング日")
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateFunctionTest",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "試験日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "試験日")
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAssemble",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "組立日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "組立日")
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "ShShukkaID",
                table: "ExcelData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 0)
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StrSeiban",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "製番",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "製番")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "StrOrderFrom",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "注文主",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "注文主")
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "StrKishu",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "機種",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "機種")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "StrHinmei",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "品名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "品名")
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<int>(
                name: "IntAmount",
                table: "ExcelData",
                type: "INTEGER",
                nullable: false,
                comment: "発番数量",
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldComment: "発番数量")
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShutuzu",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "出図日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "出図日")
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShukka",
                table: "ExcelData",
                type: "TEXT",
                nullable: false,
                comment: "出荷計画",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldComment: "出荷計画")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateShippingTest",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "出荷検査日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "出荷検査日")
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatePrepare",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "出荷準備",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "出荷準備")
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateMarshalling",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "マーシャリング日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "マーシャリング日")
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateFunctionTest",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "試験日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "試験日")
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateAssemble",
                table: "ExcelData",
                type: "TEXT",
                nullable: true,
                comment: "組立日",
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "組立日")
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "ShShukkaID",
                table: "ExcelData",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Sqlite:Autoincrement", true);
        }
    }
}
