using System;
using System.IO;
using LibX4.Xml;

namespace LibX4.FileSystem
{

    /// <summary>
    /// Modの情報
    /// </summary>
    public class ModInfo
    {
        /// <summary>
        /// 識別ID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; }


        /// <summary>
        /// バージョン
        /// </summary>
        public ModVersion Version { get; }


        /// <summary>
        /// 作成日時
        /// </summary>
        public string Date { get; }


        /// <summary>
        /// 有効化
        /// </summary>
        public bool Enabled { get; }


        /// <summary>
        /// セーブデータの互換性
        /// </summary>
        public bool Save { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modDirPath">Modのフォルダパス(絶対パスで指定すること)</param>
        public ModInfo(string modDirPath)
        {
            var contentXmlPatth = Path.Combine(modDirPath, "content.xml");
            var xml = XDocumentEx.Load(contentXmlPatth);

            ID      = xml.Root.Attribute("id")?.Value      ?? "";
            Name    = xml.Root.Attribute("name")?.Value    ?? "";
            Author  = xml.Root.Attribute("author")?.Value  ?? "";
            Version = ModVersion.Parse(xml.Root.Attribute("version")?.Value);
            Date    = xml.Root.Attribute("date")?.Value    ?? "";
            Enabled = ParseBoolean(xml.Root.Attribute("enabled")?.Value);
            Save    = ParseBoolean(xml.Root.Attribute("save")?.Value);
        }


        /// <summary>
        /// 真偽値を X4 の使用に合わせて整形する
        /// </summary>
        /// <param name="booleanString">記載された真偽値</param>
        /// <returns>整形後のバージョン名</returns>
        private bool ParseBoolean(string? booleanString)
            => booleanString == "1"
            || string.Equals(booleanString, "true", StringComparison.OrdinalIgnoreCase);
    }
}
