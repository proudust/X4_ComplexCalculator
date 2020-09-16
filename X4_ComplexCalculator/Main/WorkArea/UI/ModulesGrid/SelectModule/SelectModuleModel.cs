using System;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule
{
    class SelectModuleModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール追加先
        /// </summary>
        private ObservableRangeCollection<ModulesGridItem> ItemCollection;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュール種別
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesListItem> ModuleTypes { get; private set; } = new ObservablePropertyChangedCollection<ModulesListItem>();


        /// <summary>
        /// モジュール所有派閥
        /// </summary>
        public ObservablePropertyChangedCollection<FactionsListItem> ModuleOwners { get; private set; } = new ObservablePropertyChangedCollection<FactionsListItem>();


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesListItem> Modules { get; private set; } = new ObservablePropertyChangedCollection<ModulesListItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="itemCollection">選択結果格納先</param>
        public SelectModuleModel(ObservableRangeCollection<ModulesGridItem> itemCollection)
        {
            ItemCollection = itemCollection;

            ModuleOwners.CollectionPropertyChanged += UpdateModules;
            ModuleTypes.CollectionPropertyChanged += UpdateModules;

            InitModuleTypes();
            InitModuleOwners();
            UpdateModulesMain();
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            ModuleTypes.CollectionPropertyChanged -= UpdateModules;
            ModuleOwners.CollectionPropertyChanged -= UpdateModules;
        }


        /// <summary>
        /// モジュール種別一覧を初期化する
        /// </summary>
        private void InitModuleTypes()
        {
            const string sql1 = "SELECT ID FROM SelectModuleCheckStateModuleTypes";
            var checkedIds = SettingDatabase.Instance.Query<string>(sql1).ToHashSet();

            const string sql2 = @"
SELECT
    ModuleTypeID,
    Name
FROM
    ModuleType
WHERE
    ModuleTypeID IN (SELECT ModuleTypeID FROM Module)
ORDER BY Name";
            var items = X4Database.Instance
                .Query<(string ID, string Name)>(sql2, new { checkedIds })
                .Select(t => new ModulesListItem(t.ID, t.Name, checkedIds.Contains(t.ID)));

            ModuleTypes.AddRange(items);
        }


        /// <summary>
        /// 派閥一覧を初期化する
        /// </summary>
        private void InitModuleOwners()
        {
            const string sql1 = "SELECT ID FROM SelectModuleCheckStateModuleOwners";
            var checkedIds = SettingDatabase.Instance.Query<string>(sql1).ToHashSet();

            const string sql2 = @"
SELECT
    FactionID
FROM
    Faction
WHERE
    FactionID IN (SELECT FactionID FROM ModuleOwner)
ORDER BY Name ASC";
            var items = X4Database.Instance
                .Query<string>(sql2, new { checkedIds })
                .Select(Faction.Get)
                .Where(f => f != null)
                .Select(f => new FactionsListItem(f!, checkedIds.Contains(f!.FactionID)));

            ModuleOwners.AddRange(items);
        }


        /// <summary>
        /// モジュール一覧を更新する
        /// </summary>
        private void UpdateModules(object sender, EventArgs e) => UpdateModulesMain();


        /// <summary>
        /// モジュール一覧を更新する
        /// </summary>
        private void UpdateModulesMain()
        {
            const string query = @"
SELECT
    DISTINCT Module.ModuleID,
	Module.Name
FROM
    Module,
	ModuleOwner
WHERE
	Module.ModuleID = ModuleOwner.ModuleID AND
    Module.NoBlueprint = 0 AND
    Module.ModuleTypeID   IN :checkedTypeIds AND
	ModuleOwner.FactionID IN :checkedOwnerIds";
            var checkedTypeIds = ModuleTypes.Where(x => x.IsChecked).Select(x => x.ID);
            var checkedOwnerIds = ModuleOwners.Where(x => x.IsChecked).Select(x => x.Faction.FactionID);
            var param = new { checkedTypeIds, checkedOwnerIds };

            Modules.Clear();
            foreach (var (id, name) in X4Database.Instance.Query<(string, string)>(query, param))
            {
                Modules.Add(new ModulesListItem(id, name, false));
            }
        }


        /// <summary>
        /// 選択中のモジュール一覧をコレクションに追加する
        /// </summary>
        public void AddSelectedModuleToItemCollection()
        {
            // 選択されているアイテムを追加
            var items = Modules.Where(x => x.IsChecked)
                               .Select(x => Module.Get(x.ID))
                               .Where(x => x != null)
                               .Select(x => x!)
                               .Select(x => new ModulesGridItem(x) { EditStatus = EditStatus.Edited });

            ItemCollection.AddRange(items);
        }

        /// <summary>
        /// チェック状態を保存する
        /// </summary>
        public void SaveCheckState()
        {
            // 前回値クリア
            SettingDatabase.Instance.ExecQuery("DELETE FROM SelectModuleCheckStateModuleTypes", null);
            SettingDatabase.Instance.ExecQuery("DELETE FROM SelectModuleCheckStateModuleOwners", null);

            // トランザクション開始
            SettingDatabase.Instance.BeginTransaction();

            // モジュール種別のチェック状態保存
            foreach (var id in ModuleTypes.Where(x => x.IsChecked).Select(x => x.ID))
            {
                SettingDatabase.Instance.ExecQuery($"INSERT INTO SelectModuleCheckStateModuleTypes(ID) VALUES ('{id}')", null);
            }

            // 派閥一覧のチェック状態保存
            foreach (var id in ModuleOwners.Where(x => x.IsChecked).Select(x => x.Faction.FactionID))
            {
                SettingDatabase.Instance.ExecQuery($"INSERT INTO SelectModuleCheckStateModuleOwners(ID) VALUES ('{id}')", null);
            }

            // コミット
            SettingDatabase.Instance.Commit();
        }
    }
}
