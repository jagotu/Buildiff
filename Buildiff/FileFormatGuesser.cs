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
            { "cab", new IFileFormat[] {new MSCabFormat()} },
            { "bmp", new IFileFormat[] {new ImageFormat()} },
            { "gif", new IFileFormat[] {new ImageFormat()} },
            { "jpg", new IFileFormat[] {new ImageFormat()} },
            { "jpeg", new IFileFormat[] {new ImageFormat()} },
            { "png", new IFileFormat[] {new ImageFormat()} },
            { "tiff", new IFileFormat[] {new ImageFormat()} },
            { "exe", new IFileFormat[] {new CILFormat()} },
            { "dll", new IFileFormat[] {new CILFormat()} }
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
                    ec.ReportExtra(Extras.Format, format.ToString());
                    format.Compare(oldFile, newFile, ec);
                }
            }
        }

        public static void HashGuessCompare(Stream oldFile, Stream newFile, string filename, ExportContext ec)
        {
            bool memoryStreamsCreated = false;
            Stream oldSeekable;
            Stream newSeekable;
            if (!oldFile.CanSeek || !newFile.CanSeek)
            {
                //Guessing requires ablity to seek back to beginning
                oldSeekable = new MemoryStream();
                newSeekable = new MemoryStream();
                oldFile.CopyTo(oldSeekable);
                newFile.CopyTo(newSeekable);
                oldSeekable.Seek(0, SeekOrigin.Begin);
                newSeekable.Seek(0, SeekOrigin.Begin);
                memoryStreamsCreated = true;
            } else
            {
                oldSeekable = oldFile;
                newSeekable = newFile;
            }
        

            if (UnknownBinaryFormat.HashCompare(oldFile, newFile))
            {
                ec.ReportSelf(CompareResult.Identical);
            }
            else
            {
                oldSeekable.Seek(0, SeekOrigin.Begin);
                IFileFormat format = FileFormatGuesser.GuessFileFormat(filename, oldFile);
                if (format == null || !format.CanLoad(newFile))
                {
                    ec.ReportSelf(CompareResult.Modified);
                }
                else
                {
                    oldSeekable.Seek(0, SeekOrigin.Begin);
                    newSeekable.Seek(0, SeekOrigin.Begin);
                    ec.ReportExtra(Extras.Format, format.ToString());
                    format.Compare(oldSeekable, newSeekable, ec);
                }
            }

            if(memoryStreamsCreated)
            {
                oldSeekable.Dispose();
                newSeekable.Dispose();
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
