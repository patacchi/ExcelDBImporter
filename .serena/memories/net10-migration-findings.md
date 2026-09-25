# net8.0-windows → net10.0-windows 移行調査 (2026-09-25)

- ビルド設定: global.json / Directory.Build.props / packages.config / nuget.config 無し。SDK ピン留め無し
- csproj: SDK型・net8.0-windows・Nullable/ImplicitUsings enable。LangVersion 指定無し。FileVersion/AssemblyVersion=0.6.2.03
- COM参照 (Office Core 2.8 / Excel Interop 1.9, EmbedInteropTypes) → dotnet CLI では MSB4803 で失敗、VS MSBuild 必須 (不変)
- View/** は Compile Remove 済みでフォルダ空。Backup csproj ×2 は sln 非参照で実害なし
- 危険 API: BinaryFormatter / AppDomain / WebRequest / Thread.Abort / DllImport 全て不使用
- System.Drawing は FrmDataViewer / FrmQRread / FrmPrintQRCode / Tool/PdfCreator 等で使用 → Windows Desktop ランタイム提供なので net10 でも可
- Application.StartupPath: Context/ExcelDbContext.cs:30 のみ (WinForms で継続サポート)
- リフレクション: Tool/GetAllProperty.cs (GetTypes/GetMembers), Tool/DbContextExtensions.cs (Activator.CreateInstance), Program.cs (GetEntryAssembly) — トリミング非有効化なので影響小
- EF Core: 全18マイグレーションが 9.0.0-preview.1/.3 で生成、ModelSnapshot あり (preview.3)。EF Core 10 へ上げる際に新マイグレーションで差分ゼロ確認推奨。SQLitePCLRaw.bundle_e_sqlite3 2.1.8 の明示参照は EF10 と整合確認要
- レガシー設定: App.config は空の <configuration/>。Properties/Settings.settings は空 Profile (ApplicationSettingsBase、Desktop ランタイムで動作)。csproj が参照する Resource1.resx/Resource1.Designer.cs は実体なし (Update のみなので無害)
- Tools/qr.cs はプロジェクトフォルダ外の .NET 10 ファイルベースアプリ (#:package ZXing.Net@0.16.9) → SDK アップグレードでむしろ動作対象になる。Sample/ は csv/txt のみでビルド無関係

## 破壊的変更 影響度調査 (2026-09-25 公式docs+実データ検証)

### Microsoft.Data.Sqlite 10 UTC変更 (High×3) → 実影響【低】
- 公式: ①GetDateTimeOffset オフセットなし→UTC前提(従来Local) ②DateTimeOffset→REAL書込UTC化 ③GetDateTime オフセット付き→UTC返却(従来Local)
- コード: DateTimeOffset 不使用。Microsoft.Data.Sqlite の GetDateTime/GetDateTimeOffset 直接呼びなし(ShShukkaUpsert の GetDateTime は ClosedXML XLCellValue のもの)
- 全 DateTime プロパティは DateTime/DateTime? のみ。DateTime.Now(Kind=Local) 書込はオフセットなし TEXT として保存され従来同様
- 実DB検証(bin配下 excel_data.db, 28,812日時値): オフセット付き 0件(+09:00形式 0件、"Z"26件はバイナリblob内の偶然一致)。全て `yyyy-MM-dd HH:mm:ss[(.fff)]` 形式
- 結論: ①③はオフセット付きデータが前提→該当ゼロ。②はDateTimeOffset不使用→該当ゼロ。安全策として `AppContext.SetSwitch("Microsoft.Data.Sqlite.Pre10TimeZoneHandling", true)` を Program.cs に置く選択肢あり(2-6 で判断)
- 要スモーク: 日付範囲クエリ(FrmExcelDBImporter_tools DatePerDay Between)と ViewTableEditor の日付突合

### EF Core 10 その他 → 実影響【低】
- ExecuteUpdateAsync lambda 変更: FrmExcelDBImporter_tools.cs:52 / FrmExcelImpoerter.cs:346 の単純ラムダはそのまま動作(式ツリー手組みではない)
- パラメータ化コレクション(MultipleParameters化): SQLトランスレーションの Contains なし(ExcludeList.Contains はインメモリ)
- Application Name 接続文字列注入: SQLite なので影響なし
- Complex type/ToJson/カスタム convention: 不使用
- multi-target 時の --framework 必須化: 単一TFMなので無関係
- 18マイグレーションが preview 生成 → 2-4 の空マイグレーション確認が必須(従来どおり)

### CsvHelper 31→33 → 実影響【低】
- 32.0.0 breaking: RecordWriter/RecordCreator 等の低レベル内部APIのみ → 不使用
- 33.0.0 breaking: Nullable 有効化(アノテーション変更程度)。CsvConfiguration ctor(CultureInfo)/BadDataFound/FromAttributes<T> は 31→33 でシグネチャ不変(FrmExcelDBImporter_tools.cs:261-278 の使用箇所は妥当性確認のみ)
- net6/7 TFM 削除 → net8/10 には無関係

### ClosedXML 0.104.0-preview2→0.105.1 → 実影響【低】(ただし注意2点)
- 現行は 2023年のテスト版 preview2。0.104 正式版 breaking(AutoFilter刷新/IXLNamedRange→IXLDefinedName/OpenXML SDK 3)と 0.105 breaking(数式エンジン再実装/Sort の参照更新/Hyperlinks移動/強名化)をまとめて跨ぐ
- アプリ使用API: new XLWorkbook/Worksheet(n)/RangeUsed/Cell.Value(XLCellValue)/SaveAs/Style.NumberFormat.Format/Worksheets.Add のみ → 数式・AutoFilter・Pivot・NamedRange 不使用で影響小
- 注意①: preview2→0.104正式版で XLCellValue 周りが確定済み。新規書換済みの ExcelFileComverter は XLCellValue を使用しており 0.105 でも同一API
- 注意②: PdfSharp/Svg と SixLabors.Fonts の解決版が競合しないか restore 時に確認(0.105.1 は [1.0.0,3.0.0) にバインド)

### .NET 10 本体/WinForms/SDK → 実影響【低】
- WinForms: StatusStrip/TreeView 不使用。OutOfMemoryException キャッチなし(ExternalException化の影響なし)。WPF併用なし
- System.Text.Json プロパティ名衝突チェック: ParseDMtextToTQRinput/QRcodeCreate の自己完結JSON(TQRinput/DMInputOPcode)は衝突プロパティなし→GUIスモークで確認
- SDK: dotnet restore の transitive audit / NU1510 等は警告レベル。global.json なしで SDK 10.0.401 がそのまま有効
- 危険API(BinaryFormatter/AppDomain/WebRequest/Thread.Abort): 不使用(前回調査から不変)

### 総合判定
- 移行ブロッカーなし。最大の注意点=SQLite UTC変更だが実データ/コード双方で該当ゼロ
- 2-4(空マイグレーション)と 2-6(日付クエリ+CSV入出力+QR解析スモーク)が検証の要
