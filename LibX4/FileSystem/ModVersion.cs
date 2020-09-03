using System;

namespace LibX4.FileSystem
{
    /// <summary>
    /// Mod のバージョンを表す値オブジェクト
    /// </summary>
    public class ModVersion
    {
        /// <summary>
        /// メジャーバージョン
        /// </summary>
        public short Major { get; }


        /// <summary>
        /// マイナーバージョン
        /// </summary>
        public byte Minor { get; }


        /// <summary>
        /// Mod のバージョンを表す値オブジェクトを生成する
        /// </summary>
        /// <param name="major">メジャーバージョン</param>
        /// <param name="minor">マイナーバージョン</param>
        public ModVersion(short major, byte minor)
        {
            this.Major = major;
            this.Minor = minor;
        }


        /// <summary>
        /// content.xml や version.bat に書かれたバージョン文字列を ModVersion に変換する
        /// </summary>
        /// <param name="s">content.xml や version.bat に書かれたバージョン文字列</param>
        /// <returns>文字列と等しい ModVersion オブジェクト</returns>
        public static ModVersion Parse(ReadOnlySpan<char> s)
        {
            int digitLength = 0;
            while (++digitLength < s.Length && char.IsDigit(s[digitLength])) { }

            if (digitLength == 0) return new ModVersion(0, 0);

            var digits = s.Slice(0, digitLength);

            if (digitLength < 3)
            {
                var minor = byte.Parse(digits);
                return new ModVersion(0, minor);
            }
            else
            {
                var major = short.Parse(digits[..^2]);
                var minor = byte.Parse(digits[^2..]);
                return new ModVersion(major, minor);
            }
        }


        /// <inheritdoc />
        public override string ToString() => $"{Major}.{Minor:00}";

    }
}
