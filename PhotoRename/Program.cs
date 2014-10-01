using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using ExifLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhotoRename
{
    class Program
    {
        static void ProcessImage(string image, string outpath)
        {
            var timage = image;
            try {
                DateTime datePicture = DateTime.MinValue;
                try {
                    using (ExifReader reader = new ExifReader(timage)) {
                        DateTime datePictureTaken;
                        if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken)) {
                            datePicture = datePictureTaken;
                        }
                    }
                }
                catch (Exception) {
                }
                if (datePicture.Equals(DateTime.MinValue)) {
                    var date = File.GetLastWriteTime(timage);
                    Console.WriteLine("Can't determine date. " + timage + " Use file date: ( " + date.ToShortDateString() + " - " + date.ToShortTimeString() + " ) ? (y/n)");
                    if (Console.ReadKey().KeyChar.ToString().ToUpper().Equals("Y")) {
                        datePicture = date;
                        Encoding _Encoding = Encoding.UTF8;
                        Image theImage = new Bitmap(image);
                        // 36867 = DateTimeOriginal
                        // 36868 = DateTimeDigitized
                        PropertyItem prop = theImage.PropertyItems[0];
                        SetProperty(ref prop, 36868, date.ToString("yyyy:MM:dd HH:mm:ss"));
                        theImage.SetPropertyItem(prop);
                        theImage.Save(timage + ".tmp");
                        timage = timage + ".tmp";
                    }
                }
                if (!datePicture.Equals(DateTime.MinValue)) {
                    int i = 1;
                    while (File.Exists(DateToFile(outpath, datePicture, i))) {
                        i++;
                    }
                    //Console.WriteLine(DateToFile(outpath, datePicture, i));
                    if (!Directory.Exists(DateToYear(outpath, datePicture))) {
                        Directory.CreateDirectory(DateToYear(outpath, datePicture));
                    }
                    if (!Directory.Exists(DateToYearMonth(outpath, datePicture))) {
                        Directory.CreateDirectory(DateToYearMonth(outpath, datePicture));
                    }
                    File.Move(timage, DateToFile(outpath, datePicture, i));
                }
            }
            catch (Exception e) {
                Console.WriteLine(timage + ": " + e.Message);
            }
        }

        static void Main(string[] args)
        {
            string inpath = null;
            string outpath = null;
            bool help = false;
            bool verbose = false;
            var p = new OptionSet () {
   	            { "inpath=",      v => inpath = v },
   	            { "outpath=",      v => outpath = v },
   	            { "v|verbose",  v => verbose = v != null },
   	            { "h|?|help",   v => help = v != null },
            };
            p.Parse (args);
            //inpath = @"d:\foto\fotos\telefoon\20050702\";
            //outpath = @"d:\x\";
            if (inpath == null || outpath == null || help)
            {
                Console.WriteLine("PhotoRename.exe --inpath <path> --outpath <path>");
            }
            else
            {
                if (!Directory.Exists(inpath))
                {
                    Console.WriteLine("Invalid inpath!");
                }
                else
                {
                    if (!Directory.Exists(outpath))
                    {
                        Console.WriteLine("Invalid outpath!");
                    }
                    else
                    {
                        foreach (string image in Directory.GetFiles(inpath, "*.JPG", SearchOption.AllDirectories))
                        {
                            ProcessImage(image, outpath);
                        }
                        foreach (string image in Directory.GetFiles(inpath, "*.jpg", SearchOption.AllDirectories))
                        {
                            ProcessImage(image, outpath);
                        }
                    }
                }
            }
        }


        private static string DateToYear(string outpath, DateTime datePicture)
        {
            return Path.Combine(outpath,String.Format("{0:yyyy}", datePicture));
        }

        private static string DateToYearMonth(string outpath, DateTime datePicture)
        {
            return Path.Combine(DateToYear(outpath, datePicture),String.Format("{0:yyyyMM}", datePicture));
        }
        
        private static string DateToFile(string outpath, DateTime datePicture, int i)
        {
            return Path.Combine(DateToYearMonth(outpath, datePicture),String.Format("{0:yyyyMMdd_HHmmss}", datePicture) + i.ToString("D2") + ".JPG");
        }

        //My SetProperty code... (for ASCII property items only!)
        //Exif 2.2 requires that ASCII property items terminate with a null (0x00).
        private static void SetProperty(ref System.Drawing.Imaging.PropertyItem prop, int iId, string sTxt)
        {
            int iLen = sTxt.Length + 1;
            byte[] bTxt = new Byte[iLen];
            for (int i = 0; i < iLen - 1; i++)
                bTxt[i] = (byte)sTxt[i];
            bTxt[iLen - 1] = 0x00;
            prop.Id = iId;
            prop.Type = 2;
            prop.Value = bTxt;
            prop.Len = iLen;
        }
    }
}
