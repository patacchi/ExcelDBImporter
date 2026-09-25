# リファクタリング進捗（ExcelDBImporter）

> 計画は `refactor/plan`。ステータス: pending / in-progress / done / blocked / skipped
> 更新ルール: 着手時にホスト名+日付付きで in-progress → 完了時に検証証跡付きで done（ソフトロック、詳細は plan のプロトコル参照）

## Phase M: Dev_QRread → master マージ
| Step | 内容 | Status | Host | 日付 | 備考 |
|---|---|---|---|---|---|
| M-1 | Dev_QRread push（未pushコミット+refactorメモリ） | done | PC124761-FCH | 2026-09-25 | origin/Dev_QRread=8687742 |
| M-2 | master へ --no-ff マージ+push | done | PC124761-FCH | 2026-09-25 | マージコミット ad18971、origin/master=ad18971。コンフリクトなし |
| M-3 | タグ v0.6.2-devqrread | done | PC124761-FCH | 2026-09-25 | ad18971 に付与、リモート push 済み |

## Phase 0: UTF-8 統一
| Step | 内容 | Status | Host | 日付 | 備考 |
|---|---|---|---|---|---|
| 0-1 | SJIS2本→UTF-8-BOM | done | PC124761-FCH | 2026-09-25 | FrmExcelImpoerter.cs / Program.cs。元blob cp932デコードと文字単位一致（改行正規化後）検証済 |
| 0-2 | BOM付与2本 | done | PC124761-FCH | 2026-09-25 | Tools/qr.cs / ToDo.txt。内容不変・BOMのみ |
| 0-3 | .editorconfig 新規 | done | PC124761-FCH | 2026-09-25 | utf-8-bom 統一 / Sample/** と .github/** は unset |
| 0-4 | Sample/README.md | done | PC124761-FCH | 2026-09-25 | Shift-JIS 維持の理由明記 |
| 0-5 | 検証+単独コミット | done | PC124761-FCH | 2026-09-25 | MSBuild exit=0 / 診断エラーなし / GUI起動で日本語表示OK（メインフォームタイトル＋多重起動MessageBox、ユーザー目視確認済） |

## Phase 1: COM 全廃
| Step | 内容 | Status | Host | 日付 | 備考 |
|---|---|---|---|---|---|
| 1-1 | csproj COMReference削除+ExcelDataReader追加 | done | PC124761-FCH | 2026-09-25 | COMReference×2削除、ExcelDataReader 3.9.0+CodePages 8.0.0追加 |
| 1-2 | XlsToXlsx 書換え | done | PC124761-FCH | 2026-09-25 | ExcelDataReader+ClosedXML化。エラーハンドリング強化(実体/シート/シート名サニタイズ/出力確認/例外別メッセージ)。コア処理は ConvertXlsToXlsx に分離(UI非依存) |
| 1-3 | CodePagesEncodingProvider 登録 | done | PC124761-FCH | 2026-09-25 | Program.cs Main冒頭 |
| 1-4 | ゴミusing削除（任意） | pending | | | 本Phaseでは後回し可 |
| 1-5 | dotnet build 検証+インポート比較 | done | PC124761-FCH | 2026-09-25 | dotnet build 0 errors 0 warnings。GUI実機検証: .xls/.xlsx インポートOK。エラー系6ケース(csv選択/偽装.xls/空.xls/書込拒否/1シートxlsx/Excelロック中)で期待ダイアログ表示・未捕捉例外なしを確認(コミット 133956a) |
| 1-6 | テスト基盤+変換テスト2本 | done | PC124761-FCH | 2026-09-25 | xUnit。正常系4+異常系16=20件成功。異常系: ファイル無し/偽装/空/シート名サニタイズ。ClosedXML SaveAsは出力先ディレクトリ自動作成(実測) |

## Phase 2: .NET 10 移行
| Step | 内容 | Status | Host | 日付 | 備考 |
|---|---|---|---|---|---|
| 2-1 | TFM net10.0-windows | pending | | | |
| 2-2 | パッケージ一括更新 | pending | | | 版数は plan 参照 |
| 2-3 | dotnet-ef ツール化 | pending | | | |
| 2-4 | 空マイグレーション確認 | pending | | | 重要 |
| 2-5 | csproj 整理 | pending | | | |
| 2-6 | 検証スモーク | pending | | | |

## Phase 3: DB 再アーキテクチャ
| Step | 内容 | Status | Host | 日付 | 備考 |
|---|---|---|---|---|---|
| 3a-1 | AddDbContextFactory + Composition Root | pending | | | |
| 3a-2 | OnConfiguring 刷新 | pending | | | |
| 3b-1 | Repositories/ 新設 | pending | | | |
| 3b-2 | Services/ 新設+async化 | pending | | | |
| 3b-3 | フォーム View 化 | pending | | | |
| 3c-1 | ImportService N+1 解消 | pending | | | |
| 3c-2 | QrParseService バッチ化 | pending | | | |
| 3c-3 | UpsertService 辞書突合 | pending | | | |
| 3c-4 | AsNoTracking 適用 | pending | | | |
| 3d-1 | インデックス+マイグレーション | pending | | | |
| 3e-1 | FrmQRread Channels 化 | pending | | | |
| 3e-2 | 性能計測 Before/After | pending | | | 数値をここに記録 |

## 性能ベースライン（Phase 3 着手前に計測して記録）
- Before: （未計測 — 2,500行インポートの秒数を Stopwatch で記録すること）
- After: （未計測）

## 変更ログ
- 2026-09-25 PC124761-FCH: plan/progress/findings 作成。Phase M〜3 確定。
- 2026-09-25 PC124761-FCH: **Phase M 完了**（M-1〜M-3 done）。Dev_QRread→master --no-ff マージ（ad18971）を origin に push、タグ v0.6.2-devqrread 付与。次: Phase 0（ブランチ refactor/phase0-encoding を master から作成）。
- 2026-09-25 PC124761-FCH: Dev_QRread ブランチをローカル/リモートから削除（master に完全マージ済み、ロールバックはタグ v0.6.2-devqrread で可能）。以降の Phase は master からブランチを切る。
- 2026-09-25 PC124761-FCH: **Phase 0 完了・master マージ済み**（マージコミット 0be007f、タグ refactor-phase0-done）。次: Phase 1（ブランチ refactor/phase1-com を master から作成）。
