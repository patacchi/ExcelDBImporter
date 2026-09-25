using System.Text;
using ClosedXML.Excel;
using ExcelDataReader.Exceptions;
using Xunit;

namespace ExcelDBImporter.Tests
{
    /// <summary>
    /// ExcelFileComverter の異常系・エッジケーステスト (Phase 1: 1-6)
    /// コア処理 ConvertXlsToXlsx / SanitizeSheetName は UI 非依存なので
    /// MessageBox にブロックされずテストできる。
    /// </summary>
    public class XlsToXlsxErrorTests : IDisposable
    {
        private readonly string _strTempDir;

        static XlsToXlsxErrorTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public XlsToXlsxErrorTests()
        {
            _strTempDir = Path.Combine(Path.GetTempPath(), "ExcelDBImporterErrTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_strTempDir);
        }

        public void Dispose()
        {
            try { if (Directory.Exists(_strTempDir)) Directory.Delete(_strTempDir, true); } catch { /* 片付け失敗は無視 */ }
        }

        /// <summary>
        /// 存在しないファイルを変換しようすると FileNotFoundException になる
        /// </summary>
        [Fact]
        public void ConvertXlsToXlsx_ファイルが存在しない_FileNotFoundException()
        {
            string strMissing = Path.Combine(_strTempDir, "not_exist.xls");
            string strOut = Path.Combine(_strTempDir, "out.xlsx");

            var ex = Assert.Throws<FileNotFoundException>(
                () => ExcelFileComverter.ConvertXlsToXlsx(strMissing, strOut));
            Assert.Equal(strMissing, ex.FileName);
            Assert.False(File.Exists(strOut), "失敗時に出力ファイルが作られてはいけない");
        }

        /// <summary>
        /// .xls 偽装ファイル(実体はテキスト)は ExcelReaderException(HeaderException) で弾かれる
        /// ※HeaderException は ExcelReaderException の派生なので ThrowsAny を使う
        /// </summary>
        [Fact]
        public void ConvertXlsToXlsx_xls偽装テキストファイル_ExcelReaderException()
        {
            string strFake = Path.Combine(_strTempDir, "fake.xls");
            string strOut = Path.Combine(_strTempDir, "out.xlsx");
            File.WriteAllText(strFake, "これは.xlsではなく単なるテキストファイルです");

            Assert.ThrowsAny<ExcelReaderException>(
                () => ExcelFileComverter.ConvertXlsToXlsx(strFake, strOut));
            Assert.False(File.Exists(strOut), "失敗時に出力ファイルが作られてはいけない");
        }

        /// <summary>
        /// 空ファイル(0バイト)も ExcelReaderException で弾かれる
        /// </summary>
        [Fact]
        public void ConvertXlsToXlsx_空ファイル_ExcelReaderException()
        {
            string strEmpty = Path.Combine(_strTempDir, "empty.xls");
            string strOut = Path.Combine(_strTempDir, "out.xlsx");
            File.WriteAllBytes(strEmpty, Array.Empty<byte>());

            Assert.ThrowsAny<ExcelReaderException>(
                () => ExcelFileComverter.ConvertXlsToXlsx(strEmpty, strOut));
        }

        /// <summary>
        /// 出力先ディレクトリが存在しない場合、ClosedXML の SaveAs はディレクトリを自動作成して変換に成功する
        /// (Probe で確認した実挙動を正式テスト化: 実運用では出力元フォルダは必ず存在するため問題なし)
        /// </summary>
        [Fact]
        public void ConvertXlsToXlsx_出力先ディレクトリ無しは自動作成され成功する()
        {
            string strSample = GetSampleFilePath("rtn_betsu_list_sample.xls");
            string strOut = Path.Combine(_strTempDir, "auto_created_dir", "out.xlsx");

            ExcelFileComverter.ConvertXlsToXlsx(strSample, strOut);

            Assert.True(File.Exists(strOut), "ディレクトリが自動作成され出力ファイルが生成される");
        }

        /// <summary>
        /// シート名サニタイズ: 不正文字 ( : \ / ? * [ ] ) がアンダースコアに置換される
        /// </summary>
        [Theory]
        [InlineData("Sheet:1", "Sheet_1")]
        [InlineData("A\\B", "A_B")]
        [InlineData("A/B", "A_B")]
        [InlineData("A?B", "A_B")]
        [InlineData("A*B", "A_B")]
        [InlineData("A[B]", "A_B_")]
        public void SanitizeSheetName_不正文字が置換される(string? strRaw, string strExpected)
        {
            using var wb = new XLWorkbook();
            string strResult = ExcelFileComverter.SanitizeSheetName(strRaw, 0, wb);
            Assert.Equal(strExpected, strResult);
            //サニタイズ後の名前が ClosedXML に受理されることも確認
            var ws = wb.Worksheets.Add(strResult);
            Assert.Equal(strResult, ws.Name);
        }

        /// <summary>
        /// シート名サニタイズ: 31文字超は31文字に切り詰められる
        /// </summary>
        [Fact]
        public void SanitizeSheetName_31文字超は切り詰め()
        {
            using var wb = new XLWorkbook();
            string strLong = new string('A', 40);
            string strResult = ExcelFileComverter.SanitizeSheetName(strLong, 0, wb);
            Assert.Equal(31, strResult.Length);
            var ws = wb.Worksheets.Add(strResult);
            Assert.Equal(31, ws.Name.Length);
        }

        /// <summary>
        /// シート名サニタイズ: 空名・null は連番(SheetN)に補完される
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SanitizeSheetName_空名は連番に補完(string? strRaw)
        {
            using var wb = new XLWorkbook();
            string strResult = ExcelFileComverter.SanitizeSheetName(strRaw, 2, wb);
            Assert.Equal("Sheet3", strResult); // intSheetIndex=2 → Sheet3
            var ws = wb.Worksheets.Add(strResult);
            Assert.Equal("Sheet3", ws.Name);
        }

        /// <summary>
        /// シート名サニタイズ: 既存シートと同名の場合は重複回避サフィックスが付く
        /// </summary>
        [Fact]
        public void SanitizeSheetName_既存シートと同名はサフィックス付与()
        {
            using var wb = new XLWorkbook();
            wb.Worksheets.Add("Data");
            string strResult = ExcelFileComverter.SanitizeSheetName("Data", 1, wb);
            Assert.NotEqual("Data", strResult);
            Assert.StartsWith("Data_", strResult);
            var ws = wb.Worksheets.Add(strResult); //受理される=一意
            Assert.Equal(strResult, ws.Name);
        }

        /// <summary>
        /// シート名サニタイズ: 大文字小文字違いの重複も回避される(Excelのシート名は大文字小文字非依存)
        /// </summary>
        [Fact]
        public void SanitizeSheetName_大文字小文字違いの重複も回避()
        {
            using var wb = new XLWorkbook();
            wb.Worksheets.Add("Data");
            string strResult = ExcelFileComverter.SanitizeSheetName("DATA", 1, wb);
            Assert.NotEqual("DATA", strResult);
            wb.Worksheets.Add(strResult); //例外にならない=一意
        }

        /// <summary>
        /// 拡張子チェック: ダイアログのフィルタを無視した選択(.csv等)を弾けること
        /// </summary>
        [Theory]
        [InlineData("book.xlsx", true)]
        [InlineData("book.XLSX", true)]
        [InlineData("book.xls", true)]
        [InlineData("book.XLS", true)]
        [InlineData("book.csv", false)]
        [InlineData("book.txt", false)]
        [InlineData("book.xlsm", false)]
        [InlineData("noext", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsSupportedExcelFile_許可拡張子のみtrue(string? strFile, bool bExpected)
        {
            Assert.Equal(bExpected, ExcelFileComverter.IsSupportedExcelFile(strFile));
        }

        private static string GetSampleFilePath(string strSampleFileName)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string samplePath = Path.Combine(dir.FullName, "Sample", strSampleFileName);
                if (File.Exists(samplePath)) { return samplePath; }
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException($"Sample ファイルが見つかりません: {strSampleFileName}");
        }
    }
}
