# Sample データについて

このフォルダの CSV・txt ファイルは **Shift-JIS (CP932) で意図的に保存しています**。

- 本アプリの CSV/テキスト取り込みは UTF.Unknown による文字コード自動判定を
  行っており、Shift-JIS の実運用入力を再現するテストデータとして維持しています
- `.editorconfig` で `Sample/**` は `charset = unset` を指定済みです。
  **UTF-8 へ変換しないでください**（自動判定の検証できなくなるため）
- ソースコード（.cs 等）は UTF-8-BOM に統一されています（リポジトリ直下の
  `.editorconfig` 参照）
