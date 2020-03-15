﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EquipmentList
{
    class ShieldEquipmentListModel : EquipmentListModelBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        /// <param name="factions">種族一覧</param>
        public ShieldEquipmentListModel(Module module, MemberChangeDetectCollection<FactionsListItem> factions) : base(module, factions)
        {
            foreach (Size size in Module.Equipment.Shield.Sizes)
            {
                _Equipments.Add(size, new SmartCollection<Equipment>());
                _Equipped.Add(size, new SmartCollection<Equipment>());
                _MaxAmount.Add(size, Module.Equipment.Shield.MaxAmount[size]);

                // 前回値復元
                _Equipped[size].AddRange(Module.Equipment.Shield.GetEquipment(size).Take(_MaxAmount[size]));
            }
        }


        /// <summary>
        /// 装備一覧を更新
        /// </summary>
        protected override async Task UpdateEquipments(object sender, PropertyChangedEventArgs e)
        {
            if (SelectedSize == null)
            {
                return;
            }

            var items = new List<Equipment>();

            var selectedFactions = string.Join(", ", SelectedFactions.Select(x => $"'{x.Faction.FactionID}'"));

            var query = $@"
SELECT
	DISTINCT Equipment.EquipmentID
FROM
	Equipment,
	EquipmentOwner
WHERE
	EquipmentTypeID = 'shields' AND
	SizeID = '{SelectedSize.SizeID}' AND
	Equipment.EquipmentID = EquipmentOwner.EquipmentID AND
    EquipmentOwner.FactionID IN ({selectedFactions})";

            DBConnection.X4DB.ExecQuery(query, (SQLiteDataReader dr, object[] args) => { items.Add(new Equipment(dr["EquipmentID"].ToString())); });

            await Task.Run(() => Equipments[SelectedSize].Reset(items));
        }


        /// <summary>
        /// 装備を保存
        /// </summary>
        public override void SaveEquipment()
        {
            foreach (var size in _Equipments.Keys)
            {
                Module.Equipment.Shield.ResetEquipment(size, Equipped[size]);
            }
        }
    }
}
