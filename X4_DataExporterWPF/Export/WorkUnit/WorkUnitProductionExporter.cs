﻿using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using LibX4.Xml;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 従業員用生産情報抽出用クラス
    /// </summary>
    class WorkUnitProductionExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="waresXml">ウェア情報xml</param>
        public WorkUnitProductionExporter(XDocument waresXml)
        {
            _WaresXml = waresXml;
        }


        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="connection"></param>
        public void Export(IDbConnection connection)
        {
            //////////////////
            // テーブル作成 //
            //////////////////
            {
                connection.Execute(@"
CREATE TABLE IF NOT EXISTS WorkUnitProduction
(
    WorkUnitID  TEXT    NOT NULL,
    Time        INTEGER NOT NULL,
    Amount      INTEGER NOT NULL,
    Method      TEXT    NOT NULL,
    PRIMARY KEY (WorkUnitID, Method)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO WorkUnitProduction (WorkUnitID, Time, Amount, Method) VALUES (@WorkUnitID, @Time, @Amount, @Method)", items);
            }
        }


        /// <summary>
        /// XML から WorkUnitProduction データを読み出す
        /// </summary>
        /// <returns>読み出した WorkUnitProduction データ</returns>
        private IEnumerable<WorkUnitProduction> GetRecords()
        {
            foreach (var workUnit in _WaresXml.Root.XPathSelectElements("ware[@transport='workunit']"))
            {
                var workUnitID = workUnit.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(workUnitID)) continue;

                foreach (var prod in workUnit.XPathSelectElements("production"))
                {
                    var time = prod.Attribute("time").GetInt();
                    var amount = prod.Attribute("amount").GetInt();

                    var method = prod.Attribute("method")?.Value;
                    if (string.IsNullOrEmpty(method)) continue;

                    yield return new WorkUnitProduction(workUnitID, time, amount, method);
                }
            }
        }
    }
}
