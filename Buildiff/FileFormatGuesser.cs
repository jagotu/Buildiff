using Buildiff.FileFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff
{
    class FileFormatGuesser
    {
        static Dictionary<string, IFileFormat[]> formats = new Dictionary<string, IFileFormat[]>
        {
            { "___", new IFileFormat[] {new MSCabFormat()} },
            { "cab", new IFileFormat[] {new MSCabFormat()} }
        };


        public static void HashGuessCompare(string oldFile, string newFile, ExportContext ec)
        {
            if (UnknownBinaryFormat.HashCompare(oldFile, newFile))
            {
                ec.ReportSelf(CompareResult.Identical);
            }
            else
            {
                IFileFormat format = FileFormatGuesser.GuessFileFormat(oldFile);
                if (format == null || !format.CanLoad(newFile))
                {
                    ec.ReportSelf(CompareResult.Modified);
                }
                else
                {
                    format.Compare(oldFile, newFile, ec);
                }
            }
        }

        public static void HashGuessCompare(Stream oldFile, Stream newFile, string filename, ExportContext ec)
        {
            if (UnknownBinaryFormat.HashCompare(oldFile, newFile))
            {
                ec.ReportSelf(CompareResult.Identical);
            }
            else
            {
                IFileFormat format = FileFormatGuesser.GuessFileFormat(filename, oldFile);
                if (format == null || !format.CanLoad(newFile))
                {
                    ec.ReportSelf(CompareResult.Modified);
                }
                else
                {
                    format.Compare(oldFile, newFile, ec);
                }
            }
        }



        public static IFileFormat GuessFileFormat(string filename)
        {
            string extension = Path.GetExtension(filename).ToLower().Replace(".", "");
            if(extension.EndsWith("_"))
            {
                extension = "___";
            }

            if(formats.ContainsKey(extension))
            {
                foreach(IFileFormat format in formats[extension])
                {
                    if (format.CanLoad(filename))
                        return format;
                }
            }

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return GuessFileFormat(fs);
            }
                
        }
        public static IFileFormat GuessFileFormat(string filename, Stream file)
        {
            string extension = Path.GetExtension(filename).ToLower().Replace(".", ""); ;
            if (extension.EndsWith("_"))
            {
                extension = "___";
            }

            if (formats.ContainsKey(extension))
            {
                foreach (IFileFormat format in formats[extension])
                {
                    if (format.CanLoad(file))
                        return format;
                }
            }

            return GuessFileFormat(file);
        }

        public static IFileFormat GuessFileFormat(Stream file)
        {
            //Guess by actual contents?
            return null;
        }



    }
}
