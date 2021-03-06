using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment
{
    /// <summary>
    /// 装備編集画面のModel
    /// </summary>
    class EditEquipmentModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 編集対象モジュール
        /// </summary>
        private readonly Module _Module;


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        private PresetComboboxItem? _SelectedPreset;
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備サイズ一覧
        /// </summary>
        public ObservableRangeCollection<X4Size> EquipmentSizes { get; } = new ObservableRangeCollection<X4Size>();


        /// <summary>
        /// 種族一覧
        /// </summary>
        public ObservablePropertyChangedCollection<FactionsListItem> Factions { get; } = new ObservablePropertyChangedCollection<FactionsListItem>();


        /// <summary>
        /// プリセット名
        /// </summary>
        public ObservableRangeCollection<PresetComboboxItem> Presets { get; } = new ObservableRangeCollection<PresetComboboxItem>();


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public PresetComboboxItem? SelectedPreset
        {
            get
            {
                return _SelectedPreset;
            }
            set
            {
                _SelectedPreset = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        public EditEquipmentModel(Module module)
        {
            // 初期化
            _Module = module;
            InitEquipmentSizes(module.ModuleID);
            UpdateFactions();
            InitPreset(module.ModuleID);
        }


        /// <summary>
        /// 装備サイズコンボボックスの内容を初期化
        /// </summary>
        /// <param name="moduleID"></param>
        private void InitEquipmentSizes(string moduleID)
        {
            static void AddItem(SQLiteDataReader dr, object[] args)
            {
                ((ICollection<X4Size>)args[0]).Add(X4Size.Get((string)dr["SizeID"]));
            }

            var sizes = new List<X4Size>();
            X4Database.Instance.ExecQuery($"SELECT DISTINCT ModuleShield.SizeID FROM ModuleShield, ModuleTurret WHERE ModuleShield.ModuleID = ModuleTurret.ModuleID AND ModuleShield.ModuleID = '{moduleID}'", AddItem, sizes);
            EquipmentSizes.AddRange(sizes);
        }


        /// <summary>
        /// 派閥一覧を更新
        /// </summary>
        private void UpdateFactions()
        {
            static void AddItem(SQLiteDataReader dr, object[] args)
            {
                bool chkState = 0 < SettingDatabase.Instance.ExecQuery($"SELECT ID FROM SelectModuleEquipmentCheckStateFactions WHERE ID = '{dr["FactionID"]}'", (_, __) => { });

                var faction = Faction.Get((string)dr["FactionID"]);
                if (faction != null) ((ICollection<FactionsListItem>)args[0]).Add(new FactionsListItem(faction, chkState));

            }

            var query = $@"
SELECT
	DISTINCT FactionID
FROM
	EquipmentOwner
WHERE
	EquipmentID IN (SELECT EquipmentID FROM Equipment WHERE (EquipmentTypeID IN ('turrets', 'shields')))";

            var items = new List<FactionsListItem>();
            X4Database.Instance.ExecQuery(query, AddItem, items);
            Factions.AddRange(items);
        }

        /// <summary>
        /// プリセットを初期化
        /// </summary>
        private void InitPreset(string moduleID)
        {
            SettingDatabase.Instance.ExecQuery($"SELECT DISTINCT PresetID, PresetName FROM ModulePresets WHERE ModuleID = '{moduleID}'", (SQLiteDataReader dr, object[] args) =>
            {
                Presets.Add(new PresetComboboxItem((long)dr["PresetID"], (string)dr["PresetName"]));
            });
        }


        /// <summary>
        /// チェック状態を保存
        /// </summary>
        public void SaveCheckState() => SettingDatabase.Instance.BeginTransaction(db =>
        {
            // 前回値クリア
            db.Execute("DELETE FROM SelectModuleEquipmentCheckStateFactions");

            // モジュール種別のチェック状態保存
            var checkedFactions = Factions.Where(x => x.IsChecked).Select(x => x.Faction);
            db.Execute("INSERT INTO SelectModuleEquipmentCheckStateFactions(ID) VALUES (:FactionID)", checkedFactions);
        });


        /// <summary>
        /// プリセット編集
        /// </summary>
        public void EditPreset()
        {
            if (SelectedPreset == null)
            {
                return;
            }

            // 新プリセット名
            var (onOK, newPresetName) = SelectStringDialog.ShowDialog("Lang:EditPresetName", "Lang:PresetName", SelectedPreset.Name, IsValidPresetName);
            if (onOK)
            {
                // 新プリセット名が設定された場合

                var param = new SQLiteCommandParameters(3);
                param.Add("presetName", System.Data.DbType.String, newPresetName);
                param.Add("moduleID", System.Data.DbType.String, _Module.ModuleID);
                param.Add("presetID", System.Data.DbType.Int64, SelectedPreset.ID);
                SettingDatabase.Instance.ExecQuery($"UPDATE ModulePresets Set PresetName = :presetName WHERE ModuleID = :moduleID AND presetID = :presetID", param);

                var newPreset = new PresetComboboxItem(SelectedPreset.ID, newPresetName);
                Presets.Replace(SelectedPreset, newPreset);
                SelectedPreset = newPreset;
            }
        }


        /// <summary>
        /// プリセット追加
        /// </summary>
        public void AddPreset()
        {
            var (onOK, presetName) = SelectStringDialog.ShowDialog("Lang:EditPresetName", "Lang:PresetName", "", IsValidPresetName);
            if (onOK)
            {
                var id = 0L;

                var query = @$"
SELECT
    ifnull(MIN( PresetID + 1 ), 0) AS PresetID
FROM
    ModulePresets
WHERE
	ModuleID = '{_Module.ModuleID}' AND
    ( PresetID + 1 ) NOT IN ( SELECT PresetID FROM ModulePresets WHERE ModuleID = '{_Module.ModuleID}')";

                SettingDatabase.Instance.ExecQuery(query, (SQLiteDataReader dr, object[] args) =>
                {
                    id = (long)dr["PresetID"];
                });

                var item = new PresetComboboxItem(id, presetName);

                SettingDatabase.Instance.BeginTransaction();
                SettingDatabase.Instance.ExecQuery($"INSERT INTO ModulePresets(ModuleID, PresetID, PresetName) VALUES('{_Module.ModuleID}', {item.ID}, '{item.Name}')");
                Presets.Add(item);
                SettingDatabase.Instance.Commit();

                SelectedPreset = item;
            }
        }

        /// <summary>
        /// プリセットを削除
        /// </summary>
        public void RemovePreset()
        {
            if (SelectedPreset == null)
            {
                return;
            }

            var result = LocalizedMessageBox.Show("Lang:DeletePresetConfirmMessage", "Lang:Error", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No, SelectedPreset.Name);
            if (result == MessageBoxResult.Yes)
            {
                SettingDatabase.Instance.BeginTransaction();
                SettingDatabase.Instance.ExecQuery($"DELETE FROM ModulePresets WHERE ModuleID = '{_Module.ModuleID}' AND PresetID = {SelectedPreset.ID}");
                Presets.Remove(SelectedPreset);
                SettingDatabase.Instance.Commit();

                SelectedPreset = Presets.FirstOrDefault();
            }
        }

        /// <summary>
        /// プリセット名が有効か判定する
        /// </summary>
        /// <param name="presetName">判定対象プリセット名</param>
        /// <returns>プリセット名が有効か</returns>
        static private bool IsValidPresetName(string presetName)
        {
            var ret = true;

            if (string.IsNullOrWhiteSpace(presetName))
            {
                LocalizedMessageBox.Show("Lang:InvalidPresetNameMessage", "Lang:Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                ret = false;
            }

            return ret;
        }
    }
}
