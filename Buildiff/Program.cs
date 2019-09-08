using Buildiff.FileFormats;
using Microsoft.Deployment.Compression.Cab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Buildiff
{
    class Program
    {
        static void Main(string[] args)
        {
            //XmlExportContext ec = new XmlExportContext();
            ExportContext ec = new ConsoleExportContext();
            FileSystemSource.Compare("test", "test2", ec);

            /*using (var xmlTextWriter = XmlWriter.Create(Console.Out))
            {
                ec.GetResult().WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
            }*/
            Console.ReadKey();
        }
    }

}
