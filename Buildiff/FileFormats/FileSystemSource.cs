using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff.FileFormats
{
    class FileSystemSource
    {
        public static void Compare(string oldPath, string newPath, ExportContext ec)
        {
            if (!Directory.Exists(newPath))
            {
                ec.ReportSelf(CompareResult.Removed);
            }
            else if (!Directory.Exists(oldPath))
            {
                ec.ReportSelf(CompareResult.Added);
            }
            else
            {
                foreach (string newSubdir in Directory.GetDirectories(newPath))
                { 
                    ec.Enter(Path.GetFileName(newSubdir));
                    Compare(newSubdir.Replace(newPath, oldPath), newSubdir, ec);
                    ec.Leave();
                }

                foreach (string oldSubdir in Directory.GetDirectories(oldPath))
                {
                    if (Directory.Exists(oldSubdir.Replace(oldPath, newPath))) continue;
                    ec.Enter(Path.GetFileName(oldSubdir));
                    Compare(oldSubdir, oldSubdir.Replace(oldPath, newPath), ec);
                    ec.Leave();
                }

                foreach (string newFile in Directory.GetFiles(newPath))
                {
                    string oldFile = newFile.Replace(newPath, oldPath);
                    if (!File.Exists(oldFile))
                    {
                        ec.ReportChild(Path.GetFileName(newFile), CompareResult.Added);
                    }
                    else
                    {
                        ec.Enter(Path.GetFileName(newFile));
                        FileFormatGuesser.HashGuessCompare(oldFile, newFile, ec);
                        ec.Leave();

                    }
                }

                foreach (string oldFile in Directory.GetFiles(oldPath))
                {
                    string newFile = oldFile.Replace(oldPath, newPath);
                    if (!File.Exists(newFile))
                    {
                        ec.ReportChild(Path.GetFileName(newFile), CompareResult.Removed);
                    }
                }
            }




        }
    }
}
