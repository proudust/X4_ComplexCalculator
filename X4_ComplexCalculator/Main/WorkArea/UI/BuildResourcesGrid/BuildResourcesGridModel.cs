﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid
{
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用Model
    /// </summary>
    class BuildResourcesGridModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        private readonly ObservablePropertyChangedCollection<ModulesGridItem> _Modules;

        /// <summary>
        /// 単価保存用
        /// </summary>
        private readonly Dictionary<string, long> _UnitPriceBakDict = new Dictionary<string, long>();

        /// <summary>
        /// 建造リソース計算用
        /// </summary>
        private readonly BuildResourceCalclator _Calclator = BuildResourceCalclator.Instance;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造に必要なリソース
        /// </summary>
        public ObservablePropertyChangedCollection<BuildResourcesGridItem> Resources { get; private set; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public BuildResourcesGridModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            Resources = new ObservablePropertyChangedCollection<BuildResourcesGridItem>();
            _Modules = modules;
            _Modules.CollectionChangedAsync += OnModulesCollectionChanged;
            _Modules.CollectionPropertyChangedAsync += OnModulesPropertyChanged;
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Resources.Clear();
            _Modules.CollectionChangedAsync -= OnModulesCollectionChanged;
            _Modules.CollectionPropertyChangedAsync -= OnModulesPropertyChanged;
        }

        /// <summary>
        /// モジュールのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnModulesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is ModulesGridItem module))
            {
                await Task.CompletedTask;
                return;
            }

            switch (e.PropertyName)
            {
                // モジュール数変更の場合
                case nameof(ModulesGridItem.ModuleCount):
                    {
                        if (e is PropertyChangedExtendedEventArgs<long> ev)
                        {
                            OnModuleCountChanged(module, ev.OldValue);
                        }
                    }

                    break;

                // 装備変更の場合
                case nameof(ModulesGridItem.ModuleEquipment):
                    {
                        if (e is PropertyChangedExtendedEventArgs<IEnumerable<string>> ev)
                        {
                            OnModuleEquipmentChanged(module, ev.OldValue);
                        }
                    }

                    break;

                // 建造方式変更の場合
                case nameof(ModulesGridItem.SelectedMethod):
                    {
                        if (e is PropertyChangedExtendedEventArgs<ModuleProduction> ev)
                        {
                            OnModuleSelectedMethodChanged(module, ev.OldValue.Method);
                        }
                    }
                    break;

                default:
                    break;
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュール一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnModulesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                OnModulesAdded(e.NewItems.Cast<ModulesGridItem>());
            }

            if (e.OldItems != null)
            {
                OnModulesRemoved(e.OldItems.Cast<ModulesGridItem>());
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // 単価保存
                foreach (var resource in Resources)
                {
                    _UnitPriceBakDict.Add(resource.Ware.WareID, resource.UnitPrice);
                }

                Resources.Clear();
                OnModulesAdded(_Modules);
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュール数変更時に建造に必要なリソースを更新
        /// </summary>
        /// <param name="module">変更対象モジュール</param>
        /// <param name="prevModuleCount">モジュール数前回値</param>
        private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
        {
            (string ModuleID, string Method, long ModuleCount)[] modules =
            {
                (module.Module.ModuleID, module.SelectedMethod.Method, 1)
            };

            // モジュールの建造に必要なリソースを集計
            // モジュールの装備一覧(装備IDごとに集計)
            var equipments = module.ModuleEquipment.GetAllEquipment().Select(x => (ID: x.EquipmentID, Count: 1))
                                   .GroupBy(x => x.ID)
                                   .Select(x => (ID: x.Key, Method: "default", Count: x.LongCount()));

            Dictionary<string, long> resources = _Calclator.CalcResource(modules.Concat(equipments));

            foreach (var kvp in resources)
            {
                var itm = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (itm != null)
                {
                    itm.Amount += kvp.Value * (module.ModuleCount - prevModuleCount);
                }
            }
        }

        /// <summary>
        /// モジュールの建造方式変更時に必要なリソースを更新
        /// </summary>
        /// <param name="module"></param>
        /// <param name="buildMethod"></param>
        private void OnModuleSelectedMethodChanged(ModulesGridItem module, string buildMethod)
        {
            (string ModuleID, string Method, long ModuleCount)[] modules =
            {
                (module.Module.ModuleID, buildMethod, -1),                  // 変更前のため-1
                (module.Module.ModuleID, module.SelectedMethod.Method, 1)   // 変更後のため+1
            };

            Dictionary<string, long> resources = _Calclator.CalcResource(modules);

            var addTarget = new List<BuildResourcesGridItem>();
            foreach (var kvp in resources)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアが一覧にある場合
                    item.Amount += kvp.Value * module.ModuleCount;
                }
                else
                {
                    // ウェアが一覧にない場合
                    addTarget.Add(new BuildResourcesGridItem(kvp.Key, kvp.Value * module.ModuleCount));
                }
            }

            Resources.AddRange(addTarget);
            Resources.RemoveAll(x => x.Amount == 0);
        }



        /// <summary>
        /// モジュールの装備変更時に建造に必要なリソースを更新
        /// </summary>
        /// <param name="module">変更対象モジュール</param>
        /// <param name="prevEquipments">前回装備</param>
        private void OnModuleEquipmentChanged(ModulesGridItem module, IEnumerable<string> prevEquipments)
        {
            // 新しい装備一覧
            var newEquipments = module.ModuleEquipment.GetAllEquipment().GroupBy(x => x.EquipmentID).Select(x => (x.Key, "default", (long)x.Count()));

            // 古い装備一覧
            var oldEquipments = prevEquipments.GroupBy(x => x).Select(x => (x.Key, "default", -(long)x.Count()));

            // リソース集計
            Dictionary<string, long> resources = _Calclator.CalcResource(newEquipments.Concat(oldEquipments));

            var addTarget = new List<BuildResourcesGridItem>();
            foreach (var kvp in resources)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアが一覧にある場合
                    item.Amount += (kvp.Value * module.ModuleCount);
                }
                else
                {
                    // ウェアが一覧にない場合
                    addTarget.Add(new BuildResourcesGridItem(kvp.Key, kvp.Value * module.ModuleCount));
                }
            }

            Resources.AddRange(addTarget);
            Resources.RemoveAll(x => x.Amount == 0);
        }


        /// <summary>
        /// モジュールが追加された場合
        /// </summary>
        /// <param name="modules">追加されたモジュール</param>
        private void OnModulesAdded(IEnumerable<ModulesGridItem> modules)
        {
            Dictionary<string, long> resourcesDict = AggregateModules(modules);

            var addTarget = new List<BuildResourcesGridItem>();
            foreach (var kvp in resourcesDict)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアが一覧にある場合
                    item.Amount += kvp.Value;
                }
                else
                {
                    // ウェアが一覧にない場合
                    item = new BuildResourcesGridItem(kvp.Key, kvp.Value);
                    if (_UnitPriceBakDict.ContainsKey(item.Ware.WareID))
                    {
                        item.UnitPrice = _UnitPriceBakDict[item.Ware.WareID];
                    }
                    addTarget.Add(item);
                }
            }

            // マージ処理以外で反応しないようにするためクリアする
            _UnitPriceBakDict.Clear();
            Resources.AddRange(addTarget);
        }


        /// <summary>
        /// モジュールが削除された場合
        /// </summary>
        /// <param name="modules">削除されたモジュール</param>
        private void OnModulesRemoved(IEnumerable<ModulesGridItem> modules)
        {
            Dictionary<string, long> resourcesDict = AggregateModules(modules);

            foreach (var kvp in resourcesDict)
            {
                var itm = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (itm != null)
                {
                    itm.Amount -= kvp.Value;
                }
            }

            Resources.RemoveAll(x => x.Amount == 0);
        }


        /// <summary>
        /// 建造に必要な情報を集計
        /// </summary>
        /// <param name="modules">集計対象モジュール</param>
        /// <returns>集計結果</returns>
        private Dictionary<string, long> AggregateModules(IEnumerable<ModulesGridItem> modules)
        {
            // モジュール一覧
            var moduleList = modules.Select(x => (x.Module.ModuleID, x.SelectedMethod.Method, x.ModuleCount));

            // モジュールの装備一覧(装備IDごとに集計)
            var equipments = modules.Where(x => x.ModuleEquipment.CanEquipped)
                                    .Select(x => x.ModuleEquipment.GetAllEquipment().Select(y => (ID: y.EquipmentID, Count: x.ModuleCount)))
                                    .Where(x => x.Any())
                                    .GroupBy(x => x.First().ID)
                                    .Select(x => (ID: x.Key, Method: "default", Count: x.Sum(y => y.Sum(z => z.Count))));

            return _Calclator.CalcResource(moduleList.Concat(equipments));
        }
    }
}