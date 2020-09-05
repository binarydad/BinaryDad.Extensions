using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BinaryDad.Extensions
{
    public static class DirectoryExtensions
    {
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo directory, params string[] extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            return directory
                .EnumerateFiles()
                .Where(f => extensions.Contains(f.Extension, StringComparison.OrdinalIgnoreCase));
        }
    }
}
