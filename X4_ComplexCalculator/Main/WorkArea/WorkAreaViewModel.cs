﻿using Prism.Commands;
using System;
using System.IO;
using System.Windows.Input;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.StationSummary;
using X4_ComplexCalculator.Main.WorkArea.StoragesGrid;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace X4_ComplexCalculator.Main.WorkArea
{
    /// <summary>
    /// 作業エリア用ViewModel
    /// </summary>
    class WorkAreaViewModel : INotifyPropertyChangedBace, IDisposable
    {
        #region メンバ
        /// <summary>
        /// モデル
        /// </summary>
        private readonly WorkAreaModel _Model;


        /// <summary>
        /// レイアウトID
        /// </summary>
        private long _LayoutID;


        /// <summary>
        /// 現在のドッキングマネージャー
        /// </summary>
        private DockingManager _CurrentDockingManager;


        /// <summary>
        /// レイアウト保持用
        /// </summary>
        private byte[] _Layout;
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ModulesGridViewModel Modules { get; }


        /// <summary>
        /// 製品一覧
        /// </summary>
        public ProductsGridViewModel Products { get; }

        
        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        public ResourcesGridViewModel Resources { get; }

        
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public StoragesGridViewModel Storages { get; }


        /// <summary>
        /// 概要
        /// </summary>
        public StationSummaryViewModel Summary { get; }


        /// <summary>
        /// タブのタイトル文字列
        /// </summary>
        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_Model.SaveFilePath))
                {
                    return "no title*";
                }

                var ret = Path.GetFileNameWithoutExtension(_Model.SaveFilePath);

                return (HasChanged) ? $"{ret}*" : ret;
            }
        }

        /// <summary>
        /// 現在のドッキングマネージャー
        /// </summary>
        public DockingManager CurrentDockingManager
        {
            set
            {
                if (SetProperty(ref _CurrentDockingManager, value))
                {
                    // レイアウトIDが指定されていればレイアウト設定
                    if (0 <= _LayoutID)
                    {
                        SetLayout(_LayoutID);
                        // 1回ロードしたので次回以降ロードしないようにする
                        _LayoutID = -1;
                        return;
                    }

                    // 前回レイアウトがあれば、レイアウト復元
                    if (_Layout != null)
                    {
                        var serializer = new XmlLayoutSerializer(_CurrentDockingManager);

                        using var ms = new MemoryStream(_Layout, false);
                        serializer.Deserialize(ms);
                    }
                }
            }
            private get => _CurrentDockingManager;
        }


        /// <summary>
        /// アンロード時
        /// </summary>
        public ICommand OnUnloadedCommand { get; }


        /// <summary>
        /// モジュールの内容に変更があったか
        /// </summary>
        public bool HasChanged => _Model.HasChanged;
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="layoutID">レイアウトID</param>
        /// <remarks>
        /// レイアウトIDが負の場合、レイアウトは指定されていない事にする
        /// </remarks>
        public WorkAreaViewModel(long layoutID = -1)
        {
            _LayoutID = layoutID;

            var moduleModel     = new ModulesGridModel();
            var productsModel   = new ProductsGridModel(moduleModel.Modules);
            var resourcesModel  = new ResourcesGridModel(moduleModel.Modules);

            Summary     = new StationSummaryViewModel(moduleModel.Modules, productsModel.Products, resourcesModel.Resources);
            Modules     = new ModulesGridViewModel(moduleModel);
            Products    = new ProductsGridViewModel(productsModel);
            Resources   = new ResourcesGridViewModel(resourcesModel);
            Storages    = new StoragesGridViewModel(moduleModel.Modules);

            _Model              = new WorkAreaModel(moduleModel, productsModel, resourcesModel);
            OnUnloadedCommand   = new DelegateCommand(OnUnloaded);

            _Model.PropertyChanged += Model_PropertyChanged;
        }


        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            _Model.Save();
        }


        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        public void SaveAs()
        {
            _Model.SaveAs();
        }


        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="path">ファイルパス</param>
        public void LoadFile(string path)
        {
            _Model.Load(path);
        }


        /// <summary>
        /// レイアウト保存
        /// </summary>
        /// <param name="layoutName">レイアウト名</param>
        /// <returns>レイアウトID</returns>
        public long SaveLayout(string layoutName)
        {
            var id = 0L;

            var query = @$"
SELECT
    ifnull(MIN( LayoutID + 1 ), 0) AS LayoutID
FROM
    WorkAreaLayouts
WHERE
    ( LayoutID + 1 ) NOT IN ( SELECT LayoutID FROM WorkAreaLayouts)";

            DBConnection.CommonDB.ExecQuery(query, (dr, args) =>
            {
                id = (long)dr["LayoutID"];
            });

            
            var param = new SQLiteCommandParameters(3);
            param.Add("layoutID",   System.Data.DbType.Int32,   id);
            param.Add("layoutName", System.Data.DbType.String,  layoutName);
            param.Add("layout",     System.Data.DbType.Binary, GetCurrentLayout());

            DBConnection.CommonDB.ExecQuery("INSERT INTO WorkAreaLayouts(LayoutID, LayoutName, Layout) VALUES(:layoutID, :layoutName, :layout)", param);

            return id;
        }

        /// <summary>
        /// レイアウトを上書き保存
        /// </summary>
        /// <param name="layoutID">レイアウトID</param>
        public void OverwriteSaveLayout(long layoutID)
        {
            var param = new SQLiteCommandParameters(2);
            param.Add("layout", System.Data.DbType.Binary, GetCurrentLayout());
            param.Add("LayoutID", System.Data.DbType.Int32, layoutID);
            DBConnection.CommonDB.ExecQuery($"UPDATE WorkAreaLayouts SET Layout = :layout WHERE LayoutID = :layoutID", param);
        }


        /// <summary>
        /// レイアウトを設定
        /// </summary>
        /// <param name="layoutID"></param>
        public void SetLayout(long layoutID)
        {
            DBConnection.CommonDB.ExecQuery($"SELECT Layout FROM WorkAreaLayouts WHERE LayoutID = {layoutID}", (dr, args) =>
            {
                _Layout = (byte[])dr["Layout"];
            });

            if (_Layout != null)
            {
                var serializer = new XmlLayoutSerializer(_CurrentDockingManager);

                using var ms = new MemoryStream(_Layout, false);
                serializer.Deserialize(ms);
            }
        }


        /// <summary>
        /// 現在のレイアウトを取得
        /// </summary>
        /// <returns></returns>
        private byte[] GetCurrentLayout()
        {
            // レイアウト保存
            var serializer = new XmlLayoutSerializer(_CurrentDockingManager);
            using var ms = new MemoryStream();
            serializer.Serialize(ms);
            ms.Position = 0;

            return ms.ToArray();
        }

        /// <summary>
        /// アンロード時
        /// </summary>
        private void OnUnloaded()
        {
            _Layout = GetCurrentLayout();
        }


        /// <summary>
        /// Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_Model.HasChanged):
                    OnPropertyChanged(nameof(HasChanged));
                    OnPropertyChanged(nameof(Title));
                    break;

                case nameof(_Model.SaveFilePath):
                    OnPropertyChanged(nameof(Title));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.PropertyChanged -= Model_PropertyChanged;
            _Model.Dispose();
            Summary.Dispose();
            Modules.Dispose();
            Products.Dispose();
            Resources.Dispose();
            Storages.Dispose();
        }
    }
}
