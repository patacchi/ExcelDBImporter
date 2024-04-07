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
        public Queue<MemoryStream> QueSVGStream { get; set; }
        public FrmPrintQRCode()
        {
            InitializeComponent();
            //ここまでQueSVGがnullだったら、標準動作としてQROPcodeのQRコードを作成
            QueSVGStream ??= QRcodeCreate.GetSVGMemoryStream(CreateOPcodeJsonQue());
            SetSVGImageToPictbox();
        }
        /// <summary>
        /// QROPcodeをシリアライズしたJSONTextのQueを返す
        /// </summary>
        /// <returns>シリアライズされたクラスのQue</returns>
        public static Queue<string> CreateOPcodeJsonQue()
        {
            Queue<string> QueJSON = new();
             QueJSON.Enqueue(QRcodeCreate.GeneratetJsonFromClass(
                new TQRinput(){ QROPcode = QrOPcode.PrepareReceive }));
            QueJSON.Enqueue(QRcodeCreate.GeneratetJsonFromClass(
                new TQRinput() {QROPcode = QrOPcode.PrepareReveiveSet }));
            QueJSON.Enqueue(QRcodeCreate.GeneratetJsonFromClass(
                new TQRinput() { QROPcode = QrOPcode.FreewayDataInput }));
            QueJSON.Enqueue(QRcodeCreate.GeneratetJsonFromClass(
                new TQRinput() { QROPcode = QrOPcode.ShppingDeliverSet }));
            return QueJSON;
        }

        /// <summary>
        /// SVGImageのQueueの画像をPictureBoxに表示する
        /// </summary>
        private void SetSVGImageToPictbox()
        {
            if ( QueSVGStream == null) { return; }
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
            foreach (MemoryStream ms in QueSVGStream ) 
            {
                if (IntImageCount > 4) { break; }
                SvgDocument svgDocument = SvgDocument.Open<SvgDocument>(ms);
                PictBoxArray[IntImageCount].Image = svgDocument.Draw(300, 300);
                LabelArray[IntImageCount].Text = IntImageCount.ToString();
                ms.Dispose();
                IntImageCount++;
            }
        }
    }
}
