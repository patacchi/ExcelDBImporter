using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    /// <inheritdoc />
    public partial class AppSettingTableAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    AppSettingID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StrAppName = table.Column<string>(type: "TEXT", nullable: false, comment: "アプリ名、必須"),
                    StrLastLoadFromDir = table.Column<string>(type: "TEXT", nullable: true, comment: "最終読み取りディレクトリ"),
                    StrLastSaveToDir = table.Column<string>(type: "TEXT", nullable: true, comment: "最終保存ディレクトリ")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.AppSettingID);
                },
                comment: "各アプリの設定を格納する。1アプリ１レコード");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
