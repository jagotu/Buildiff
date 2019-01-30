using Microsoft.Deployment.Compression.Cab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff.FileFormats
{

    class MSCabFormat : PhysicalFileFormat
    {
        public override bool CanLoad(string file)
        {
            CabInfo ci = new CabInfo(file);
            return ci.IsValid();
        }

        public override void Compare(string oldFile, string newFile, ExportContext ec)
        {
            CabInfo oldCab = new CabInfo(oldFile);
            CabInfo newCab = new CabInfo(newFile);

            IList<CabFileInfo> oldFiles = oldCab.GetFiles();
            IList<CabFileInfo> newFiles = newCab.GetFiles();

            foreach (CabFileInfo newSubfile in newFiles)
            {
                CabFileInfo oldSubfile = oldFiles.FirstOrDefault(x => x.Name == newSubfile.Name);
                if (oldSubfile == null)
                {
                    ec.ReportChild(newSubfile.Name, CompareResult.Added);
                }
                else
                {
                    ec.Enter(newSubfile.Name);
                    FileFormatGuesser.HashGuessCompare(oldSubfile.OpenRead(), newSubfile.OpenRead(), newSubfile.Name, ec);
                    ec.Leave();
                }
            }

            foreach (CabFileInfo oldSubfile in oldFiles)
            {
                if (!newFiles.Any(x => x.Name == oldSubfile.Name))
                {
                    ec.ReportChild(oldSubfile.Name, CompareResult.Removed);
                }

            }
        }
    }
}
