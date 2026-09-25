# ExcelDBImporter

Windows Forms デスクトップアプリ。Excel/CSV → SQLite DB のインポート、出荷伝票の QR コード生成・PDF 出力が主機能。

- 単一プロジェクト構成: `ExcelDBImporter/`（sln はリポジトリ直下 `ExcelDBImporter.sln`）
- 名前空間はすべて `ExcelDBImporter`（フォルダと一致しない場合もある）
- DB: EF Core + SQLite。`Context/ExcelDbContext.cs` に全 DbSet。DB ファイルは `Application.StartupPath\excel_data.db`（実行出力ディレクトリ直下。リポジトリの bin/Debug 配下に実体がある）
- スキーマ変更は `Migrations/` 配下の EF マイグレーション経由（手書き SQL 変更禁止）
- 主要ドメインモデル: `Models/shShukka.cs`（出荷）、`ShInOut.cs`、`TQRinput.cs`、`AppSetting.cs`、`TableFieldAliasNameList.cs`（列別名）
- `Tool/` に補助クラス（PdfCreator、QRcodeCreate、SerialCommunication 等）
- フォーム: `FrmExcelImpoerter.cs`（※ファイル名に綴り誤り Impoerter のまま。リネーム時は Designer.cs / resx も要確認）ほか FrmDataViewer、FrmPrintQRCode、FrmQRread
- `ExcelDBImporter/View/**` は csproj の Compile Remove でビルド対象外（死んだコードと誤って編集しないこと）
- `Sample/` は入出力サンプル CSV、`Tools/qr.cs` は本体と無関係の単発スクリプト
- `.github/agents` は git submodule（patacchi/commonAgent-vscode）

技術スタック: `mem:tech_stack`。ビルド/実行コマンド: `mem:suggested_commands`。完了条件: `mem:task_completion`。