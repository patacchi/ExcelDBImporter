using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class TableAliasClassNameRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrClassName",
                table: "TableFieldAliasNameLists");

            migrationBuilder.DropColumn(
                name: "StrColumnName",
                table: "TableFieldAliasNameLists");

            migrationBuilder.AlterColumn<string>(
                name: "StrColnmnAliasName",
                table: "TableFieldAliasNameLists",
                type: "TEXT",
                nullable: true,
                comment: "列の表示用別名",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<int>(
                name: "TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                comment: "DB columnnameテーブルの外部キー")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.CreateIndex(
                name: "IX_TableFieldAliasNameLists_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists",
                column: "TableDBcolumnNameAndExcelFieldNameID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TableFieldAliasNameLists_tableDBcolumnNameAndExcelFieldNames_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists",
                column: "TableDBcolumnNameAndExcelFieldNameID",
                principalTable: "tableDBcolumnNameAndExcelFieldNames",
                principalColumn: "TableDBcolumnNameAndExcelFieldNameID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableFieldAliasNameLists_tableDBcolumnNameAndExcelFieldNames_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists");

            migrationBuilder.DropIndex(
                name: "IX_TableFieldAliasNameLists_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists");

            migrationBuilder.DropColumn(
                name: "TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists");

            migrationBuilder.AlterColumn<string>(
                name: "StrColnmnAliasName",
                table: "TableFieldAliasNameLists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "列の表示用別名")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AddColumn<string>(
                name: "StrClassName",
                table: "TableFieldAliasNameLists",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<string>(
                name: "StrColumnName",
                table: "TableFieldAliasNameLists",
                type: "TEXT",
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 2);
        }
    }
}
