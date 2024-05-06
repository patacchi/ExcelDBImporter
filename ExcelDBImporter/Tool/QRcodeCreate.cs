using ExcelDBImporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Datamatrix;

namespace ExcelDBImporter.Tool
{
    
    internal class QRcodeCreate
    {
        /// <summary>
        /// クラス TEntity を引数に取り、JSON文字列にシリアライズして返す関数
        /// </summary>
        /// <typeparam name="T">任意のクラス</typeparam>
        /// <returns></returns>
        public static string GeneratetJsonFromClass<T>(T Tclass) where T : class 
        {
            try
            {
                string Strqueues = JsonSerializer.Serialize(Tclass);
                return Strqueues;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return string.Empty;
            }
        }
        internal static T DeseriaizeFromJson<T>(string json) where T : class 
        {
            if (string.IsNullOrEmpty(json)) { return default!; }
            try
            {
                return JsonSerializer.Deserialize<T>(json)!;
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
                return default!;
            }
        }
        /// <summary>
        /// Queue(string Json)を引数として SVGイメージのQueueを返す
        /// </summary>
        /// <param name="QueJson">JsonのQueue</param>
        /// <returns></returns>
        internal static Queue<ZXing.Rendering.SvgRenderer.SvgImage> GetSVGFromJsonQue(
            Queue<string> QueJson )
        {
            BarcodeWriterSvg qrwriter = DMWriterformat();
            Queue<ZXing.Rendering.SvgRenderer.SvgImage> QuesvgImages = new();
            foreach (string StrJson in QueJson)
            {
                QuesvgImages.Enqueue(qrwriter.Write(StrJson));
            }
            return QuesvgImages;
        }

        internal static Queue<MemoryStream> GetSVGMemoryStream(
            Queue<string> QueJson)
        {
            BarcodeWriterSvg qrwriter = DMWriterformat();
            Queue<MemoryStream> QuememoryStreams = new();
            foreach (string StrJson in QueJson)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(qrwriter.Write(StrJson).Content);
                MemoryStream ms = new(byteArray);
                _ = ms.Seek(0, SeekOrigin.Begin);
                QuememoryStreams.Enqueue(ms);
            }
            return QuememoryStreams;
        }
        /// <summary>
        /// keyがコメントのDictionaryを引数にとり、SVG形式のMemoryStreamのDictionaryを返す関数
        /// </summary>
        /// <param name="DicJsonWithComment">key はコメント、string value はSVGにしたいString(JSON想定)</param>
        /// <returns>Dictionary(string,MemoryStream)で、keyがコメント、ValueがSVG画像のMemoryStream(String)</returns>
        internal static Dictionary<string,MemoryStream> GetSVGMemoryStreamWithComment(
            Dictionary<string,string> DicJsonWithComment )
        {
            BarcodeWriterSvg qrwriter = DMWriterformat();
            Dictionary<string, MemoryStream> DicmemoryStreamWithComment = [];
            foreach (KeyValuePair<string, string> keyValuePair in DicJsonWithComment)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(qrwriter.Write(keyValuePair.Value).Content);
                MemoryStream ms = new(byteArray);
                _ = ms.Seek(0, SeekOrigin.Begin);
                DicmemoryStreamWithComment.Add(
                    keyValuePair.Key,
                    ms
                    );
            }
            return DicmemoryStreamWithComment;
        }
        /// <summary>
        /// Stringを引数にとり、SVG形式のDMコードのMemoryStreamを返す
        /// </summary>
        /// <param name="StrQRstring">DMコードにしたい内容のString</param>
        /// <returns>SVGイメージのDMコードのMemoryStream</returns>
        internal static MemoryStream GetDM_SVGMemoryStreamFromText(string StrQRstring)
        {
            if (StrQRstring is null) { return new MemoryStream(); }
            BarcodeWriterSvg dmwriter = DMWriterformat();
            byte[] byteArray = Encoding.UTF8.GetBytes(dmwriter.Write(StrQRstring).Content);
            MemoryStream ms = new(byteArray);
            _ = ms.Seek(0,SeekOrigin.Begin);
            return ms;
        }
        private static BarcodeWriterSvg DMWriterformat()
        {
            //DMコードのフォーマットを指定
            BarcodeWriterSvg d2writer = new()
            {
                //種類はDMコード
                Format = BarcodeFormat.DATA_MATRIX,
                Options = new DatamatrixEncodingOptions
                {
                    //エラー訂正レベル
                    //ErrorCorrection = ErrorCorrectionLevel.H,
                    //文字列エンコード
                    CharacterSet = "UTF-8",
                    Width = 300,
                    Height = 300,
                    Margin = 5,
                },
            };
            return d2writer;
        }
    }
}
