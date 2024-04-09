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

namespace ExcelDBImporter.Tool
{
    
    /// <summary>
    /// QRコード読み取りの結果を一時格納しておくクラス。
    /// 後に、TQRinputテーブルに入れれるように処理をする。
    /// </summary>
    public class OPcodeList
    
    {
        /// <summary>
        /// QRコード読み取り時の日時、スキャナ側から入力なければ現在時刻
        /// </summary>
        public DateTime DateInputDate {  get; set; }
        /// <summary>
        /// QRコードにJSONが格納されていれば、TRQinputクラスなので、デコードして入れる
        /// </summary>
        public TQRinput? TQRinput { get; set; }
        public string? StrOrderNum { get; set; }
        public string? Description { get; set; }
    }
    internal class QRcodeCreate
    {
        /// <summary>
        /// Queue<T>を引数に取り、JSON文字列にシリアライズして返す関数
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
            BarcodeWriterSvg qrwriter = QRWriterformat();
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
            BarcodeWriterSvg qrwriter = QRWriterformat();
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
        internal static Dictionary<string,MemoryStream> GetSVGMemoryStreamWithComment(
            Dictionary<string,string> DicJsonWithComment )
        {
            BarcodeWriterSvg qrwriter = QRWriterformat();
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
        /// Stringを引数にとり、SVG形式のQRコードのMemoryStreamを返す
        /// </summary>
        /// <param name="StrQRstring">QRコードにしたい内容のString</param>
        /// <returns>SVGイメージのQRコードのMemoryStream</returns>
        internal static MemoryStream GetQR_SVFMemoryStreamFromText(string StrQRstring)
        {
            if (StrQRstring is null) { return new MemoryStream(); }
            BarcodeWriterSvg qrwriter = QRWriterformat();
            byte[] byteArray = Encoding.UTF8.GetBytes(qrwriter.Write(StrQRstring).Content);
            MemoryStream ms = new(byteArray);
            _ = ms.Seek(0,SeekOrigin.Begin);
            return ms;
        }
        private static BarcodeWriterSvg QRWriterformat()
        {
            //QRコードのフォーマットを指定
            BarcodeWriterSvg qrwriter = new()
            {
                //種類はQRコード
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    //エラー訂正レベル
                    ErrorCorrection = ErrorCorrectionLevel.H,
                    //文字列エンコード
                    CharacterSet = "UTF-8",
                    Width = 300,
                    Height = 300,
                    Margin = 5,
                },
            };
            return qrwriter;
        }
    }
}
