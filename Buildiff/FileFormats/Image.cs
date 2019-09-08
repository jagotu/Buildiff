using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildiff.FileFormats
{
    class ImageFormat : StreamableFileFormat
    {
        public override bool CanLoad(Stream file)
        {
            try
            {
                Bitmap bmp = (Bitmap)Image.FromStream(file);
                bmp.Dispose();
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            return true;
        }

        private void CopyDim(PixelFormat format, byte[] input, int inputoffset, byte[] output, int outputoffset)
        {
            int i;
            switch (format)
            {
                case PixelFormat.Format24bppRgb:
                    for (i = 0; i < 3; i++) output[outputoffset + i] = (byte)(input[inputoffset + i] / 2);
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    for (i = 1; i < 4; i++) output[outputoffset + i] = (byte)(input[inputoffset + i] / 2);
                    break;
                case PixelFormat.Format48bppRgb:
                    for (i = 0; i < 6; i++) output[outputoffset + i] = (byte)(input[inputoffset + i] / 2);
                    break;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    for (i = 0; i < 6; i++) output[outputoffset + i] = (byte)(input[inputoffset + i] / 2);
                    break;
                default:
                    throw new InvalidOperationException($"PixelFormat {format.ToString()} not supported.");

            }
        }

        private bool PixelEqual(int pixelPerByte, byte[] input1, byte[] input2, int inputoffset)
        {
            for (int i = inputoffset; i < inputoffset + pixelPerByte; i++)
            {
                if (input1[i] != input2[i]) return false;
            }
            return true;
        }

        public void Compare(Bitmap oldBMP, Bitmap newBMP, ExportContext ec)
        {
            string diffPath = ec.GetCurrentPath();

            // If they are the same resolution and pixel format, do a pixel-by-pixel comparison
            if (oldBMP.Width == newBMP.Width && oldBMP.Height == newBMP.Height && oldBMP.PixelFormat == newBMP.PixelFormat)
            {
                BitmapData oldBMPData = oldBMP.LockBits(new Rectangle(0, 0, oldBMP.Width, oldBMP.Height), ImageLockMode.ReadOnly, oldBMP.PixelFormat);
                BitmapData newBMPData = newBMP.LockBits(new Rectangle(0, 0, newBMP.Width, newBMP.Height), ImageLockMode.ReadOnly, newBMP.PixelFormat);

                int perPixelBytes = Math.Abs(oldBMPData.Stride / oldBMPData.Width);
                int stride = oldBMPData.Stride;

                int bytes = Math.Abs(oldBMPData.Stride) * oldBMPData.Height;
                byte[] oldrgbValues = new byte[bytes];
                byte[] newrgbValues = new byte[bytes];
                byte[] diffrgbValues = new byte[bytes * 2];
                System.Runtime.InteropServices.Marshal.Copy(oldBMPData.Scan0, oldrgbValues, 0, bytes);
                System.Runtime.InteropServices.Marshal.Copy(newBMPData.Scan0, newrgbValues, 0, bytes);
                oldBMP.UnlockBits(oldBMPData);
                newBMP.UnlockBits(newBMPData);


                bool different = false;


                for (int line = 0; line < oldBMP.Height; line += 1)
                {
                    for (int j = 0; j < stride; j += perPixelBytes)
                    {
                        if (PixelEqual(perPixelBytes, oldrgbValues, newrgbValues, line * stride + j))
                        {
                            CopyDim(oldBMP.PixelFormat, oldrgbValues, line * stride + j, diffrgbValues, line * stride * 2 + j);
                            CopyDim(newBMP.PixelFormat, oldrgbValues, line * stride + j, diffrgbValues, line * stride * 2 + stride + j);
                        }
                        else
                        {
                            different = true;
                            Array.Copy(oldrgbValues, line * stride + j, diffrgbValues, line * stride * 2 + j, perPixelBytes);
                            Array.Copy(newrgbValues, line * stride + j, diffrgbValues, line * stride * 2 + stride + j, perPixelBytes);
                        }
                    }

                }

                if (different)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(diffPath));

                    Bitmap diff = new Bitmap(oldBMP.Width * 2, oldBMP.Height, oldBMP.PixelFormat);
                    BitmapData diffData = diff.LockBits(new Rectangle(0, 0, diff.Width, diff.Height), ImageLockMode.WriteOnly, diff.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(diffrgbValues, 0, diffData.Scan0, bytes * 2);
                    diff.UnlockBits(diffData);
                    diff.Save(diffPath);
                    ec.ReportExtra(Extras.DetailedDiff, diffPath);
                    ec.ReportSelf(CompareResult.Modified);
                }
                else
                {
                    ec.ReportSelf(CompareResult.Identical);
                }


            }
            else
            {
                ec.ReportSelf(CompareResult.Modified);
            }


        }

        public override void Compare(Stream oldFile, Stream newFile, ExportContext ec)
        {

            using (Bitmap oldBMP = (Bitmap)Image.FromStream(oldFile))
            using (Bitmap newBMP = (Bitmap)Image.FromStream(newFile))
            {
                Compare(oldBMP, newBMP, ec);
            }
        }
    }
}
