using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace imageConvert
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string selectPath = "";
        //固定高宽，便于后续处理
        int WWidth = 1650;
        int HHeight = 2320;
        List<System.Drawing.Image> AllName = new List<System.Drawing.Image>();
        /// <summary>
        /// 选择路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                txtFrompath.Text = foldPath;
                selectPath = foldPath;
            }



        }
        //开始转换
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            Thread thred = new Thread(Start);
            thred.Start();
            // btnStart.Enabled = true;
        }
        public void Start()
        {
            //获取文件信息
            DirectoryInfo folder = new DirectoryInfo(selectPath);
            int count = folder.GetDirectories().Count();
            int current = 1;
            string mess = DateTime.Now.ToString() + ":" + "本初一共处理数据，" + count + "条\r\n";
            TextWrite(txtToPath.Text, DateTime.Now.ToString("yyyy-MM-dd"), mess);
            foreach (var dirInfo in folder.GetDirectories())
            {

                Object thisLock = new Object();
                lock (thisLock)
                {
                    GC.Collect();
                    CombineImages(dirInfo.GetFiles("*.jpg"), dirInfo.Name, txtToPath.Text);
                    GC.Collect();

                }
                current++;
                TextWrite(txtToPath.Text, DateTime.Now.ToString("yyyy-MM-dd"), mess + "当前正在处理第" + current + "条，文件名称为，" + dirInfo.Name + "\r\n");


            }
        }






        //合并图片
        private void CombineImages(FileInfo[] files, string fileName, string toPath, ImageMergeOrientation mergeType = ImageMergeOrientation.Vertical, ConvertType convertType = ConvertType.PDF)
        {
            var finalImage = toPath;
            var imgs = files.Select(f => System.Drawing.Image.FromFile(f.FullName));

            var finalWidth = mergeType == ImageMergeOrientation.Horizontal ?
                imgs.Sum(img => img.Width) :
                imgs.Max(img => img.Width);

            var finalHeight = mergeType == ImageMergeOrientation.Vertical ?
                imgs.Sum(img => img.Height) :
                imgs.Max(img => img.Height);

            var finalImg = new Bitmap(finalWidth, finalHeight - (files.Count() * 1000));

            switch (convertType)
            {
                #region JPG合成
                case ConvertType.JPG:
                    Graphics g = Graphics.FromImage(finalImg);
                    g.Clear(SystemColors.AppWorkspace);

                    var width = finalWidth;
                    var height = finalHeight;
                    var nIndex = 0;
                    foreach (FileInfo file in files)
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromFile(file.FullName);
                        if (nIndex == 0)
                        {
                            g.DrawImage(img, new Point(500, 0));
                            nIndex++;
                            width = img.Width;
                            height = img.Height;
                        }
                        else
                        {
                            switch (mergeType)
                            {
                                case ImageMergeOrientation.Horizontal:
                                    g.DrawImage(img, new Point(width, 0));
                                    width += img.Width;
                                    break;
                                case ImageMergeOrientation.Vertical:
                                    g.DrawImage(img, new Point(500, height - 1000));
                                    height += img.Height - 1000;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException("mergeType");
                            }
                        }
                        img.Dispose();
                    }
                    string imageFilePath = finalImage + "\\" + fileName + ".jpg";
                    g.Dispose();
                    finalImg.Save(imageFilePath);
                    finalImg.Dispose();
                    break;
                #endregion
                #region PDF合成
                case ConvertType.PDF:
                    Document document = new Document();
                    document.SetPageSize(new iTextSharp.text.Rectangle(WWidth + 72f, HHeight + 72f));
                    PdfWriter write = PdfWriter.GetInstance(document, new FileStream(finalImage + "\\" + fileName + ".pdf", FileMode.OpenOrCreate, FileAccess.Write));
                    document.Open();
                    iTextSharp.text.Image jpg;

                    foreach (FileInfo file in files)
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromFile(file.FullName);
                        jpg = iTextSharp.text.Image.GetInstance(img, ImageFormat.Jpeg);
                        document.NewPage();
                        document.Add(jpg);
                    }
                    if (document != null && document.IsOpen())
                    {
                        document.Close();
                    }
                    if (write != null)
                    {
                        write.Close();
                    }
                    break;
                #endregion
                default:
                    break;
            }


        }
        public void TurnTheImageToPdf(string FileName, FileInfo[] images)
        {
            //  AllName.Clear();
            //  ChangeTheImageToS(ref SourceImage);
            Document document = new Document();
            document.SetPageSize(new iTextSharp.text.Rectangle(WWidth + 72f, HHeight + 72f));
            PdfWriter write = PdfWriter.GetInstance(document, new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write));
            document.Open();
            iTextSharp.text.Image jpg;

            for (int i = 0; i < AllName.Count; ++i)
            {
                jpg = iTextSharp.text.Image.GetInstance(AllName[i], ImageFormat.Jpeg);
                document.NewPage();
                document.Add(jpg);
            }
            if (document != null && document.IsOpen())
            {
                document.Close();
            }
            if (write != null)
            {
                write.Close();
            }
        }
        private void ChangeTheImageToS(ref List<string> ImageName)
        {
            for (int i = 0; i < ImageName.Count; ++i)
            {
                Bitmap src = new Bitmap(ImageName[i]);
                Bitmap bmImage = new Bitmap(WWidth, HHeight);
                Graphics g = Graphics.FromImage(bmImage);
                g.InterpolationMode = InterpolationMode.Low;
                g.DrawImage(src, new System.Drawing.Rectangle(0, 0, bmImage.Width, bmImage.Height), new System.Drawing.Rectangle(0, 0, src.Width, src.Height), GraphicsUnit.Pixel);
                g.Dispose();
                AllName.Add(bmImage);
            }
            GC.Collect();
        }

        /// <summary>
        /// 写文本文件，换行，不换行，以流的方式写数据
        /// </summary>
        public void TextWrite(string str, string logName, string strins)
        {

            if (Directory.Exists(txtToPath.Text) == false) //如果不存在就创建file文件夹
            {
                Directory.CreateDirectory(str);
            }

                //判断文件的存在
                if (File.Exists(str + "\\" + logName + ".log"))
                {
                    Console.WriteLine("该文件已经存在");
                }
                else
                {
                    //如果文件不存在，则创建该文件
                    File.Create(str + "\\" + logName + ".log");
                }

                //写入字符串数组，并且换行
                List<string> lstStr = new List<string>();
                lstStr.Add(strins);
                //如果该文件存在，并向其中追加写入数据
                if (File.Exists(str + "\\" + logName + ".log"))
                {
                    File.AppendAllLines(str + "\\" + logName + ".log", lstStr, Encoding.UTF8);
              
                }
                else
                //如果该文件不存在，则创建该文件，写入数据
                {
                    //如果该文件存在，这个方法会覆盖该文件中的内容
                    File.AppendAllLines(str + "\\" + logName + ".log", lstStr, Encoding.UTF8);
                }
                ////如果文件不存在，则创建；存在则覆盖

                //System.IO.File.AppendAllLines(str + "\\" + logName + ".log", lstStr, Encoding.UTF8);
                lstStr.Clear();
            }
        

    }

    enum ImageMergeOrientation
    {
        Horizontal,
        Vertical,
    }
    enum ConvertType
    {
        JPG,
        PDF
    }
}
