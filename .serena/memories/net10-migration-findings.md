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
