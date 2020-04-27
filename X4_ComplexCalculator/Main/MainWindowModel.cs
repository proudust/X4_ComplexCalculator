﻿using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のModel
    /// </summary>
    class MainWindowModel
    {
        #region プロパティ
        /// <summary>
        /// ワークエリア一覧
        /// </summary>
        public ObservableCollection<WorkAreaViewModel> Documents = new ObservableCollection<WorkAreaViewModel>();


        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public WorkAreaViewModel ActiveContent { set; get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowModel()
        {

        }

        /// <summary>
        /// 上書き保存
        /// </summary>
        public void Save()
        {
            ActiveContent?.Save();
        }

        /// <summary>
        /// 名前を付けて保存
        /// </summary>
        public void SaveAs()
        {
            ActiveContent?.SaveAs();
        }

        /// <summary>
        /// 開く
        /// </summary>
        public void Open()
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = "X4 Station calclator data (*.x4)|*.x4|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    var vm = new WorkAreaViewModel();
                    vm.LoadFile(dlg.FileName);
                    Documents.Add(vm);
                }
                catch (Exception e)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show($"ファイルの読み込みに失敗しました。\r\n\r\n■理由：\r\n{e.Message}", "読み込み失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// 新規作成
        /// </summary>
        public void CreateNew()
        {
            var vm = new WorkAreaViewModel();
            Documents.Add(vm);
            ActiveContent = vm;
        }


        /// <summary>
        /// DB更新
        /// </summary>
        public void UpdateDB()
        {
            var result = MessageBox.Show("DB更新画面を表示しますか？\r\n※ 画面が起動するまでしばらくお待ち下さい。", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (!DBConnection.UpdateDB())
                {
                    MessageBox.Show("DBの更新に失敗しました。\r\nDBファイルにアクセス可能か確認後、再度実行してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        /// <returns>キャンセルされたか</returns>
        public bool WindowClosing()
        {
            var canceled = false;

            // 未保存の内容が存在するか？
            if (Documents.Where(x => x.HasChanged).Any())
            {
                var result = MessageBox.Show("未保存の項目があります。保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    // 保存する場合
                    case MessageBoxResult.Yes:
                        foreach (var doc in Documents)
                        {
                            doc.Save();
                        }
                        break;

                    // 保存せずに閉じる場合
                    case MessageBoxResult.No:
                        break;

                    // キャンセルする場合
                    case MessageBoxResult.Cancel:
                        canceled = true;
                        break;
                }
            }

            // 閉じる場合、リソースを開放
            if (!canceled)
            {
                foreach (var doc in Documents)
                {
                    doc.Dispose();
                }
            }

            return canceled;
        }


        /// <summary>
        /// 作業エリアが閉じられる時
        /// </summary>
        /// <param name="vm">閉じようとしている作業エリア</param>
        /// <returns></returns>
        public bool DocumentClosing(WorkAreaViewModel vm)
        {
            var canceled = false;

            // 変更があったか？
            if (vm.HasChanged)
            {
                var result = MessageBox.Show("変更内容を保存しますか？", "確認", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    // 保存する場合
                    case MessageBoxResult.Yes:
                        vm.Save();
                        break;

                    // 保存せずに閉じる場合
                    case MessageBoxResult.No:
                        break;

                    // キャンセルする場合
                    case MessageBoxResult.Cancel:
                        canceled = true;
                        break;
                }
            }

            // 閉じる場合、リソースを開放
            if (!canceled)
            {
                vm.Dispose();
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => Documents.Remove(vm)), DispatcherPriority.Background);

                // 最後のタブを閉じた時にAvalonDockのActiveContentが更新されないためここでnullにする
                // → nullにしないと閉じたはずのタブを保存できてしまう
                if (Documents.Count == 1)
                {
                    ActiveContent = null;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            return canceled;
        }
    }
}
