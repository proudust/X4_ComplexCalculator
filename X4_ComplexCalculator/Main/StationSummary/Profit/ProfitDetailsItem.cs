﻿using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.StationSummary.Profit
{
    /// <summary>
    /// 利益詳細表示用ListViewのアイテム
    /// </summary>
    class ProfitDetailsItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 単価
        /// </summary>
        private long _UnitPrice;
        #endregion

        #region プロパティ
        public string WareID { get; }

        /// <summary>
        /// ウェア名
        /// </summary>
        public string WareName { get; }


        /// <summary>
        /// ウェア数量
        /// </summary>
        public long Count { get; }


        /// <summary>
        /// 単価
        /// </summary>
        public long UnitPrice
        {
            get
            {
                return _UnitPrice;
            }
            set
            {
                if (value != _UnitPrice)
                {
                    _UnitPrice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }
        }

        /// <summary>
        /// 価格
        /// </summary>
        public long TotalPrice => UnitPrice * Count;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="wareName">ウェア名</param>
        /// <param name="count">ウェア数量</param>
        /// <param name="unitPrice">単価</param>
        public ProfitDetailsItem(string wareID, string wareName, long count, long unitPrice)
        {
            WareID = wareID;
            WareName = wareName;
            Count = count;
            _UnitPrice = unitPrice;
        }
    }
}
