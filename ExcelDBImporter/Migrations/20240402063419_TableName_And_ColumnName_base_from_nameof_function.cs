using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class TableName_And_ColumnName_base_from_nameof_function : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableFieldAliasNameLists_tableDBcolumnNameAndExcelFieldNames_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TableFieldAliasNameLists",
                table: "TableFieldAliasNameLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tableDBcolumnNameAndExcelFieldNames",
                table: "tableDBcolumnNameAndExcelFieldNames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppSettings",
                table: "AppSettings");

            migrationBuilder.RenameTable(
                name: "TableFieldAliasNameLists",
                newName: "TableFieldAliasNameList");

            migrationBuilder.RenameTable(
                name: "tableDBcolumnNameAndExcelFieldNames",
                newName: "TableDBcolumnNameAndExcelFieldName");

            migrationBuilder.RenameTable(
                name: "AppSettings",
                newName: "AppSetting");

            migrationBuilder.RenameIndex(
                name: "IX_TableFieldAliasNameLists_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameList",
                newName: "IX_TableFieldAliasNameList_TableDBcolumnNameAndExcelFieldNameID");

            migrationBuilder.AlterColumn<string>(
                name: "StrLastSaveToDir",
                table: "AppSetting",
                type: "TEXT",
                nullable: true,
                comment: "最終保存ディレクトリ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "最終保存ディレクトリ")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "StrLastLoadFromDir",
                table: "AppSetting",
                type: "TEXT",
                nullable: true,
                comment: "最終読み取りディレクトリ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "最終読み取りディレクトリ")
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "StrAppName",
                table: "AppSetting",
                type: "TEXT",
                nullable: false,
                comment: "アプリ名、必須",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "アプリ名、必須")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "AppSettingID",
                table: "AppSetting",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 0)
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableFieldAliasNameList",
                table: "TableFieldAliasNameList",
                column: "TableFieldAliasNameListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableDBcolumnNameAndExcelFieldName",
                table: "TableDBcolumnNameAndExcelFieldName",
                column: "TableDBcolumnNameAndExcelFieldNameID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppSetting",
                table: "AppSetting",
                column: "AppSettingID");

            migrationBuilder.AddForeignKey(
                name: "FK_TableFieldAliasNameList_TableDBcolumnNameAndExcelFieldName_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameList",
                column: "TableDBcolumnNameAndExcelFieldNameID",
                principalTable: "TableDBcolumnNameAndExcelFieldName",
                principalColumn: "TableDBcolumnNameAndExcelFieldNameID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TableFieldAliasNameList_TableDBcolumnNameAndExcelFieldName_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TableFieldAliasNameList",
                table: "TableFieldAliasNameList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TableDBcolumnNameAndExcelFieldName",
                table: "TableDBcolumnNameAndExcelFieldName");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppSetting",
                table: "AppSetting");

            migrationBuilder.RenameTable(
                name: "TableFieldAliasNameList",
                newName: "TableFieldAliasNameLists");

            migrationBuilder.RenameTable(
                name: "TableDBcolumnNameAndExcelFieldName",
                newName: "tableDBcolumnNameAndExcelFieldNames");

            migrationBuilder.RenameTable(
                name: "AppSetting",
                newName: "AppSettings");

            migrationBuilder.RenameIndex(
                name: "IX_TableFieldAliasNameList_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists",
                newName: "IX_TableFieldAliasNameLists_TableDBcolumnNameAndExcelFieldNameID");

            migrationBuilder.AlterColumn<string>(
                name: "StrLastSaveToDir",
                table: "AppSettings",
                type: "TEXT",
                nullable: true,
                comment: "最終保存ディレクトリ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "最終保存ディレクトリ")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "StrLastLoadFromDir",
                table: "AppSettings",
                type: "TEXT",
                nullable: true,
                comment: "最終読み取りディレクトリ",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldComment: "最終読み取りディレクトリ")
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<string>(
                name: "StrAppName",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                comment: "アプリ名、必須",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldComment: "アプリ名、必須")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<int>(
                name: "AppSettingID",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true)
                .OldAnnotation("Relational:ColumnOrder", 0)
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TableFieldAliasNameLists",
                table: "TableFieldAliasNameLists",
                column: "TableFieldAliasNameListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tableDBcolumnNameAndExcelFieldNames",
                table: "tableDBcolumnNameAndExcelFieldNames",
                column: "TableDBcolumnNameAndExcelFieldNameID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppSettings",
                table: "AppSettings",
                column: "AppSettingID");

            migrationBuilder.AddForeignKey(
                name: "FK_TableFieldAliasNameLists_tableDBcolumnNameAndExcelFieldNames_TableDBcolumnNameAndExcelFieldNameID",
                table: "TableFieldAliasNameLists",
                column: "TableDBcolumnNameAndExcelFieldNameID",
                principalTable: "tableDBcolumnNameAndExcelFieldNames",
                principalColumn: "TableDBcolumnNameAndExcelFieldNameID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
