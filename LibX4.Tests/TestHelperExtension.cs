﻿using System.Linq;
using System.Xml.Linq;

namespace LibX4.Tests
{
    /// <summary>
    /// LibX4.Tests 用のユーティリティクラス
    /// </summary>
    internal static class TestHelperExtension
    {
        /// <summary>
        /// 文字列から文頭文末の改行及び各行のインデントを取り除く
        /// </summary>
        /// <param name="source">対象の文字列</param>
        /// <returns>文頭文末の改行及び各行のインデントを取り除いた文字列</returns>
        public static string TrimIndent(this string source)
        {
            var lines = source.Split("\r\n").ToList();
            if (string.IsNullOrWhiteSpace(lines[0])) lines.RemoveAt(0);
            if (string.IsNullOrWhiteSpace(lines[^1])) lines.RemoveAt(lines.Count - 1);
            var indent = lines
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Min(l => l.TakeWhile(char.IsWhiteSpace).Count());
            return string.Join("\r\n", lines.Select(l => l[indent..]));
        }


        /// <summary>
        /// 文字列を XML として読み込む
        /// </summary>
        /// <param name="source">XML として読み込む文字列</param>
        /// <returns>読み込んだ XML</returns>
        public static XDocument ToXDocument(this string source)
            => XDocument.Parse(source.TrimIndent());
    }
}
