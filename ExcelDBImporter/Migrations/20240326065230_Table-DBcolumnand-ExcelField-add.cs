using DocumentFormat.OpenXml.Office2021.PowerPoint.Comment;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class TableDBcolumnandExcelFieldadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tableDBcolumnNameAndExcelFieldNames",
                columns: table => new
                {
                    TableDBcolumnNameAndExcelFieldNameID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StrClassName = table.Column<string>(type: "TEXT", nullable: false, comment: "格納されているモデルクラス名"),
                    StrDBColumnName = table.Column<string>(type: "TEXT", nullable: false, comment: "モデルクラスのオリジナルDBColumn名"),
                    StrshShukkaFieldName = table.Column<string>(type: "TEXT", nullable: true, comment: "出荷物件予定表Excelファイルのフィールド名、4行目と5行目の文字列を連結する"),
                    StrshProcessManagementFieldName = table.Column<string>(type: "TEXT", nullable: true, comment: "工程管理システムからの出力Excelファイルのフィールド名")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tableDBcolumnNameAndExcelFieldNames", x => x.TableDBcolumnNameAndExcelFieldNameID);
                },
                comment: "DBとExcelファイルのフィールド名の対応格納テーブル。対応Excelファイルが増えると列が増えていく");
            //ここまでにAliasテーブルにデータ追加していると、クラス名とDBcolumnがDBcolumnテーブルに格納するが、
            //Aliasテーブルが外部キー(依存側)なので、新規作成したReference(親)側のテーブルに合わせるとAliasテーブルのデータが消えてしまう
            //そのため、Aliasテーブルにクラス名とDBcolumnの関係があったら DBcolumnテーブルに転記をする
            //(この時にIDがオートインクリメントで付加されるので外部キーとして機能するようになり、次のマイグレーションで活用できる)
            migrationBuilder.Sql(@"
            INSERT INTO ""tableDBcolumnNameAndExcelFieldNames"" ( ""StrClassName"",""StrDBColumnName"") 
            SELECT ""StrClassName"",""StrColumnName""
            FROM ""TableFieldAliasNameLists""
            WHERE ""StrClassName"" = ""ShShukka"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tableDBcolumnNameAndExcelFieldNames");
        }
    }
}
