using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff.FileFormats
{
    interface IFileFormat
    {
        void Compare(string oldFile, string newFile, ExportContext ec);
        void Compare(Stream oldFile, Stream newFile, ExportContext ec);

        bool CanLoad(string file);

        bool CanLoad(Stream file);
    }

    abstract class PhysicalFileFormat : IFileFormat
    {
        public abstract bool CanLoad(string file);

        public abstract void Compare(string oldFile, string newFile, ExportContext ec);

        public virtual void Compare(Stream oldFile, Stream newFile, ExportContext ec)
        {
            string filename = Path.Combine(Path.GetTempPath(), "Buildiff\\", Path.GetRandomFileName());
            using (FileStream fs = new FileStream(filename + "_old", FileMode.Create))
            {
                oldFile.CopyTo(fs);
            }
            using (FileStream fs = new FileStream(filename + "_new", FileMode.Create))
            {
                newFile.CopyTo(fs);
            }
            Compare(filename + "_old", filename + "_new", ec);

            File.Delete(filename + "_old");
            File.Delete(filename + "_new");
        }

        public virtual bool CanLoad(Stream file)
        {
            string filename = Path.Combine(Path.GetTempPath(), "Buildiff\\", Path.GetRandomFileName());
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                file.CopyTo(fs);
            }
            bool result = CanLoad(filename);
            File.Delete(filename);

            return result;
        }

    }

    abstract class StreamableFileFormat : IFileFormat
    {
        public virtual bool CanLoad(string file)
        {
            using (FileStream FS = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                return CanLoad(FS);
            }
        }

        public abstract bool CanLoad(Stream file);

        public virtual void Compare(string oldFile, string newFile, ExportContext ec)
        {
            using (FileStream oldFS = new FileStream(oldFile, FileMode.Open, FileAccess.Read))
            using (FileStream newFS = new FileStream(newFile, FileMode.Open, FileAccess.Read))
            {
                Compare(oldFS, newFS, ec);
            }
        }

        public abstract void Compare(Stream oldFile, Stream newFile, ExportContext ec);
    }
}
