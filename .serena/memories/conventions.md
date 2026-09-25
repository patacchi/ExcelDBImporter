# Conventions

- 部分クラスでフォームロジックを分割: `FrmExcelImpoerter.cs`（本体）+ `FrmExcelImpoerter.Designer.cs`（生成）+ `FrmExcelImpoerter_tools.cs`（補助）。Designer.cs は手編集しない
- 日本語コメント・日本語文字列リテラルが普通に使われる
- 既存の綴り誤りを勝手に修正しない（`FrmExcelImpoerter`、`EnumExtention`、`RegistAllClassAndPropertys` など）。修正するならリネームツールで参照ごと更新し、Designer/resx/マイグレーションとの整合を確認
- モデルクラスは EF Core の DataAnnotation 中心。テーブル名・列名は `nameof()` ベースでマイグレーション側と対応させている（20240402 マイグレーション）
- 列の表示名は `TableFieldAliasNameList` / `TableDBcolumnNameAndExcelFieldName` テーブルで DB 列 ↔ Excel フィールド名をマッピング