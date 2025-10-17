/*
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UTF8Conv
{
    public static class Utf8Helper
    {
        public static string Decode(byte[] utf8Bytes)
        {
            var chars = new List<char>();
            int i = 0;
            while (i < utf8Bytes.Length)
            {
                byte b = utf8Bytes[i];
                if (b < 0x80)
                {
                    chars.Add((char)b);
                    i++;
                }
                else if ((b & 0xE0) == 0xC0)
                {
                    char c = (char)(((b & 0x1F) << 6) | (utf8Bytes[i + 1] & 0x3F));
                    chars.Add(c);
                    i += 2;
                }
                else if ((b & 0xF0) == 0xE0)
                {
                    char c = (char)(((b & 0x0F) << 12) |
                                    ((utf8Bytes[i + 1] & 0x3F) << 6) |
                                    (utf8Bytes[i + 2] & 0x3F));
                    chars.Add(c);
                    i += 3;
                }
                else
                {
                    chars.Add('?'); // unsupported char
                    i++;
                }
            }
            return new string(chars.ToArray());
        }
    }

}
*/