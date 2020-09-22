using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Entity
{
    /// <summary>
    /// モジュールの装備兵装のコレクション
    /// </summary>
    public class ModuleEquipmentCollection
    {
        #region メンバ
        /// <summary>
        /// サイズごとの装備中の兵装リストの辞書
        /// </summary>
        private readonly Dictionary<X4Size, List<Equipment>> _Equipments = new Dictionary<X4Size, List<Equipment>>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備可能なサイズ一覧
        /// </summary>
        public IEnumerable<X4Size> Sizes => _Equipments.Keys;


        /// <summary>
        /// 何らかの兵装を装備可能かどうか
        /// </summary>
        public bool CanEquipped => 0 < _Equipments.Count;


        /// <summary>
        /// 装備中の兵装の列挙
        /// </summary>
        public IEnumerable<Equipment> AllEquipments => _Equipments.Values.SelectMany(x => x);


        /// <summary>
        /// 装備中の兵装の数
        /// </summary>
        public int AllEquipmentsCount => CanEquipped ? _Equipments.Sum(x => x.Value.Count) : 0;
        #endregion


        /// <summary>
        /// サイズごとの装備可能数からモジュールの装備兵装のコレクションを生成する
        /// </summary>
        /// <param name="capacity">装備可能な装備の数</param>
        public ModuleEquipmentCollection(IReadOnlyDictionary<X4Size, int> capacity)
            => _Equipments = capacity.ToDictionary(p => p.Key, p => new List<Equipment>(p.Value));


        /// <summary>
        /// 装備中の兵装の内、指定のサイズのみの列挙を返す
        /// </summary>
        /// <param name="size">列挙するサイズ</param>
        /// <returns>指定のサイズの兵装の列挙</returns>
        public IEnumerable<Equipment> GetEquipment(X4Size size) => _Equipments[size];


        /// <summary>
        /// 指定のサイズの装備可能数を返す
        /// </summary>
        /// <param name="size">列挙するサイズ</param>
        /// <returns>指定のサイズの兵装の列挙</returns>
        public int GetCapacity(X4Size size) => _Equipments[size].Capacity;


        /// <summary>
        /// 装備一覧をリセット
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <param name="equipments">装備一覧</param>
        public void ResetEquipment(X4Size size, ICollection<Equipment> equipments)
        {
            if (_Equipments[size].Capacity < equipments.Count)
            {
                throw new IndexOutOfRangeException("これ以上装備できません。");
            }

            _Equipments[size].Clear();
            _Equipments[size].AddRange(equipments);
        }


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="equipment">追加対象</param>
        public void AddEquipment(Equipment equipment)
        {
            if (_Equipments[equipment.Size].Count < _Equipments[equipment.Size].Capacity)
            {
                _Equipments[equipment.Size].Add(equipment);
            }
        }


        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is ModuleEquipmentCollection tgt && _Equipments.Equals(tgt._Equipments);


        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var equipment in _Equipments.SelectMany(x => x.Value))
            {
                hash.Add(equipment);
            }
            return hash.ToHashCode();
        }
    }
}
