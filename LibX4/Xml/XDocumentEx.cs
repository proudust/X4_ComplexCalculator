using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace LibX4.Xml
{
    /// <summary>
    /// XML v1.1 を扱える XDocument.Load を提供するクラス
    /// </summary>
    internal static class XDocumentEx
    {
        /// <summary>
        /// XML 宣言の文頭を UTF-8 で表した配列
        /// </summary>
        private static readonly byte[] Utf8XmlDeclaration = Encoding.UTF8.GetBytes(XmlDeclaration);


        /// <summary>
        /// XML 宣言の文頭を UTF-16 LE で表した配列
        /// </summary>
        private const string XmlDeclaration = "<?xml";


        /// <summary>
        /// XML 宣言の文頭を UTF-16 BE で表した配列
        /// </summary>
        private static readonly byte[] Utf16BEXmlDeclaration
            = new byte[] { 0x00, 0x3C, 0x00, 0x3F, 0x00, 0x78, 0x00, 0x6D, 0x00, 0x6C };


        /// <summary>
        /// 絶対パスから XDocument を生成する
        /// </summary>
        /// <param name="url">ファイル名</param>
        /// <returns>生成した XDocument</returns>
        public static XDocument Load(string url)
        {
            using var stream = new FileStream(url, FileMode.Open, FileAccess.Read);
            return Load(stream);
        }


        /// <summary>
        /// ストリームから XDocument を生成する
        /// </summary>
        /// <param name="stream">XML データのストリーム</param>
        /// <returns>生成した XDocument</returns>
        public static XDocument Load(Stream stream) => XDocument.Load(SkipXmlDeclaration(stream));


        /// <summary>
        /// ストリームの XML 宣言を読み飛ばす
        /// </summary>
        /// <param name="stream">XML のストリーム</param>
        /// <returns>XML 宣言を読み飛ばしたストリーム</returns>
        private static Stream SkipXmlDeclaration(Stream stream)
        {
            Span<byte> buff = stackalloc byte[112]; // XML 宣言の全属性を指定した場合の文字数
            stream.Read(buff);

            var head = MemoryMarshal.Read<(byte, byte)>(buff[0..2]);

            return MemoryMarshal.Read<(byte, byte)>(buff[0..2]) switch
            {
                (0xEF, 0xBB) => SkipUtf8XmlDeclaration(stream, buff, true),     // UTF-8 BOM
                (0x3C, 0x00) => SkipUtf16LEXmlDeclaration(stream, buff),        // UTF-16 LE
                (0x3C, _) => SkipUtf8XmlDeclaration(stream, buff),              // UTF-8
                (0x00, 0x3C) => SkipUtf16BEXmlDeclaration(stream, buff),        // UTF-16 BE
                (0xFF, 0xFE) => SkipUtf16LEXmlDeclaration(stream, buff, true),  // UTF-16 LE BOM
                (0xFE, 0xFF) => SkipUtf16BEXmlDeclaration(stream, buff, true),  // UTF-16 BE BOM
                _ => throw new NotSupportedException(),
            };
        }


        /// <summary>
        /// UTF-8 エンコードされた XML ストリームの XML 宣言を読み飛ばす
        /// </summary>
        /// <param name="stream">XML のストリーム</param>
        /// <param name="bom">BOM 付きの場合 true</param>
        /// <returns>XML 宣言を読み飛ばしたストリーム</returns>
        private static Stream SkipUtf8XmlDeclaration(Stream stream, ReadOnlySpan<byte> buff,
                                                     bool bom = false)
        {
            int seek = bom ? Encoding.UTF8.Preamble.Length : 0;

            // XML 宣言が省略されている場合はそのまま返す
            if (!buff.Slice(seek).StartsWith(Utf8XmlDeclaration))
            {
                stream.Position = seek;
                return stream;
            }

            // XML 宣言部分を読み飛ばす
            for (seek += 6; seek < buff.Length; seek++)
            {
                switch (buff[seek])
                {
                    case (byte)'v':
                        seek += 12; // skip 'version="1.x"'
                        break;

                    case (byte)'e':
                        seek += 13; // skip 'encoding="UTF-8"'
                        break;

                    case (byte)'s':
                        seek += 14; // skip 'standalone="no"'
                        break;

                    case (byte)'?':
                        seek += 2;  // skip '?>'
                        stream.Position = seek;
                        return stream;
                }
            }
            throw new InvalidDataException("XML declaration has unexpected length."
                + Environment.NewLine + $"Buff: {Encoding.UTF8.GetString(buff)}");
        }


        /// <summary>
        /// UTF-16 LE エンコードされた XML ストリームの XML 宣言を読み飛ばす
        /// </summary>
        /// <param name="stream">XML のストリーム</param>
        /// <param name="bom">BOM 付きの場合 true</param>
        /// <returns>XML 宣言を読み飛ばしたストリーム</returns>
        private static Stream SkipUtf16LEXmlDeclaration(Stream stream, ReadOnlySpan<byte> buff,
                                                     bool bom = false)
        {
            var chars = MemoryMarshal.Cast<byte, char>(buff);

            int seek = bom ? 1 : 0;

            // XML 宣言が省略されている場合はそのまま返す
            if (!chars.Slice(seek).StartsWith(XmlDeclaration))
            {
                stream.Position = bom ? Encoding.Unicode.Preamble.Length : 0;
                return stream;
            }

            // XML 宣言部分を読み飛ばす
            for (seek += 6; seek < chars.Length; seek++)
            {
                switch (chars[seek])
                {
                    case 'v':
                        seek += 12; // skip 'version="1.x"'
                        break;

                    case 'e':
                        seek += 13; // skip 'encoding="UTF-8"'
                        break;

                    case 's':
                        seek += 14; // skip 'standalone="no"'
                        break;

                    case '?':
                        seek += 2;  // skip '?>'
                        stream.Position = seek * 2;
                        return stream;
                }
            }
            throw new InvalidDataException("XML declaration has unexpected length."
                + Environment.NewLine + $"Buff: {chars.ToString()}");
        }


        /// <summary>
        /// UTF-16 BE エンコードされた XML ストリームの XML 宣言を読み飛ばす
        /// </summary>
        /// <param name="stream">XML のストリーム</param>
        /// <param name="bom">BOM 付きの場合 true</param>
        /// <returns>XML 宣言を読み飛ばしたストリーム</returns>
        private static Stream SkipUtf16BEXmlDeclaration(Stream stream, ReadOnlySpan<byte> buff,
                                                        bool bom = false)
        {
            int seek = bom ? Encoding.BigEndianUnicode.Preamble.Length : 0;

            // XML 宣言が省略されている場合はそのまま返す
            if (!buff.Slice(seek).StartsWith(Utf16BEXmlDeclaration))
            {
                stream.Position = bom ? Encoding.Unicode.Preamble.Length : 0;
                return stream;
            }

            // XML 宣言部分を読み飛ばす
            for (seek += 13; seek < buff.Length; seek += 2)
            {
                switch (buff[seek])
                {
                    case (byte)'v':
                        seek += 12 * 2; // skip 'version="1.x"'
                        break;

                    case (byte)'e':
                        seek += 13 * 2; // skip 'encoding="UTF-8"'
                        break;

                    case (byte)'s':
                        seek += 14 * 2; // skip 'standalone="no"'
                        break;

                    case (byte)'?':
                        seek += 3;      // skip '?>'
                        stream.Position = seek;
                        return stream;
                }
            }
            throw new InvalidDataException("XML declaration has unexpected length."
                + Environment.NewLine + $"Buff: {Encoding.BigEndianUnicode.GetString(buff)}");
        }
    }
}
