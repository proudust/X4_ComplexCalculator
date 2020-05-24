﻿using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Collection;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule
{
    class SelectModuleViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// モデル
        /// </summary>
        readonly SelectModuleModel _Model;


        /// <summary>
        /// 検索モジュール名
        /// </summary>
        private string _SearchModuleName = "";


        /// <summary>
        /// 置換モードか
        /// </summary>
        private readonly bool _IsReplaceMode;


        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        private bool _CloseWindow = false;
        #endregion


        #region プロパティ
        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        public bool CloseWindowProperty
        {
            get
            {
                return _CloseWindow;
            }
            set
            {
                _CloseWindow = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        /// ウィンドウが閉じられる時のコマンド
        /// </summary>
        public ICommand WindowClosingCommand { get; }


        /// <summary>
        /// 変更前のモジュール名
        /// </summary>
        public string PrevModuleName { get; }


        /// <summary>
        /// モジュール一覧表示用
        /// </summary>
        public ListCollectionView ModulesView { get; }


        /// <summary>
        /// 変更前モジュールを表示するか
        /// </summary>
        public Visibility PrevModuleVisiblity => _IsReplaceMode ? Visibility.Visible : Visibility.Collapsed;


        /// <summary>
        /// モジュール種別
        /// </summary>
        public ObservableCollection<ModulesListItem> ModuleTypes => _Model.ModuleTypes;


        /// <summary>
        /// モジュール所有派閥
        /// </summary>
        public ObservableCollection<ModulesListItem> ModuleOwners => _Model.ModuleOwners;


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservableCollection<ModulesListItem> Modules => _Model.Modules;


        /// <summary>
        /// モジュール名検索用
        /// </summary>
        public string SearchModuleName
        {
            private get { return _SearchModuleName; }
            set
            {
                if (_SearchModuleName != value)
                {
                    _SearchModuleName = value;
                    RaisePropertyChanged();
                    ModulesView.Refresh();
                }
            }
        }


        /// <summary>
        /// 選択ボタンクリック時
        /// </summary>
        public ICommand OKButtonClickedCommand { get; }


        /// <summary>
        /// 閉じるボタンクリック時
        /// </summary>
        public ICommand CloseButtonClickedCommand { get; }


        /// <summary>
        /// モジュール一覧ListBoxの選択モード
        /// </summary>
        public SelectionMode ModuleListSelectionMode => _IsReplaceMode ? SelectionMode.Single : SelectionMode.Extended;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">選択結果格納先</param>
        /// <param name="isReplaceMode">置換モードか(falseで複数選択許可)</param>
        public SelectModuleViewModel(ObservableRangeCollection<ModulesGridItem> modules, string prevModuleName = "")
        {
            _Model = new SelectModuleModel(modules);

            PrevModuleName = prevModuleName;
            _IsReplaceMode  = prevModuleName != "";

            ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(Modules);
            ModulesView.Filter = Filter;
            ModulesView.SortDescriptions.Clear();
            ModulesView.SortDescriptions.Add(new SortDescription(nameof(ModulesListItem.Name), ListSortDirection.Ascending));

            OKButtonClickedCommand    = new DelegateCommand(OKButtonClicked);
            CloseButtonClickedCommand = new DelegateCommand(CloseWindow);
            WindowClosingCommand      = new DelegateCommand<CancelEventArgs>(WindowClosing);
        }


        /// <summary>
        /// ウィンドウが閉じられる時のイベント
        /// </summary>
        /// <param name="e"></param>
        public void WindowClosing(CancelEventArgs _)
        {
            _Model.SaveCheckState();
            _Model.Dispose();
        }


        /// <summary>
        /// OKボタンクリック時
        /// </summary>
        private void OKButtonClicked()
        {
            _Model.AddSelectedModuleToItemCollection();

            // 置換モードならウィンドウを閉じる
            if (_IsReplaceMode)
            {
                CloseWindowProperty = true;
            }
        }


        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        private void CloseWindow()
        {
            CloseWindowProperty = true;
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filter(object obj)
        {
            // 表示するか
            var ret = false;

            if (obj is ModulesListItem src)
            {
                ret = SearchModuleName == "" || 0 <= src.Name.IndexOf(SearchModuleName, StringComparison.InvariantCultureIgnoreCase);

                // 非表示になる場合、選択解除(選択解除しないと非表示のモジュールが意図せずモジュール一覧に追加される)
                if (!ret)
                {
                    src.IsChecked = false;
                }
            }

            return ret;
        }
    }
}