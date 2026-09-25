# リファクタリング計画（ExcelDBImporter）

> 進捗は `refactor/progress` で管理。この plan.md は計画の単一の真実 (single source of truth)。
> 調査詳細は `refactor/findings`。変更は必ず plan/progress を同じコミットに含めて git 管理すること。

## 全体像
4案件+準備を順に実施: **M(master統合) → 0(UTF-8統一) → 1(COM全廃) → 2(.NET 10) → 3(DB再アーキテクチャ)**
Phase は逐次実行。各 Phase は独立ブランチで行い、完了時に master へマージ+タグ。

## ホスト間作業プロトコル（複数ホスト共通）
1. セッション開始: `git pull` → `refactor/plan` + `refactor/progress` + `refactor/findings` を読む
2. 着手: progress の次 pending ステップを `in-progress` に変更（**ホスト名+日付**を記入 = ソフトロック）→ この変更を先にコミット&プッシュ
3. 実施: コード変更と同じコミット（または直後のコミット）で progress を更新
4. 完了: `done` + 検証証跡（ビルド結果・計測値）を記録してプッシュ。次のステップへ
5. ルール: 同一ステップの並行作業禁止。`in-progress` が24h以上更新されない場合は他ホストが引き継ぎ可（progress に「引き継ぎ」メモを追記）
6. ホスト識別子は `$env:COMPUTERNAME`（例: PC124761-FCH）
7. ホスト固有情報（MSBuild パス等）は `mem:suggested_commands` にホスト名付きで追記。Phase 1 完了後は `dotnet build` が全ホスト共通

## Phase M: Dev_QRread → master マージ（前提作業）
- 調査済み: master は Dev_QRread の祖先。**fast-forward 可能・コンフリクトなし**。Dev_QRread 先行 59 コミット、master 側先行 0。Dev_QRread に未 push 2 コミット
- M-1: `git push origin Dev_QRread`（未 push 2 コミット + refactor/ メモリ一式を push）
- M-2: master で `git merge --no-ff Dev_QRread`（統合点コミットを残す。FF 希望なら `--ff-only`）→ `git push origin master`
- M-3: タグ `v0.6.2-devqrread` を打って push（ロールバック点）

## Phase 0: エンコーディング UTF-8 統一
- 対象（調査済み・詳細は findings）: Shift-JIS = `FrmExcelImpoerter.cs` / `Program.cs` の2本のみ。BOMなしUTF-8 = `Tools/qr.cs` / `ToDo.txt`
- 0-1: SJIS 2本を cp932 デコード → UTF-8-BOM 再保存。日本語リテラル目視確認
- 0-2: BOMなし2本に BOM 付与
- 0-3: `.editorconfig` 新規（`root=true` / `[*] charset=utf-8-bom` / `[Sample/**] charset=unset`）
- 0-4: `Sample/README.md` に「意図的 Shift-JIS（UTF.Unknown 自動判定テスト入力）」明記
- 0-5: 検証: ビルド 0 errors + exe 起動で日本語文字化けなし → **内容差分0の単独コミット**
- ブランチ: `refactor/phase0-encoding`（master から）

## Phase 1: COM 参照全廃（dotnet build 化）
- 1-1: csproj の COMReference ×2 削除。`ExcelDataReader 3.9.0` + `System.Text.Encoding.CodePages` 追加
- 1-2: `ExcelFileComverter.XlsToXlsx` を ExcelDataReader 読取 → ClosedXML で .xlsx 書出しに書換え（戻り値・一時ファイル契約維持。下流の ClosedXML パイプライン無変更）
- 1-3: `Program.cs` Main 冒頭に `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)`
- 1-4:（任意・並行）未使用 `using DocumentFormat.OpenXml.*` 削除（約14ファイル）
- 1-5: 検証: `dotnet build ExcelDBImporter.sln` 0 errors（VS MSBuild 不要）+ .xls/.xlsx 両方インポート結果目視比較
- 1-6: **テスト基盤**: `ExcelDBImporter.Tests/`（xUnit、net8.0-windows、main を ProjectReference）を sln に追加。`XlsToXlsx` の変換テスト2本: ①Sample の実 .xls → 変換 → ClosedXML で開ける/シート構成 ②ExcelDataReader 直接読取値と変換後 xlsx の ClosedXML 読取値のセル単位突合（double/DateTime/bool/string/空の型境界）。`dotnet test` が全ホスト共通の回帰ゲートになる（Phase 2/3 で再利用）
- ブランチ: `refactor/phase1-com`。完了時 master マージ+タグ `refactor-phase1-done`
- （任意）GitHub Actions windows-latest で `dotnet build` CI → ホスト間検証の客観化

## Phase 2: .NET 10 (LTS) 移行
- 2-1: csproj TFM → `net10.0-windows`
- 2-2: パッケージ更新: EF Core Sqlite/Design/Tools **10.0.12** / System.IO.Ports **10.0.12** / ClosedXML **0.105.1**（0.104→0.105 移行ガイド要確認）/ PDFsharp **6.2.4** / CsvHelper **33.1.0**（CsvConfiguration breaking change 要確認）。`SQLitePCLRaw.bundle_e_sqlite3` ピン留め外して確認。ZXing.Net/Svg/UTF.Unknown 据え置き
- 2-3: `dotnet tool install --global dotnet-ef`（10.x）。`mem:suggested_commands` 更新
- 2-4: **EF preview→安定版の空マイグレーション確認**: `dotnet ef migrations add CheckEf10` → Up/Down 空を確認して削除（空でなければ差分精査）
- 2-5: csproj 整理（実体のない Resource1.resx Update 削除、空 App.config 削除可）
- 2-6: 検証: `dotnet build`/`dotnet run`、起動時 Migrate 成功、全フォームスモーク、`Tools/qr.cs` が `dotnet run --file` で動作
- 2-7: **テスト基盤拡張（A+B 方針、下記「テスト戦略」参照）**: ExcelDbContext 注入対応(済) + DB層テスト(Migrate/DateTime/Upsert) + CSV往復 + QR/PDF生成 + DM解析
- ブランチ: `refactor/phase2-net10`。完了時マージ+タグ `refactor-phase2-done`

## テスト戦略（2026-09-25 決定: Phase 3 を踏まえた A/B/C 分類）
現状テストは xls→xlsx 変換の30件のみ。EF/DB・CSV・QR 領域が無検証。Phase 3(層分離/N+1修正)を控えて、テストを「Phase 3 後の生き残り」で分類する。
- **A. バージョンゲート（Phase 2 で実施）**: ①全マイグレーションの一時DBへ Migrate 成功 ②DateTime ラウンドトリップ(Local保存→分秒一致。SQLite UTC破壊的変更の回帰ゲート) ③QR/PDF生成が例外なし。→ スキーマ/生成ロジック不変のため Phase 3 でもそのまま生存。3d の新マイグレーションも①が自動検証
- **B. 特性化テスト（Phase 2 で実施、振る舞い粒度で書く）**: Upsert(エンティティ投入→DB状態アサート) / CSV往復(ShInOut) / DM解析(テキスト→TTempQRrow)。実装詳細に密着させず「入力→DB/ファイルの状態」のみ検証 → Phase 3 で呼び出し先が Repository/Service に変わったら**呼び出し行だけ移植**してアサションを再利用(移植は各 3x ステップの完了条件に組み込む)
- **C. アーキテクチャテスト（Phase 3 に回す）**: Service/Repository の async API・DI 構成・性能計測(3e-2)。Phase 3 で新設される API なので今は書けない
- 前提(済): `ExcelDbContext` に `DbContextOptions` 注入コンストラクタ追加 + `OnConfiguring` を `IsConfigured` ガード。テストは一時 SQLite を差し込む。この形は 3a-1/3a-2 でも維持
- テストは `ExcelDBImporter.Tests` に DB フィクスチャ(一時ファイル+Migrate)を共通化して追加

## Phase 3: DB アーキテクチャ再構築（3a→3b/3c→3d→3e、各段後にビルド+スモーク）
### 3a DI 基盤
- 3a-1: `Program.cs` を Composition Root 化: `AddDbContextFactory<ExcelDbContext>`（IDbContextFactory。操作ごと短命 DbContext 意味論を維持しつつ options/モデルキャッシュ共有）。DB パスは `Application.StartupPath\excel_data.db` 維持
- 3a-2: `ExcelDbContext.OnConfiguring` の文字列構築+常時 LogTo を撤去（ログは AppSetting フラグ/Debug のみ）
### 3b 層分離
- 3b-1: `Repositories/` 新設（IShShukkaRepository / ITQRinputRepository / ITTempQRRowRepository / IAliasRepository / IAppSettingRepository）。`DbContextExtensions.UpsertOperation` は Repositories 側へ移設しキー一括 SELECT→Dictionary 突合に刷新。リフレクションは型ごと静的キャッシュ
- 3b-2: `Services/` 新設: ImportService（現 ShShukkaUpsert）/ InOutCsvService / QrParseService（現 ParseDMtextToTQRinput）/ ExportService（現 OutputxlsxFilterdByTimePickerTime）/ ViewSharingService。全メソッド `*Async` + `SaveChangesAsync`
- 3b-3: フォームを薄い View 化。`Thread.Sleep(1000)`+再帰リトライ（ShShukkaUpsert）撤去
### 3c N+1 解消・バッチ化（ShShukka 約3万件 × 1インポート1,500〜2,500行前提）
- 3c-1: ImportService: 既存 StrSeiban を1クエリ事前読込 → HashSet/Dictionary 突合 → AddRange → SaveChangesAsync 1回+トランザクション（テンプレ: `Tool/ViewTableEditor.FillEmptyDay`）
- 3c-2: QrParseService.RegistToTQRinput: ループ内 IsDupe+都度 SaveChanges → バッチキー事前読込+保存1回。DeleteTtempByIDQueue は ExecuteDeleteAsync 化
- 3c-3: UpsertService.Execute: エンティティごと FirstOrDefault → キー一括 SELECT 辞書突合
- 3c-4: 読み取り専用クエリに AsNoTracking（FrmDataViewer、エイリアス参照、xlsx 出力のセルごとクエリ→辞書化）
### 3d インデックス
- 3d-1: `[Index]` 追加: ShShukka.StrSeiban（最重要）/ TQRinput.DateInputDate, QROPcode, StrOrderNum, StrTagBarcode / TableDBcolumnNameAndExcelFieldName(StrClassName,StrDBColumnName) 複合 / ViewMarsharing.DatePerDay → マイグレーション `AddPerformanceIndexes`
### 3e UI スレッド分離
- 3e-1: FrmQRread シリアル受信 → System.Threading.Channels キュー + バックグラウンド消費者（受信ハンドラは Enqueue のみ）
- 3e-2: 検証: 2,500行インポート前後の Stopwatch 計測（3万件 DB。目標1桁短縮、数値を progress に記録）、受信中 UI 応答、全フォーム手動スモーク
- ブランチ: `refactor/phase3-db`。完了時マージ+タグ `refactor-phase3-done`

## 検証（全体）
- 各 Phase 末: `dotnet build` 0 errors + VS Code Problems エラーゼロ + GUI スモーク + progress 更新
- Phase 3: 性能計測値を progress に記録（Before/After）

## 明示的スコープ外
- スキーマ再設計（インデックス追加のみ）/ excel_data.db の場所・名前変更 / PublishSingleFile・Native AOT / 綴り誤りリネーム（FrmExcelImpoerter 等・規約）/ ビルド対象外 View/・Tools/qr.cs・Sample/ の内容変更

## 決定事項
- .xls は ExcelDataReader で読み取り維持（Excel 起動全廃）
- DI + Repository/Service/View 層分離 + async 化まで実施。DbContext は IDbContextFactory パターン
- エンコーディング UTF-8-BOM 統一。Sample/ は Shift-JIS 維持
- 順序 M→0→1→2→3。Phase 別ブランチ+マージ+タグ
- QR 受信は Channels キュー（推奨案、3e 着手時に再確認可）

## Further Considerations（未決）
1. テスト基盤: xUnit + 一時 SQLite のサービス層テストを Phase 3 と並行で新設するか（現状テストなし、GUI 手動確認が代替）
2. CI: Phase 1 後に GitHub Actions を導入するか（全ホスト共通の検証ゲート）
