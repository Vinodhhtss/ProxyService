using System.IO;
using System.Text;

namespace HTTPDataAnalyzer
{
    public class Decompressor
    {
        public static byte[] DecompressGzip(Stream input, Encoding e)
        {
            byte[] tempOutput;
            using (System.IO.Compression.GZipStream decompressor = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
            {
                int read = 0;
                var buffer = new byte[375];

                using (MemoryStream output = new MemoryStream())
                {
                    while ((read = decompressor.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                    tempOutput = output.ToArray();
                }
            }
            return tempOutput;
        }
    }
}
