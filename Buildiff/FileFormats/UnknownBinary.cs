using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff.FileFormats
{
    class UnknownBinaryFormat
    {
        static MD5 md5 = MD5.Create();

        public static bool HashCompare(string oldFile, string newFile)
        {

            bool result;
            using (FileStream oldStream = new FileStream(oldFile, FileMode.Open, FileAccess.Read))
            using (FileStream newStream = new FileStream(newFile, FileMode.Open, FileAccess.Read))
            {
                result = HashCompare(oldStream, newStream);
            }
            return result;
        }

        public static bool HashCompare(Stream oldFile, Stream newFile)
        {
            byte[] oldHash;
            byte[] newHash;
            
            oldHash = md5.ComputeHash(oldFile);
            newHash = md5.ComputeHash(newFile);
            return newHash.SequenceEqual(oldHash);
        }

    }

}
