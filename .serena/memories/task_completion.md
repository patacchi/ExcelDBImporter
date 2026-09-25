# Task completion

コーディングタスク完了時に実行:

1. ビルド: `"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ExcelDBImporter.sln /t:Build /p:Configuration=Debug`（0 errors 必須。`dotnet build` は COM 参照で MSB4803 になるため使わない）
2. モデル/DbContext を変更した場合: `dotnet ef migrations add <Name> --project ExcelDBImporter --startup-project ExcelDBImporter` を追加し、ビルド再実行
3. テストプロジェクトは存在しない（手動 GUI 確認が代替）
4. 変更後、VS Code の Problems（get_errors / diagnostics）で新規エラーがないことを確認