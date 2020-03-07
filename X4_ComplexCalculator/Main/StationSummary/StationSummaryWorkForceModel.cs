﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid;

namespace X4_ComplexCalculator.Main.StationSummary
{
    class StationSummaryWorkForceModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 必要な労働者数
        /// </summary>
        private long _NeedWorkforce = 0;

        /// <summary>
        /// 現在の労働者数
        /// </summary>
        private long _WorkForce = 0;
        #endregion

        #region プロパティ
        /// <summary>
        /// 労働力の詳細情報
        /// </summary>
        public SmartCollection<WorkForceDetailsItem> WorkForceDetails { get; private set; } = new SmartCollection<WorkForceDetailsItem>();

        /// <summary>
        /// 必要な労働者数
        /// </summary>
        public long NeedWorkforce
        {
            get
            {
                return _NeedWorkforce;
            }
            set
            {
                if (_NeedWorkforce != value)
                {
                    _NeedWorkforce = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// 現在の労働者数
        /// </summary>
        public long WorkForce
        {
            get
            {
                return _WorkForce;
            }
            set
            {
                if (_WorkForce != value)
                {
                    _WorkForce = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧のModel</param>
        public StationSummaryWorkForceModel(ModulesGridModel moduleGridModel)
        {
            moduleGridModel.OnModulesChanged += UpdateWorkForce;
        }


        /// <summary>
        /// 製品更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateWorkForce(object sender, NotifyCollectionChangedEventArgs e)
        {
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                UpdateWorkForceMain(sender, e);
            });
        }


        /// <summary>
        /// 労働力情報を更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateWorkForceMain(object sender, NotifyCollectionChangedEventArgs e)
        {
            var modules = ((IEnumerable<ModulesGridItem>)sender).Where(x => x.Module.ModuleType.ModuleTypeID == "production" || x.Module.ModuleType.ModuleTypeID == "habitation");

            var details = modules.GroupBy(x => x.Module.ModuleID)
                                 .Select(x => new WorkForceDetailsItem(x.First().Module, x.Sum(y => y.ModuleCount)))
                                 .ToArray();

            // 値を更新
            NeedWorkforce = details.Sum(x => x.MaxWorkers * x.ModuleCount);
            WorkForce = details.Sum(x => x.WorkersCapacity * x.ModuleCount);
            WorkForceDetails.Reset(details);
        }
    }
}
