# Suggested commands

リポジトリルート = ワークスペースルート（`ExcelDBImporter.sln` 直下）。

- ビルド（COM 参照のため `dotnet build` は MSB4803 で失敗。VS 2022 の MSBuild を使う）:
  `"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ExcelDBImporter.sln /t:Build /p:Configuration=Debug`
- リストア: `dotnet restore ExcelDBImporter.sln`（restore 自体は dotnet で可）
- 実行: `ExcelDBImporter\bin\Debug\net8.0-windows\ExcelDBImporter.exe`（GUI アプリ。ターミナルから起動するとブロックされる）
- EF マイグレーション追加: `dotnet ef migrations add <Name> --project ExcelDBImporter --startup-project ExcelDBImporter`
- EF マイグレーション適用はアプリ起動時に自動（`Tool/DatabaseInitializer.cs` 経由の可能性。DB は exe と同じ出力ディレクトリの `excel_data.db`）
- git / ls / grep は標準的。PowerShell 環境（`&&` 不可、`;` で連結）