using System.Collections.Generic;
using System.Data;
using Dapper;
using LibX4.FileSystem;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// MOD 情報保存用クラス
    /// </summary>
    class ModInfoExporter : IExporter
    {
        /// <summary>
        /// 保存する MOD 情報
        /// </summary>
        private IEnumerable<ModInfo> _ModInfos;


        /// <summary>
        /// MOD 情報保存用クラスを初期化
        /// </summary>
        /// <param name="modInfos">保存する MOD 情報</param>
        public ModInfoExporter(IEnumerable<ModInfo> modInfos) => _ModInfos = modInfos;


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="connection">データベースとの接続</param>
        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
CREATE TABLE ModInfo
(
    ID      TEXT    NOT NULL PRIMARY KEY,
    Name    TEXT    NOT NULL,
    Author  TEXT    NOT NULL,
    Version INTEGER NOT NULL,
    Date    TEXT    NOT NULL,
    Enabled BOOLEAN NOT NULL,
    Save    BOOLEAN NOT NULL
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                SqlMapper.AddTypeHandler(new ModVersionTypeHandler());
                connection.Execute("INSERT INTO ModInfo VALUES (@ID, @Name, @Author, @Version, @Date, @Enabled, @Save)", _ModInfos);
            }
        }


        /// <summary>
        /// Dappar が ModVersion と int を相互変換するためのクラス
        /// </summary>
        private class ModVersionTypeHandler : SqlMapper.TypeHandler<ModVersion>
        {
            /// <inheritdoc />
            public override ModVersion Parse(object value) => ModVersion.Parse(value.ToString());


            /// <inheritdoc />
            public override void SetValue(IDbDataParameter parameter, ModVersion value)
            {
                parameter.DbType = DbType.Int32;
                parameter.Value = value.ToInteger();
            }
        }
    }
}
