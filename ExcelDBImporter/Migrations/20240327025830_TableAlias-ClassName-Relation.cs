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
            //AliasテーブルからStrClassNameとStrColumnName列を消去する前に、未登録のものがあればDBcolumnテーブルに追記
            migrationBuilder.Sql(@"
            --Aliasテーブルに新規レコードがあればDBcolumnテーブルに転記(ShShukkaクラスのみ)
            --dbmasterとして、クラス名、カラム名をキーとしてAliasテーブルとDBcolumテーブルを外部結合(SQliteの制限で左外部結合しかできない)
            --結合したうえで、クラス名がShShukkaの物で、なおかつDBcolumnに登録がないもの(新規追加すべきもの)のみ選択
            WITH dbmaster as (
            SELECT 
	            a.StrClassName,
	            a.StrColumnName
            FROM TableFieldAliasNameLists as a 
            LEFT OUTER JOIN tableDBcolumnNameAndExcelFieldNames as db
	            on a.StrClassName = db.StrClassName AND a.StrColumnName = db.StrDBColumnName 
            WHERE
	            db.TableDBcolumnNameAndExcelFieldNameID is NULL
	            and a.StrClassName = 'ShShukka'
            )
            --抽出した行をDBcolumnテーブルに挿入
            INSERT INTO tableDBcolumnNameAndExcelFieldNames
            (
	            StrClassName,
	            StrDBColumnName
            )
	            SELECT 
		            dbmaster.StrClassName,
		            dbmaster.StrColumnName
	            FROM dbmaster 
	            --ONがJoinの物かどうか区別がつかなくなるため、where trueは必須
	            WHERE true
            --コンフリクトが起きた時は明示的に何もしない
            ON CONFLICT (TableDBcolumnNameAndExcelFieldNameID) do NOTHING;
            ");
            //追記(転記)完了したら列削除
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
            //UniqueIndexを作成する前に外部キー列に一意の値を入れておかないと、デフォルト値(0)が並んだ時点でUnique制約違反が発生してしまう
            //そのため、ここでDBcolumnテーブルに存在するクラス名とカラム名の組み合わせのものがあれば、そのIDを追記
            //組み合わせがないものはAliasテーブルから削除してしまう
            //Aliasテーブルに外部キーのID値反映
            migrationBuilder.Sql(@"
            --Aliasテーブルに外部キーのIDを反映させる
            --AliasテーブルとDBcolumnテーブルでinner join
            WITH dbcolumn as (
            SELECT 
	            dbc.StrClassName,
	            dbc.StrDBColumnName,
	            dbc.TableDBcolumnNameAndExcelFieldNameID as ID
            FROM tableDBcolumnNameAndExcelFieldNames as dbc
	            INNER JOIN TableFieldAliasNameLists as a
	            ON dbc.StrClassName = a.StrClassName AND dbc.StrDBColumnName = a.StrColumnName
            )
            --AliasテーブルにDBcolumnテーブルのID値をセット
            UPDATE TableFieldAliasNameLists as ali
            SET
	            TableDBcolumnNameAndExcelFieldNameID = dbcolumn.ID
            FROM dbcolumn 
            WHERE
	            ali.StrClassName = dbcolumn.StrClassName
	            AND ali.StrColumnName = dbcolumn.StrDBColumnName;
            ");
            //外部キーの設定できないAliasテーブルの行を削除
            migrationBuilder.Sql(@"
            --DBcolumnに外部キーが存在しないレコードを削除
            DELETE 
            FROM TableFieldAliasNameLists as ali 
            WHERE ROWID IN (
	            SELECT ali.ROWID
	            FROM TableFieldAliasNameLists as ali
	            LEFT JOIN tableDBcolumnNameAndExcelFieldNames as dbcolumn
	            ON ali.TableDBcolumnNameAndExcelFieldNameID = dbcolumn.TableDBcolumnNameAndExcelFieldNameID
	            WHERE dbcolumn.TableDBcolumnNameAndExcelFieldNameID IS NULL);
            ");
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
