﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Dapper;
using X4_DataExporterWPF.Entity;

namespace X4_DataExporterWPF.Export
{
    /// <summary>
    /// 装備保有派閥抽出用クラス
    /// </summary>
    class EquipmentOwnerExporter : IExporter
    {
        /// <summary>
        /// ウェア情報xml
        /// </summary>
        private readonly XDocument _WaresXml;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="xml">ウェア情報xml</param>
        public EquipmentOwnerExporter(XDocument xml)
        {
            _WaresXml = xml;
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
CREATE TABLE IF NOT EXISTS EquipmentOwner
(
    EquipmentID TEXT    NOT NULL,
    FactionID   TEXT    NOT NULL,
    PRIMARY KEY (EquipmentID, FactionID),
    FOREIGN KEY (EquipmentID)   REFERENCES Equipment(EquipmentID),
    FOREIGN KEY (FactionID)     REFERENCES Faction(FactionID)
) WITHOUT ROWID");
            }


            ////////////////
            // データ抽出 //
            ////////////////
            {
                var items = GetRecords();

                connection.Execute("INSERT INTO EquipmentOwner (EquipmentID, FactionID) VALUES (@EquipmentID, @FactionID)", items);
            }
        }


        /// <summary>
        /// XML から EquipmentOwner データを読み出す
        /// </summary>
        /// <returns>読み出した EquipmentOwner データ</returns>
        internal IEnumerable<EquipmentOwner> GetRecords()
        {
            foreach (var equipment in _WaresXml.Root.XPathSelectElements("ware[@transport='equipment']"))
            {
                var equipmentID = equipment.Attribute("id")?.Value;
                if (string.IsNullOrEmpty(equipmentID)) continue;

                var owners = equipment.XPathSelectElements("owner")
                    .Select(owner => owner.Attribute("faction")?.Value)
                    .Where(factionID => !string.IsNullOrEmpty(factionID))
                    .Distinct();
                foreach (var factionID in owners)
                {
                    if (factionID == null) continue;
                    yield return new EquipmentOwner(equipmentID, factionID);
                }
            }
        }
    }
}
