using System.Text;
using ClosedXML.Excel;
using ExcelDataReader;
using Xunit;

namespace ExcelDBImporter.Tests
{
    /// <summary>
    /// ExcelFileComverter.XlsToXlsx の変換テスト (Phase 1: 1-6)
    /// ①変換結果が ClosedXML で開けること・シート構成が保たれること
    /// ②元 .xls を ExcelDataReader で直接読んだ値と、変換後 xlsx の ClosedXML 読取値が一致すること
    /// </summary>
    public class XlsToXlsxConverterTests : IDisposable
    {
        private readonly string _strTempDir;

        static XlsToXlsxConverterTests()
        {
            // .NET Core 系で BIFF の旧コードページ(cp932等)を読むために必須
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public XlsToXlsxConverterTests()
        {
            _strTempDir = Path.Combine(Path.GetTempPath(), "ExcelDBImporterTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_strTempDir);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_strTempDir)) Directory.Delete(_strTempDir, true); } catch { /* テスト後片付けは失敗させても無視 */ }
        }

        /// <summary>
        /// リポジトリの Sample フォルダパスをテストアセンブリ位置から上方探索して取得
        /// </summary>
        private static string GetSampleDirPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string samplePath = Path.Combine(dir.FullName, "Sample");
                if (Directory.Exists(samplePath)) { return samplePath; }
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("Sample フォルダが見つかりません");
        }

        /// <summary>
        /// Sample の .xls を一時フォルダにコピー(変換はコピー先で実行しリポジトリを汚さない)
        /// </summary>
        private string CopySampleToTemp(string strSampleFileName)
        {
            string strSource = Path.Combine(GetSampleDirPath(), strSampleFileName);
            Assert.True(File.Exists(strSource), $"Sample ファイルが見つかりません: {strSource}");
            string strTempFile = Path.Combine(_strTempDir, strSampleFileName);
            File.Copy(strSource, strTempFile, true);
            return strTempFile;
        }

        /// <summary>
        /// ①変換が成功し、ClosedXML で開け、シート数・順序・行数が保たれること
        /// </summary>
        [Theory]
        [InlineData("rtn_betsu_list_sample.xls")]
        [InlineData("syukka_HTU001_0001.xls")]
        public void XlsToXlsx_変換結果がClosedXMLで開けシート構成が保たれる(string strSampleFile)
        {
            string strXlsPath = CopySampleToTemp(strSampleFile);
            var converter = new ExcelFileComverter();

            converter.XlsToXlsx(strXlsPath);

            Assert.NotNull(converter.StrConvertedFilePath);
            Assert.True(File.Exists(converter.StrConvertedFilePath), "変換後xlsxが生成されていない");
            Assert.Equal(".xlsx", Path.GetExtension(converter.StrConvertedFilePath));

            //元ファイルのシート情報を ExcelDataReader で直接取得
            int intSheetCount;
            var sheetRowCounts = new List<int>();
            using (FileStream fs = File.Open(strXlsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(fs))
            {
                intSheetCount = reader.ResultsCount;
                do
                {
                    //1つ以上値を持つ行の行数(末尾空行を除外した実効行数)
                    int intUsedRows = 0;
                    while (reader.Read())
                    {
                        bool hasValue = false;
                        for (int c = 0; c < reader.FieldCount; c++)
                        {
                            if (reader.IsDBNull(c)) { continue; }
                            object? v = reader.GetValue(c);
                            if (v == null) { continue; }
                            if (v is string sv && string.IsNullOrEmpty(sv)) { continue; }
                            hasValue = true; break;
                        }
                        if (hasValue) { intUsedRows = reader.Depth + 1; }
                    }
                    sheetRowCounts.Add(intUsedRows);
                } while (reader.NextResult());
            }

            //変換後xlsxを ClosedXML で開く
            using (var wb = new XLWorkbook(converter.StrConvertedFilePath))
            {
                Assert.Equal(intSheetCount, wb.Worksheets.Count);
                //シート順序は元ファイルと同じ(1シート目=元1シート目)
                for (int i = 0; i < intSheetCount; i++)
                {
                    IXLWorksheet ws = wb.Worksheet(i + 1);
                    int intLastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                    //ClosedXMLの最終使用行は元の実効行数と一致(空行扱いの差は許容)
                    Assert.True(intLastRow <= sheetRowCounts[i],
                        $"シート{i + 1}: 変換後の最終行 {intLastRow} が元の実効行数 {sheetRowCounts[i]} を超えている");
                    Assert.True(intLastRow >= sheetRowCounts[i] - 1,
                        $"シート{i + 1}: 変換後の最終行 {intLastRow} が元の実効行数 {sheetRowCounts[i]} より2行以上少ない");
                }
            }
        }

        /// <summary>
        /// ②値の忠実性: 元 .xls(ExcelDataReader直接読取)と変換後 xlsx(ClosedXML)のセル値が一致する
        /// double/DateTime/bool/string/空 の型境界を検出する
        /// </summary>
        [Theory]
        [InlineData("rtn_betsu_list_sample.xls")]
        [InlineData("syukka_HTU001_0001.xls")]
        public void XlsToXlsx_セル値が元ファイルと一致する(string strSampleFile)
        {
            string strXlsPath = CopySampleToTemp(strSampleFile);
            var converter = new ExcelFileComverter();

            converter.XlsToXlsx(strXlsPath);
            Assert.NotNull(converter.StrConvertedFilePath);

            //元 .xls を全シート・全セル分読み取り
            var originCells = new List<(int Sheet, int Row, int Col, object? Value)>();
            using (FileStream fs = File.Open(strXlsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(fs))
            {
                int sheetIndex = 0;
                do
                {
                    int rowIndex = 0;
                    while (reader.Read())
                    {
                        rowIndex++;
                        for (int c = 0; c < reader.FieldCount; c++)
                        {
                            object? v = reader.IsDBNull(c) ? null : reader.GetValue(c);
                            if (v == null || (v is string sv && string.IsNullOrEmpty(sv))) { continue; }
                            originCells.Add((sheetIndex, rowIndex, c + 1, v));
                        }
                    }
                    sheetIndex++;
                } while (reader.NextResult());
            }

            Assert.NotEmpty(originCells);

            //変換後 xlsx を ClosedXML で読み取り突合
            using (var wb = new XLWorkbook(converter.StrConvertedFilePath))
            {
                foreach (var (sheet, row, col, value) in originCells)
                {
                    IXLWorksheet ws = wb.Worksheet(sheet + 1);
                    IXLCell cell = ws.Cell(row, col);
                    string strPos = $"シート{sheet + 1} R{row}C{col}";

                    switch (value)
                    {
                        case double d:
                        {
                            double actual = cell.GetValue<double>();
                            Assert.True(Math.Abs(d - actual) < 1e-10, $"{strPos}: double不一致 (expected={d}, actual={actual})");
                            break;
                        }
                        case int i:
                        {
                            double actual = cell.GetValue<double>();
                            Assert.True(Math.Abs((double)i - actual) < 1e-10, $"{strPos}: int->double不一致 (expected={i}, actual={actual})");
                            break;
                        }
                        case DateTime dt:
                        {
                            DateTime actual = cell.GetValue<DateTime>();
                            Assert.True(actual == dt, $"{strPos}: DateTime不一致 (expected={dt:yyyy-MM-dd HH:mm:ss}, actual={actual:yyyy-MM-dd HH:mm:ss})");
                            break;
                        }
                        case bool b:
                        {
                            bool actual = cell.GetValue<bool>();
                            Assert.True(actual == b, $"{strPos}: bool不一致 (expected={b}, actual={actual})");
                            break;
                        }
                        case TimeSpan ts:
                        {
                            TimeSpan actual = cell.GetValue<TimeSpan>();
                            Assert.True(actual == ts, $"{strPos}: TimeSpan不一致 (expected={ts}, actual={actual})");
                            break;
                        }
                        case string s:
                        {
                            string actual = cell.GetValue<string>();
                            Assert.True(actual == s, $"{strPos}: string不一致 (expected=[{s}], actual=[{actual}])");
                            break;
                        }
                        default:
                        {
                            string expected = value?.ToString() ?? string.Empty;
                            string actual = cell.GetValue<string>();
                            Assert.True(actual == expected, $"{strPos}: 想定外型のstring化不一致 (expected=[{expected}], actual=[{actual}])");
                            break;
                        }
                    }
                }
            }
        }
    }
}
