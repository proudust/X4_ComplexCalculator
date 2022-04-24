using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace X4_DataExporterWPF.DataExportWindow;

/// <summary>
/// データ抽出処理用ViewModel
/// </summary>
sealed partial class DataExportViewModel : ObservableObject
{
    #region メンバ
    /// <summary>
    /// モデル
    /// </summary>
    private readonly DataExportModel _Model = new();

    /// <summary>
    /// 出力先ファイルパス
    /// </summary>
    private readonly string _OutFilePath;

    /// <summary>
    /// 入力元フォルダパス
    /// </summary>
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(CanExport))]
    [AlsoNotifyCanExecuteFor(nameof(ExportCommand))]
    private string _InDirPath;

    /// <summary>
    /// 現在の入力元フォルダパスから言語一覧を取得できなかった場合 true
    /// </summary>
    [ObservableProperty]
    private bool _IsUnableToGetLanguages = false;

    /// <summary>
    /// 言語一覧
    /// </summary>
    [ObservableProperty]
    private LangComboboxItem[] _Languages = Array.Empty<LangComboboxItem>();

    /// <summary>
    /// 選択された言語
    /// </summary>
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(CanExport))]
    [AlsoNotifyCanExecuteFor(nameof(ExportCommand))]
    private LangComboboxItem? _SelectedLanguage;

    /// <summary>
    /// 進捗最大
    /// </summary>
    [ObservableProperty]
    private int _MaxSteps = 1;

    /// <summary>
    /// 現在の進捗
    /// </summary>
    [ObservableProperty]
    private int _CurrentStep = 0;

    /// <summary>
    /// 進捗最大(小項目)
    /// </summary>
    [ObservableProperty]
    private int _MaxStepsSub = 1;

    /// <summary>
    /// 現在の進捗(小項目)
    /// </summary>
    [ObservableProperty]
    private int _CurrentStepSub = 0;

    /// <summary>
    /// ユーザが操作可能か
    /// </summary>
    [ObservableProperty]
    [AlsoNotifyChangeFor(nameof(CanExport))]
    [AlsoNotifyCanExecuteFor(nameof(SelectInDirCommand))]
    [AlsoNotifyCanExecuteFor(nameof(ExportCommand))]
    [AlsoNotifyCanExecuteFor(nameof(ClosingCommand))]
    private bool _CanOperation = false;
    #endregion


    #region プロパティ
    /// <summary>
    /// エクスポート実行可能か
    /// 操作可能かつ入力項目に不備がない場合に true にする
    /// </summary>
    public bool CanExport => CanOperation
            && !string.IsNullOrEmpty(InDirPath)
            && SelectedLanguage is not null;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public DataExportViewModel(string inDirPath, string outFilePath)
    {
        _OutFilePath = outFilePath;
        _InDirPath = inDirPath;

        OnInDirPathChanged(InDirPath);
    }


    partial void OnInDirPathChanged(string value)
    {
        try
        {
            CanOperation = false;
            IsUnableToGetLanguages = false;

            (IsUnableToGetLanguages, Languages) = _Model.GetLanguages(value);
        }
        finally
        {
            CanOperation = true;
        }
    }


    /// <summary>
    /// 入力元フォルダを選択
    /// </summary>
    [ICommand(CanExecute = nameof(CanOperation))]
    private void SelectInDir()
    {
        var dlg = new CommonOpenFileDialog();
        dlg.IsFolderPicker = true;
        dlg.AllowNonFileSystemItems = false;

        if (Directory.Exists(InDirPath))
        {
            dlg.InitialDirectory = InDirPath;
        }
        else
        {
            dlg.InitialDirectory = Path.GetDirectoryName(InDirPath);
        }

        if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
        {
            InDirPath = dlg.FileName;
        }
    }


    /// <summary>
    /// データ抽出実行
    /// </summary>
    [ICommand(CanExecute = nameof(CanExport))]
    private async Task Export()
    {
        try
        {
            CanOperation = false;

            // 言語が未選択なら何もしない
            if (SelectedLanguage is null)
            {
                return;
            }

            var owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);

            var progress = new Progress<(int currentStep, int maxSteps)>(s =>
            {
                CurrentStep = s.currentStep;
                MaxSteps = s.maxSteps;
            });

            var progressSub = new Progress<(int currentStep, int maxSteps)>(s =>
            {
                CurrentStepSub = s.currentStep;
                MaxStepsSub = s.maxSteps;
            });

            await Task.Run(() => _Model.Export(
                progress,
                progressSub,
                InDirPath,
                _OutFilePath,
                SelectedLanguage,
                owner
            ));
            CurrentStep = 0;
        }
        finally
        {
            CanOperation = true;
        }
    }


    /// <summary>
    /// ウィンドウを閉じる
    /// </summary>
    [ICommand(CanExecute = nameof(CanOperation))]
    private void Closing(CancelEventArgs e) => e.Cancel = !CanOperation;
}