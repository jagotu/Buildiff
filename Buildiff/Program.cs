using Buildiff.FileFormats;
using Microsoft.Deployment.Compression.Cab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff
{
    class Program
    {
        static void Main(string[] args)
        {
            ExportContext ec = new ConsoleExportContext();
            FileSystemSource.Compare("H:\\", "G:\\", ec);

        }
    }

}
