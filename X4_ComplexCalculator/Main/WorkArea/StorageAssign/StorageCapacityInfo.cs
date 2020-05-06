﻿using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.StorageAssign
{
    /// <summary>
    /// 保管庫容量情報
    /// </summary>
    class StorageCapacityInfo : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 保管庫総容量
        /// </summary>
        private long _TotalCapacity;


        /// <summary>
        /// 保管庫使用容量
        /// </summary>
        private long _UsedCapacity;
        #endregion


        /// <summary>
        /// 保管庫総容量
        /// </summary>
        public long TotalCapacity
        {
            get => _TotalCapacity;
            set
            {
                if (SetProperty(ref _TotalCapacity, value))
                {
                    OnPropertyChanged(nameof(FreeCapacity));
                }
            }
        }

        /// <summary>
        /// 保管庫使用容量
        /// </summary>
        public long UsedCapacity
        {
            get => _UsedCapacity;
            set
            {
                if (SetProperty(ref _UsedCapacity, value))
                {
                    OnPropertyChanged(nameof(FreeCapacity));
                }
            }
        }


        /// <summary>
        /// 保管庫空き容量
        /// </summary>
        public long FreeCapacity => TotalCapacity - _UsedCapacity;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="totalCapacity">保管庫総容量</param>
        /// <param name="usedCapacity">保管庫使用容量</param>
        public StorageCapacityInfo(long totalCapacity = 0, long usedCapacity = 0)
        {
            TotalCapacity = totalCapacity;
            UsedCapacity = usedCapacity;
        }
    }
}
