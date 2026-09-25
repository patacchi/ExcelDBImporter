# DB遅延調査結果 (2026-09-25)

## DbContext (Context/ExcelDbContext.cs)
- DbSet 8個: ShShukka, TableFieldAliasNameLists, AppSettings, TableDBcolumnNameAndExcelFieldNames, TQRinputs, TTempQRrows, ViewMarsharings, ShInOuts
- OnConfiguring 内で UseSqlite(`Application.StartupPath/excel_data.db`) + LogTo(Debug, Information) を毎回設定 → オプションがインスタンスごとに異なる扱いになり、EF 内部サービスプロバイダ/モデルの再構築を誘発（`new ExcelDbContext()` のたびに重い）。static DbContextOptions or AddDbContext 化が定石
- OnModelCreating 無し。ライフサイクル: ほぼ全メソッドで `new ExcelDbContext()`（using）。FrmDataViewer のみフォーム寿命で保持。DI 無し

## 最悪の遅延箇所
1. Tool/ParseDMtextToTQRinput.cs `RegistToTQRinput` — ループ内で IsDupe(SELECT) + Add + **SaveChanges() を1件ごと**(L298, L324)。DeleteTtempByIDQueue も ID ごと FirstOrDefault + 呼び出しごと SaveChanges。N+1 + 1件保存
2. Tool/DbContextExtensions.cs `UpsertOperation.Execute` — エンティティごとに GetEntitySpecFieldEqual → `Set<TEntity>().FirstOrDefault()` = N+1。SaveChanges は最後1回
3. ShShukkaUpsert.cs `JudgeUpdateInsert` — Excel 1行ごとに `ShShukka.FirstOrDefault(StrSeiban)` N+1。StrSeiban にインデックス無し→フルスキャン。例外時 Thread.Sleep(1000)+再帰
4. RegistAllClassAndPropertys.cs / DatabaseInitializer.cs UpsertProperty — ループ内 FirstOrDefault（起動時にも走る）
5. FrmExcelImpoerter.cs `OutputxlsxFilterdByTimePickerTime` — ヘッダーセルごとに Include+Where+FirstOrDefault（N+1）

## その他
- AsNoTracking は全コード 0 件。async/await DB 処理なし（全部同期・UIスレッド）。.Result/.Wait は無し
- FrmQRread のシリアルイベント → DecordQRstringToTQRinput が同期的に DB 大量処理
- 唯一のインデックス: TTempQRrowData.DateInputDate ([Index])。ShShukka.StrSeiban / TQRinput(DateInputDate,QROPcode) 等にインデックス無し
- マイグレーション16本 (2024/03/02〜04/25)。DB: bin/Debug/net8.0-windows/excel_data.db (+.bak, DB_Snapshot/)
- 良い例: Tool/ViewTableEditor.cs FillEmptyDay（BeginTransaction + AddRange + SaveChanges 1回）
