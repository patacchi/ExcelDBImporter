using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExcelDBImporter.Tool;
using ExcelDBImporter.Models;
using Svg;

namespace ExcelDBImporter
{
    public partial class FrmPrintQRCode : Form
    {
        public Dictionary<string,MemoryStream> DicSVGStream { get; set; }
        public FrmPrintQRCode()
        {
            InitializeComponent();
            //ここまででDicSVFStreamがnullだったら初期値としてOROPcodeを設定
            DicSVGStream ??= QRcodeCreate.GetSVGMemoryStreamWithComment(
                CreateOPcodeJsonDic(GetQROPcodeQue()));
            SetSVGImageToPictbox();
        }

        /// <summary>
        /// QROPcodeのQueを返す
        /// </summary>
        /// <returns>Que<QROPcode></returns>
        private static Queue<QrOPcode> GetQROPcodeQue()
        {
            Queue<QrOPcode> qrOPcodes = new ();
            qrOPcodes.Enqueue(QrOPcode.PrepareReceive);
            qrOPcodes.Enqueue(QrOPcode.PrepareReveiveSet);
            qrOPcodes.Enqueue(QrOPcode.FreewayDataInput);
            qrOPcodes.Enqueue(QrOPcode.ShppingDeliverSet);
            return qrOPcodes;
        }

        /// <summary>
        /// QROPcodeのQueueを引数に、キーがCommentのDictionaryを返す関数
        /// </summary>
        /// <param name="QueqrOPcode">QROPcodeのQueue</param>
        /// <returns>キーがCommentのDictionary、Commen無い時はプロパティ名そのまま
        ///  Value はTQRinputにQROPcodeの値がセットされたクラスのJson</returns>
        public static Dictionary<string, string> CreateOPcodeJsonDic(Queue<QrOPcode> QueqrOPcode)
        {
            Dictionary<string, string> DicJson = [];
            foreach (QrOPcode qrOPcode in QueqrOPcode)
            {
                //それぞれのQROPcodeからCommentを取り出す

                DicJson.Add(
                    //キーはQROPcodeのComment
                    GetAllProperty.GetEnumComment<QrOPcode>(qrOPcode),
                    //ValueはTQRinputクラスのJson
                    QRcodeCreate.GeneratetJsonFromClass(
                    new TQRinput() { QROPcode = qrOPcode })
                    );
            }
            return DicJson;
        }

        /// <summary>
        /// SVGImageのQueueの画像をPictureBoxに表示する
        /// </summary>
        private void SetSVGImageToPictbox()
        {
            if ( DicSVGStream == null) { return; }
            int IntImageCount = 0;
            PictureBox[] PictBoxArray =
            [
                PictBox1of4,
                PictBox2of4,
                PictBox3of4,
                PictBox4of4
            ];
            Label[] LabelArray =
                [
                Lbl1of4,
                Lbl2of4,
                Lbl3of4,
                Lbl4of4,
                ];
            foreach (KeyValuePair<string, MemoryStream> keyValuePair in DicSVGStream) 
            {
                if (IntImageCount > 4) { break; }
                SvgDocument svgDocument = SvgDocument.Open<SvgDocument>(keyValuePair.Value);
                PictBoxArray[IntImageCount].Image = svgDocument.Draw(300, 300);
                LabelArray[IntImageCount].Text = keyValuePair.Key;
                keyValuePair.Value.Dispose();
                IntImageCount++;
            }
        }
    }
}
