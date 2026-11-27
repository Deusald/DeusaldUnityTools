// MIT License

// DeusaldUnityTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#nullable enable

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace DeusaldUnityTools
{
    /// <summary>
    /// Minimal reader of .tgz archives (npm package tarballs): regular files and directories only.
    /// </summary>
    public static class TarArchive
    {
        #region Variables

        private const int _BLOCK_SIZE = 512;

        #endregion Variables

        #region Public Methods

        /// <summary>
        /// Extracts a gzipped tar archive into the destination folder.
        /// </summary>
        /// <param name="stripComponents">Leading path parts to drop, e.g. 1 for npm's "package/" folder.</param>
        public static void ExtractTarGz(byte[] archive, string destination, int stripComponents)
        {
            byte[]  tar             = Decompress(archive);
            string  destinationRoot = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string? longName        = null;
            int     offset          = 0;

            Directory.CreateDirectory(destinationRoot);

            while (offset + _BLOCK_SIZE <= tar.Length && !IsZeroBlock(tar, offset))
            {
                string name   = ReadString(tar, offset, 100);
                long   size   = ReadNumber(tar, offset + 124, 12);
                char   type   = (char)tar[offset + 156];
                string prefix = ReadString(tar, offset + 257, 6).StartsWith("ustar") ? ReadString(tar, offset + 345, 155) : string.Empty;

                int dataOffset = offset + _BLOCK_SIZE;
                if (size < 0 || dataOffset + size > tar.Length) throw new InvalidDataException("The package archive is truncated.");
                offset = dataOffset + (int)((size + _BLOCK_SIZE - 1) / _BLOCK_SIZE * _BLOCK_SIZE);

                switch (type)
                {
                    case 'L': // GNU long name of the next entry
                        longName = ReadString(tar, dataOffset, (int)size);
                        continue;
                    case 'x': // PAX header of the next entry
                        longName = ReadPaxPath(tar, dataOffset, (int)size) ?? longName;
                        continue;
                    case 'g': // PAX global header
                        continue;
                }

                string path = longName ?? (prefix.Length > 0 ? prefix + "/" + name : name);
                longName = null;

                bool isFile      = type == '0' || type == '\0' || type == '7';
                bool isDirectory = type == '5';
                if (!isFile && !isDirectory) continue; // links and devices are not needed in packages

                string? relative = StripComponents(path, stripComponents);
                if (relative == null) continue;

                string target = Path.GetFullPath(Path.Combine(destinationRoot, relative));
                if (!target.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"The package archive has an entry outside of its folder: {path}");

                if (isDirectory)
                {
                    Directory.CreateDirectory(target);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(target) ?? destinationRoot);
                using FileStream file = new FileStream(target, FileMode.Create, FileAccess.Write);
                file.Write(tar, dataOffset, (int)size);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static byte[] Decompress(byte[] archive)
        {
            using MemoryStream compressed   = new MemoryStream(archive);
            using GZipStream   gzip         = new GZipStream(compressed, CompressionMode.Decompress);
            using MemoryStream decompressed = new MemoryStream();
            gzip.CopyTo(decompressed);
            return decompressed.ToArray();
        }

        private static bool IsZeroBlock(byte[] data, int offset)
        {
            for (int x = 0; x < _BLOCK_SIZE; ++x)
            {
                if (data[offset + x] != 0) return false;
            }

            return true;
        }

        private static string ReadString(byte[] data, int offset, int length)
        {
            int end = offset;
            while (end < offset + length && data[end] != 0) ++end;
            return Encoding.UTF8.GetString(data, offset, end - offset);
        }

        private static long ReadNumber(byte[] data, int offset, int length)
        {
            // Large sizes use base-256: the high bit of the first byte is set.
            if ((data[offset] & 0x80) != 0)
            {
                long value = data[offset] & 0x7F;
                for (int x = 1; x < length; ++x) value = (value << 8) | data[offset + x];
                return value;
            }

            string octal = ReadString(data, offset, length).Trim(' ', '\0');
            return octal.Length == 0 ? 0 : Convert.ToInt64(octal, 8);
        }

        private static string? ReadPaxPath(byte[] data, int offset, int length)
        {
            // Records look like "<length> <key>=<value>\n".
            string records = Encoding.UTF8.GetString(data, offset, length);
            foreach (string record in records.Split('\n'))
            {
                int space  = record.IndexOf(' ');
                int equals = record.IndexOf('=');
                if (space < 0 || equals < space) continue;
                if (record.Substring(space + 1, equals - space - 1) == "path") return record.Substring(equals + 1);
            }

            return null;
        }

        private static string? StripComponents(string path, int count)
        {
            string[] parts = path.Replace('\\', '/')
                                 .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(part => part != ".")
                                 .ToArray();
            if (parts.Length <= count) return null;
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts.Skip(count));
        }

        #endregion Private Methods
    }
}
