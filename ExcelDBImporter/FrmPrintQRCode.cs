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
        public Dictionary<string, MemoryStream> DicSVGStream { get; set; }
        public FrmPrintQRCode()
        {
            InitializeComponent();
            //ここまででDicSVFStreamがnullだったら初期値としてOROPcodeを設定
            DicSVGStream ??= QRcodeCreate.GetSVGMemoryStreamWithComment(
                CreateOPcodeJsonDic(GetQROPcodeQue()));
            SetSVGImageToPictbox();
            ComboBoxInitialize();
        }

        /// <summary>
        /// ページ数入力コンボボックスの設定
        /// </summary>
        private void ComboBoxInitialize()
        {
            //2から16の範囲の数を設定
            CmbBoxImagesPerPage.DataSource = Enumerable.Range(2, 15).ToArray();
        }

        /// <summary>
        /// QROPcodeのQueを返す
        /// </summary>
        /// <returns>Que<QROPcode></returns>
        private static Queue<QrOPcode> GetQROPcodeQue()
        {
            Queue<QrOPcode> qrOPcodes = new();
            qrOPcodes.Enqueue(QrOPcode.PrepareReveiveSet);
            qrOPcodes.Enqueue(QrOPcode.FreewayDataInput);
            qrOPcodes.Enqueue(QrOPcode.Delivery);
            qrOPcodes.Enqueue(QrOPcode.ShppingDeliverSet);
            qrOPcodes.Enqueue(QrOPcode.MicrowaveDelivary);
            qrOPcodes.Enqueue(QrOPcode.CutCable);
            qrOPcodes.Enqueue(QrOPcode.Moving);
            qrOPcodes.Enqueue(QrOPcode.ChangeLocation);
            qrOPcodes.Enqueue(QrOPcode.Other);
            return qrOPcodes;
        }

        /// <summary>
        /// QROPcodeのQueueを引数に、キーがCommentのDictionaryを返す関数
        /// </summary>
        /// <param name="QueqrOPcode">QROPcodeのQueue</param>
        /// <returns>キーがCommentのDictionary、Commen無い時はプロパティ名そのまま
        ///  Value はTQRinputにQROPcodeの値がセットされたTQRinputクラスのJson</returns>
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
            if (DicSVGStream == null) { return; }
            PictureBox[] PictBoxArray =
            [
                PictBox1,
                PictBox2,
                PictBox3,
                PictBox4,
                PictBox5,
                PictBox6
            ];
            Label[] LabelArray =
            [
                Lbl1,
                Lbl2,
                Lbl3,
                Lbl4,
                Lbl5,
                Lbl6
            ];
            int IntImageCount = 0;
            foreach (KeyValuePair<string, MemoryStream> keyValuePair in DicSVGStream)
            {
                if (IntImageCount > 5) { break; }
                SvgDocument svgDocument = SvgDocument.Open<SvgDocument>(keyValuePair.Value);
                PictBoxArray[IntImageCount].Image = svgDocument.Draw(200, 200);
                LabelArray[IntImageCount].Text = keyValuePair.Key;
                IntImageCount++;
            }
        }

        private void TxtBoxUserString_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtBoxUserString.Text)) { return; }
            //QR自由記述欄に記入があったら、カスタムQRコードを作成するので、6番目のQRコードと当初のラベルを非表示にする
            PictBox6.Image = null;
            Lbl6.Visible = false;
            TxtBoxUserDescription.Visible = true;
            //(いる？)
            //自由記記述欄と同じ内容をDescriptionにも反映
            TxtBoxUserDescription.Text = TxtBoxUserString.Text;
            //記述された内容のSVGイメージのMemoryStreamを取得し、イメージ表示する
            SvgDocument svgDocument = SvgDocument.Open<SvgDocument>(QRcodeCreate.GetQR_SVGMemoryStreamFromText(TxtBoxUserString.Text));
            //得られたSVGドキュメントを描画
            PictBox6.Image = svgDocument.Draw(200, 200);
        }

        /// <summary>
        /// ComboBoxで指定された枚数をもとにPDFを作成する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreatePDF_Click(object sender, EventArgs e)
        {
            if (CmbBoxImagesPerPage.SelectedIndex == -1) { CmbBoxImagesPerPage.SelectedIndex = 0; }
            int IntImageContInPage = (int)CmbBoxImagesPerPage.SelectedItem!;
            PdfCreator creator = new();
            creator.CreatePdf(DicSVGStream, IntImageContInPage);
        }
    }
}
