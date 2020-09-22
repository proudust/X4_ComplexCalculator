using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Entity
{
    /// <summary>
    /// モジュールの装備管理用クラス
    /// </summary>
    public class ModuleEquipment
    {
        #region プロパティ
        /// <summary>
        /// タレット情報
        /// </summary>
        public ModuleEquipmentCollection Turret { get; }


        /// <summary>
        /// シールド情報
        /// </summary>
        public ModuleEquipmentCollection Shield { get; }


        /// <summary>
        /// 装備を持っているか
        /// </summary>
        public bool CanEquipped => Turret.CanEquipped || Shield.CanEquipped;


        /// <summary>
        /// 装備中の兵装の列挙
        /// </summary>
        public IEnumerable<Equipment> AllEquipments => Turret.AllEquipments.Concat(Shield.AllEquipments);
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュール</param>
        public ModuleEquipment(Module module)
        {
            Turret = new ModuleEquipmentCollection(module.TurretCapacity);
            Shield = new ModuleEquipmentCollection(module.ShieldCapacity);
        }


        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is ModuleEquipment equipment
            && Turret.Equals(equipment.Turret) && Shield.Equals(equipment.Shield);


        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Turret, Shield);
    }
}
