using System.IO;
using System.Threading.Tasks;

namespace BinaryDad.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] GetBytes(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);

                return memoryStream.ToArray();
            }
        }

        public static async Task<byte[]> GetBytesAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }
    }
}
