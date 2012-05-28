using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using ExifLib;

namespace PhotoRename
{
    class Program
    {
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
            List<string> extra = p.Parse (args);
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
                            try
                            {
                                DateTime datePicture = DateTime.MinValue;
                                using (ExifReader reader = new ExifReader(image))
                                {
                                    DateTime datePictureTaken;
                                    if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken))
                                    {
                                        datePicture = datePictureTaken;
                                    }
                                }
                                if (!datePicture.Equals(DateTime.MinValue))
                                {
                                    int i = 1;
                                    while ( File.Exists(DateToFile(outpath, datePicture, i)))
                                    {
                                        i++;
                                    }
                                    //Console.WriteLine(DateToFile(outpath, datePicture, i));
                                    if (!Directory.Exists(DateToYear(outpath, datePicture)))
                                    {
                                        Directory.CreateDirectory(DateToYear(outpath, datePicture));
                                    }
                                    if (!Directory.Exists(DateToYearMonth(outpath, datePicture)))
                                    {
                                        Directory.CreateDirectory(DateToYearMonth(outpath, datePicture));
                                    }
                                    File.Move(image, DateToFile(outpath, datePicture, i));
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(image + ": " + e.Message);
                            }
                        }
                    }
                }
            }
        }


        private static string DateToYear(string outpath, DateTime datePicture)
        {
            return outpath + "\\" + String.Format("{0:yyyy}", datePicture) + "\\";
        }

        private static string DateToYearMonth(string outpath, DateTime datePicture)
        {
            return DateToYear(outpath, datePicture) + String.Format("{0:yyyyMM}", datePicture) + "\\";
        }
        
        private static string DateToFile(string outpath, DateTime datePicture, int i)
        {
            return DateToYearMonth(outpath, datePicture) + String.Format("{0:yyyyMMdd_HHmmss}", datePicture) + i.ToString("D2") + ".JPG";
        }
    }
}
