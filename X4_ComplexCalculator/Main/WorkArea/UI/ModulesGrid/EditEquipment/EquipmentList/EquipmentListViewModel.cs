﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;
using Prism.Commands;
using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// 装備一覧用ViewModel
    /// </summary>
    class EquipmentListViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 装備一覧用Model
        /// </summary>
        readonly EquipmentListModelBase _Model;


        /// <summary>
        /// 装備検索文字列
        /// </summary>
        private string _SearchEquipmentName = "";


        /// <summary>
        /// 装備一覧表示用
        /// </summary>
        private readonly Dictionary<X4Size, ListCollectionView> _EquipmentsViews = new Dictionary<X4Size, ListCollectionView>();
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備一覧表示用
        /// </summary>
        public ListCollectionView? EquipmentsView => (_Model.SelectedSize != null) ? _EquipmentsViews[_Model.SelectedSize] : null;


        /// <summary>
        /// 装備中の装備
        /// </summary>
        public ObservableCollection<EquipmentListItem>? Equipped => (_Model.SelectedSize != null) ? _Model.Equipped[_Model.SelectedSize] : null;


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        public int MaxAmount => (_Model.SelectedSize != null) ? _Model.MaxAmount[_Model.SelectedSize] : 0;


        /// <summary>
        /// 現在装備中の個数
        /// </summary>
        public int EquippedCount => (_Model.SelectedSize != null) ? _Model.Equipped[_Model.SelectedSize].Count : 0;


        /// <summary>
        /// 装備の検索文字列
        /// </summary>
        public string SearchEquipmentName
        {
            get => _SearchEquipmentName;
            set
            {
                if (_SearchEquipmentName != value)
                {
                    _SearchEquipmentName = value;
                    RaisePropertyChanged();
                    if (EquipmentsView == null)
                    {
                        throw new InvalidOperationException();
                    }
                    EquipmentsView.Refresh();
                }
            }
        }


        /// <summary>
        /// 追加ボタンクリック時のコマンド
        /// </summary>
        public DelegateCommand AddButtonClickedCommand { get; }


        /// <summary>
        /// 削除ボタンクリック時のコマンド
        /// </summary>
        public DelegateCommand RemoveButtonClickedCommand { get; }


        /// <summary>
        /// 選択中の装備サイズ
        /// </summary>
        public X4Size SelectedSize
        {
            set
            {
                _Model.SelectedSize = value;
                RaisePropertyChanged(nameof(MaxAmount));
                RaisePropertyChanged(nameof(EquippedCount));
                RaisePropertyChanged(nameof(Equipped));
                AddButtonClickedCommand.RaiseCanExecuteChanged();
                RemoveButtonClickedCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(EquipmentsView));
            }
        }


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public PresetComboboxItem? SelectedPreset
        {
            set
            {
                _Model.SelectedPreset = value;
                RaisePropertyChanged(nameof(MaxAmount));
                RaisePropertyChanged(nameof(EquippedCount));
                RaisePropertyChanged(nameof(Equipped));
                AddButtonClickedCommand.RaiseCanExecuteChanged();
                RemoveButtonClickedCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(EquipmentsView));
                Unsaved = true;
            }
        }


        /// <summary>
        /// 未保存か
        /// </summary>
        public bool Unsaved { get; set; } = false;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="model"></param>
        public EquipmentListViewModel(EquipmentListModelBase model)
        {
            _Model = model;

            foreach (var pair in _Model.Equipments)
            {
                var item = (ListCollectionView)CollectionViewSource.GetDefaultView(pair.Value);
                item.Filter = Filter;
                _EquipmentsViews.Add(pair.Key, item);
            }

            AddButtonClickedCommand = new DelegateCommand(AddButtonClicked, () => EquippedCount < MaxAmount);
            RemoveButtonClickedCommand = new DelegateCommand(DeleteButtonClicked, () => 0 < EquippedCount);
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose() => _Model.Dispose();


        /// <summary>
        /// 装備を保存
        /// </summary>
        public void SaveEquipment()
        {
            _Model.SaveEquipment();
            Unsaved = false;
        }


        /// <summary>
        /// 追加ボタンクリック時
        /// </summary>
        void AddButtonClicked()
        {
            if (_Model.AddSelectedEquipments())
            {
                // 装備が追加された場合
                Unsaved = true;
                AddButtonClickedCommand.RaiseCanExecuteChanged();
                RemoveButtonClickedCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(EquippedCount));
            }
        }


        /// <summary>
        /// 削除ボタンクリック時
        /// </summary>
        void DeleteButtonClicked()
        {
            if (_Model.RemoveSelectedEquipments())
            {
                // 装備が削除された場合
                Unsaved = true;
                AddButtonClickedCommand.RaiseCanExecuteChanged();
                RemoveButtonClickedCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(EquippedCount));
            }
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filter(object obj)
        {
            return obj is EquipmentListItem src && (SearchEquipmentName == "" || 0 <= src.Equipment.Name.IndexOf(SearchEquipmentName, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// プリセット一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnPresetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _Model.OnPresetsCollectionChanged(sender, e);
            Unsaved = true;
        }


        /// <summary>
        /// プリセット保存
        /// </summary>
        public void SavePreset() => _Model.SavePreset();
    }
}
