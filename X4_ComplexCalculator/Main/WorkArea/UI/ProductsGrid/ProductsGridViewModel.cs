﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品一覧用DataGridViewのViewModel
    /// </summary>
    class ProductsGridViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 製品一覧用DataGridViewのModel
        /// </summary>
        readonly ProductsGridModel _Model;


        /// <summary>
        /// 製品価格割合
        /// </summary>
        private long _UnitPricePercent = 50;


        /// <summary>
        /// 不足ウェアを購入しない
        /// </summary>
        private bool _NoBuy;


        /// <summary>
        /// 余剰ウェアを売却しない
        /// </summary>
        private bool _NoSell;
        #endregion


        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public ICollectionView ProductsView { get; }


        /// <summary>
        /// 単価(百分率)
        /// </summary>
        public double UnitPricePercent
        {
            get => _UnitPricePercent;
            set
            {
                _UnitPricePercent = (long)value;

                foreach (var product in _Model.Products)
                {
                    product.SetUnitPricePercent(_UnitPricePercent);
                }

                RaisePropertyChanged();
            }
        }


        /// <summary>
        /// 選択されたアイテムの展開/折りたたみ状態を設定する
        /// </summary>
        public ICommand SetSelectedExpandedCommand { get; }


        /// <summary>
        /// モジュール自動追加
        /// </summary>
        public ICommand AutoAddModuleCommand { get; }


        /// <summary>
        /// 選択されたアイテムの不足ウェア購入オプションを設定
        /// </summary>
        public ICommand SetNoBuyToSelectedItemCommand { get; }


        /// <summary>
        /// 選択されたアイテムの余剰ウェア販売オプションを設定
        /// </summary>
        public ICommand SetNoSellToSelectedItemCommand { get; }


        /// <summary>
        /// 不足ウェアを購入しない
        /// </summary>
        public bool NoBuy
        {
            get => _NoBuy;
            set
            {
                foreach (var prod in _Model.Products)
                {
                    prod.NoBuy = value;
                }

                SetProperty(ref _NoBuy, value);
            }
        }


        /// <summary>
        /// 余剰ウェアを売却しない
        /// </summary>
        public bool NoSell
        {
            get => _NoSell;
            set
            {
                foreach (var prod in _Model.Products)
                {
                    prod.NoSell = value;
                }

                SetProperty(ref _NoSell, value);
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="productsGridModel">製品一覧用Model</param>
        public ProductsGridViewModel(ProductsGridModel productsGridModel)
        {
            _Model = productsGridModel;

            ProductsView = new CollectionViewSource { Source = _Model.Products }.View;

            // ソート方向設定
            ProductsView.SortDescriptions.Clear();
            ProductsView.SortDescriptions.Add(new SortDescription("Ware.WareGroup.Tier", ListSortDirection.Ascending));
            ProductsView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));

            SetSelectedExpandedCommand = new DelegateCommand<string>(SetSelectedExpanded);
            AutoAddModuleCommand = new DelegateCommand(_Model.AutoAddModule);
            SetNoBuyToSelectedItemCommand = new DelegateCommand<string>(SetNoBuyToSelectedItem);
            SetNoSellToSelectedItemCommand = new DelegateCommand<string>(SetNoSellToSelectedItem);
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose() => _Model.Dispose();


        /// <summary>
        /// 選択されたアイテムの展開/折りたたみ状態を設定
        /// </summary>
        /// <param name="param">"True"か"False"</param>
        private void SetSelectedExpanded(string param)
        {
            bool value = bool.Parse(param);
            foreach (var item in _Model.Products.Where(x => x.IsSelected))
            {
                item.IsExpanded = value;
            }
        }


        /// <summary>
        /// 選択されたアイテムの不足ウェア購入オプションを設定
        /// </summary>
        /// <param name="param">"True"か"False"</param>
        private void SetNoBuyToSelectedItem(string param)
        {
            bool value = bool.Parse(param);
            foreach (var item in _Model.Products.Where(x => x.IsSelected))
            {
                item.NoBuy = value;
            }
        }


        /// <summary>
        /// 選択されたアイテムの余剰ウェア販売オプションを設定
        /// </summary>
        /// <param name="param">"True"か"False"</param>
        private void SetNoSellToSelectedItem(string param)
        {
            bool value = bool.Parse(param);
            foreach (var item in _Model.Products.Where(x => x.IsSelected))
            {
                item.NoSell = value;
            }
        }
    }
}
