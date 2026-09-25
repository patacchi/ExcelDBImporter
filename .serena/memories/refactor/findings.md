# リファクタリング調査結果（ExcelDBImporter）

> 2026-09-25 調査（PC124761-FCH、Serena+Context7+NuGet確認）。計画の根拠データ。

## Git 状態（Phase M 根拠）
- remote: github.com/patacchi/ExcelDBImporter。作業ブランチ Dev_QRread
- master(bfe3da, 2024-04-02) は Dev_QRread の祖先 → **FF 可能・コンフリクトなし**。Dev_QRread 先行59コミット、master 側先行0。Dev_QRread の未push 2コミット
- 差分規模: 78 files, +8636/-225

## COM 参照（Phase 1 根拠）
- 実使用は `ExcelFileComverter.XlsToXlsx`（.xls→.xlsx 変換で Excel 起動、Workbooks.Open→SaveAs→Quit）のみ
- `Microsoft.Office.Core` COMReference はコード中完全未使用 → 即削除可
- 呼び出し元: `ExcelFileComverter.ExcelFileComVerter()`（.xls のときのみ）← `FrmExcelImpoerter.BtnImputExcelFile_Click`
- 読み取り/出力本体は ClosedXML 化済み（印刷・リボン等なし）
- 未使用 `using DocumentFormat.OpenXml.*` が約14ファイル（ゴミ）
- COM 以外の dotnet build 阻害要因なし（DllImport/レジストリ等ゼロ）

## エンコーディング（Phase 0 根拠）
- Shift-JIS(cp932): `FrmExcelImpoerter.cs`、`Program.cs`（BOMなし・UTF-8不正・cp932で正常デコード確認。日本語リテラル「小計」「多重起動は…」あり）
- BOMなしUTF-8: `Tools/qr.cs`、`ToDo.txt`。他90超は UTF-8-BOM
- Sample/*.csv(3)・txt は Shift-JIS（データファイル、維持決定）
- .gitattributes は `* text=auto` のみ（エンコーディング変換なし）。.editorconfig 未存在

## .NET 10 移行（Phase 2 根拠）
- global.json / Directory.Build.props / LangVersion ピン なし。App.config は空。Resource1.resx は実体なし（csproj の Update だけ残存）
- 不使用確認: BinaryFormatter/AppDomain/WebRequest/Registry 等ゼロ
- System.Drawing 使用（WinForms 提供なので net10 でも可）: FrmDataViewer/FrmQRread/FrmPrintQRCode/PdfCreator 等
- リフレクション多用: GetAllProperty.cs、DbContextExtensions.cs、Program.cs（GetEntryAssembly().Location — SingleFile 化しない限り問題なし）
- マイグレーション18本すべて EF 9.0.0-preview 生成（preview→安定版で誤検出差分のリスク → 空マイグレーション確認ステップでガード）
- 最新安定版（2026-09 時点）: EF Core 10.0.12 / System.IO.Ports 10.0.12 / ClosedXML 0.105.1 / PDFsharp 6.2.4 / CsvHelper 33.1.0 / ExcelDataReader 3.9.0（MIT, net8+, BIFF2-2021, 読取専用。net core で BIFF 旧コードページには System.Text.Encoding.CodePages + RegisterProvider 必須）

## DB 性能（Phase 3 根拠）
- ShShukka 約3万件累積、1インポート1,500〜2,500行
- `new ExcelDbContext()` 151箇所。OnConfiguring で毎回 options 構築 + LogTo 常時（モデルキャッシュ/サービスプロバイダ再構築誘発）
- 🔴 `ParseDMtextToTQRinput.RegistToTQRinput`（L264-397）: ループ内 IsDupe SELECT + Add + **SaveChanges 1件ずつ** + DeleteTtempByIDQueue 都度 SaveChanges
- 🔴 `DbContextExtensions.UpsertOperation.Execute`（L342-541）: エンティティごと FirstOrDefault（N+1）。SaveChanges は末尾1回。リフレクション行ごと
- 🔴 `ShShukkaUpsert.JudgeUpdateInsert`（L100-268）: 行ごと FirstOrDefault かつ **StrSeiban にインデックス無し**（2500×30000 行スキャン）。例外時 Thread.Sleep(1000)+再帰+MessageBox
- 🟠 RegistAllClassAndPropertys.RegistToDB / DatabaseInitializer.UpsertProperty: 起動時ループ内 FirstOrDefault
- 🟠 OutputxlsxFilterdByTimePickerTime（L318-325）: ヘッダーセルごと Include+FirstOrDefault
- 検索キーにインデックス無し: ShShukka.StrSeiban / TQRinput.DateInputDate,QROPcode,StrOrderNum,StrTagBarcode / TableDBcolumnNameAndExcelFieldName(StrClassName,StrDBColumnName) / ViewMarsharing.DatePerDay。[Index] は TTempQRrowData.DateInputDate のみ
- AsNoTracking ゼロ。DB 処理 100% 同期（.Result/.Wait はなし）。FrmQRread.SerialCommunication_CompReceive = UI スレッドで重い DB 処理
- 良いテンプレ: `Tool/ViewTableEditor.FillEmptyDay`（BeginTransaction+AddRange+SaveChanges1回）
- DB 実体: bin/Debug/net8.0-windows/excel_data.db（+ .bak、DB_Snapshot/）
- FrmExcelImpoerter ctor から DateTimePickerInitialize→全件GroupBy+Upsert（起動時同期実行、起動遅延）
