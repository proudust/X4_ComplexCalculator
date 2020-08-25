using System.IO;
using System.Linq;
using System.Text;
using LibX4.Xml;
using Xunit;

namespace LibX4.Tests
{
    /// <summary>
    /// <see cref="XDocumentEx"/> のテストクラス
    /// </summary>
    public class XDocumentExTest
    {
        /// <summary>
        /// UTF-8 の BOM
        /// </summary>
        private static readonly byte[] Utf8Bom = Encoding.UTF8.GetPreamble();


        /// <summary>
        /// UTF-16 LE の BOM
        /// </summary>
        private static readonly byte[] Utf16LEBom = Encoding.Unicode.GetPreamble();


        /// <summary>
        /// UTF-16 BE の BOM
        /// </summary>
        private static readonly byte[] Utf16BEBom = Encoding.BigEndianUnicode.GetPreamble();


        /// <summary>
        /// テスト用の XML 文字列の配列
        /// </summary>
        public static string[][] TestXmls => new[] {
            new [] { "<root></root>" },
            new [] { @"<?xml version=""1.1""?><root></root>" },
            new [] { @"<?xml version=""1.1"" encoding=""UTF-8""?><root></root>" },
            new [] { @"<?xml version=""1.1"" encoding=""UTF-8"" standalone=""no""?><root></root>" },
            new [] { @"<?xml version=""1.1"" encoding=""UTF-8"" standalone=""yes""?><root></root>" },
        };


        /// <summary>
        /// UTF-8 エンコードされた XML ストリームから読み込める
        /// </summary>
        /// <param name="source">テスト用の文字列</param>
        [Theory]
        [MemberData(nameof(TestXmls))]
        public void Utf8(string source)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(source));
            XDocumentEx.Load(stream);
        }


        /// <summary>
        /// UTF-8 with BOM エンコードされた XML ストリームから読み込める
        /// </summary>
        /// <param name="source">テスト用の文字列</param>
        [Theory]
        [MemberData(nameof(TestXmls))]
        public void Utf8WithBom(string source)
        {
            var stream = new MemoryStream(Utf8Bom.Concat(Encoding.UTF8.GetBytes(source)).ToArray());
            XDocumentEx.Load(stream);
        }


        /// <summary>
        /// UTF-16 LE エンコードされた XML ストリームから読み込める
        /// </summary>
        /// <param name="source">テスト用の文字列</param>
        [Theory]
        [MemberData(nameof(TastXmls))]
        public void Utf16LE(string source)
        {
            var stream = new MemoryStream(Encoding.Unicode.GetBytes(source));
            XDocumentEx.Load(stream);
        }


        /// <summary>
        /// UTF-16 LE with BOM エンコードされた XML ストリームから読み込める
        /// </summary>
        /// <param name="source">テスト用の文字列</param>
        [Theory]
        [MemberData(nameof(TastXmls))]
        public void Utf16LEWithBom(string source)
        {
            var bytes = Encoding.Unicode.GetBytes(source);
            var stream = new MemoryStream(Utf16LEBom.Concat(bytes).ToArray());
            XDocumentEx.Load(stream);
        }


        /// <summary>
        /// UTF-16 BE エンコードされた XML ストリームから読み込める
        /// </summary>
        /// <param name="source">テスト用の文字列</param>
        [Theory]
        [MemberData(nameof(TastXmls))]
        public void Utf16BE(string source)
        {
            var stream = new MemoryStream(Encoding.BigEndianUnicode.GetBytes(source));
            XDocumentEx.Load(stream);
        }


        /// <summary>
        /// UTF-16 BE with BOM エンコードされた XML ストリームから読み込める
        /// </summary>
        /// <param name="source">テスト用の文字列</param>
        [Theory]
        [MemberData(nameof(TastXmls))]
        public void Utf16BEWithBom(string source)
        {
            var bytes = Encoding.BigEndianUnicode.GetBytes(source);
            var stream = new MemoryStream(Utf16BEBom.Concat(bytes).ToArray());
            XDocumentEx.Load(stream);
        }
    }
}
