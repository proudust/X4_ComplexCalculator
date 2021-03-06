﻿using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Prism.Commands;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Entity;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid
{
    /// <summary>
    /// ModuleクラスをDataGrid表示用クラス
    /// </summary>
    public class ModulesGridItem : BindableBaseEx, IEditable, ISelectable, IReorderble
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュール数最大値
        /// </summary>
        public const long MAX_MODULE_COUNT = 99999;
        #endregion


        #region メンバ
        /// <summary>
        /// モジュール数
        /// </summary>
        private long _ModuleCount = 1;


        /// <summary>
        /// 選択された建造方式
        /// </summary>
        private ModuleProduction _SelectedMethod;


        /// <summary>
        /// 選択されているか
        /// </summary>
        private bool _IsSelected;


        /// <summary>
        /// 編集状態
        /// </summary>
        private EditStatus _EditStatus = EditStatus.Unedited;


        /// <summary>
        /// 順番入れ替え対象か
        /// </summary>
        private bool _IsReorderTarget;
        #endregion


        #region プロパティ
        /// <summary>
        /// モジュール
        /// </summary>
        public Module Module { get; }


        /// <summary>
        /// 装備情報
        /// </summary>
        public ModuleEquipment ModuleEquipment { get; }


        /// <summary>
        /// モジュールの装備編集
        /// </summary>
        public ICommand EditEquipmentCommand { get; }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount
        {
            get => _ModuleCount;
            set
            {
                var setValue = (value < 0) ? 0L :
                               (MAX_MODULE_COUNT < value) ? MAX_MODULE_COUNT : value;

                if (SetPropertyEx(ref _ModuleCount, setValue))
                {
                    EditStatus = EditStatus.Edited;
                }
            }
        }

        /// <summary>
        /// 装備中のタレットの個数
        /// </summary>
        public int TurretsCount => ModuleEquipment.Turret.AllEquipmentsCount;


        /// <summary>
        /// タレットのツールチップ文字列
        /// </summary>
        public string TurretsToolTip => MakeEquipmentToolTipString(ModuleEquipment.Turret);


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount => ModuleEquipment.Shield.AllEquipmentsCount;


        /// <summary>
        /// シールドのツールチップ文字列
        /// </summary>
        public string ShieldsToolTip => MakeEquipmentToolTipString(ModuleEquipment.Shield);


        /// <summary>
        /// 編集ボタンを表示すべきか
        /// </summary>
        public Visibility EditEquipmentButtonVisiblity => (ModuleEquipment.CanEquipped) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 建造方式を表示すべきか
        /// </summary>
        public Visibility SelectedMethodVisiblity => (2 <= Module.ModuleProductions.Count) ? Visibility.Visible : Visibility.Hidden;


        /// <summary>
        /// 選択中の建造方式
        /// </summary>
        public ModuleProduction SelectedMethod
        {
            get => _SelectedMethod;
            set
            {
                if (SetPropertyEx(ref _SelectedMethod, value))
                {
                    EditStatus = EditStatus.Edited;
                }
            }
        }


        /// <summary>
        /// 編集状態
        /// </summary>
        public EditStatus EditStatus
        {
            get => _EditStatus;
            set => SetProperty(ref _EditStatus, value);
        }


        /// <summary>
        /// 順番入れ替え対象か
        /// </summary>
        public bool IsReorderTarget
        {
            get => _IsReorderTarget;
            set => SetProperty(ref _IsReorderTarget, value);
        }
        #endregion




        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュール</param>
        /// <param name="selectedMethod">選択中の建造方式</param>
        /// <param name="moduleCount">モジュール数</param>
        public ModulesGridItem(Module module, ModuleProduction? selectedMethod = null, long moduleCount = 1)
        {
            Module = module;
            ModuleCount = moduleCount;
            EditEquipmentCommand = new DelegateCommand(EditEquipment);
            ModuleEquipment = new ModuleEquipment(module);

            _SelectedMethod = selectedMethod ?? Module.ModuleProductions[0];
        }


        /// <summary>
        /// コンストラクタ(xmlより作成)
        /// </summary>
        /// <param name="element">モジュール情報が記載されたxml</param>
        public ModulesGridItem(XElement element)
        {
            Module = Module.Get(element.Attribute("id").Value) ?? throw new ArgumentException("Invalid XElement.", nameof(element));
            ModuleEquipment = new ModuleEquipment(Module);

            ModuleCount = long.Parse(element.Attribute("count").Value);
            SelectedMethod = Module.ModuleProductions.FirstOrDefault(x => x.Method == element.Attribute("method")?.Value)
                ?? Module.ModuleProductions.First();
            _SelectedMethod = SelectedMethod;
            EditEquipmentCommand = new DelegateCommand(EditEquipment);

            // タレットとシールドを追加
            var turrets = element.Element("turrets").Elements("turret");
            var shields = element.Element("shields").Elements("shield");
            var equipments = turrets.Concat(shields)
                .Select(elem => Equipment.Get(elem.Attribute("id").Value))
                .Where(e => e != null);
            foreach (var equipment in equipments)
            {
                AddEquipment(equipment!);
            }
        }


        /// <summary>
        /// xml化する
        /// </summary>
        /// <returns>xml化した情報</returns>
        public XElement ToXml()
        {
            // それぞれのモジュールの情報を設定
            var ret = new XElement("module");
            ret.Add(new XAttribute("id", Module.ModuleID));
            ret.Add(new XAttribute("count", ModuleCount));
            ret.Add(new XAttribute("method", SelectedMethod.Method));

            // タレットとシールドをXML化
            ret.Add(ModuleEquipment.Turret.Serialize());
            ret.Add(ModuleEquipment.Shield.Serialize());

            return ret;
        }


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="equipment">追加したい装備</param>
        public void AddEquipment(Equipment equipment) => ModuleEquipment.AddEquipment(equipment);


        /// <summary>
        /// ツールチップ文字列を更新する
        /// </summary>
        public void UpdateTooltip()
        {
            if (ModuleEquipment.CanEquipped)
            {
                RaisePropertyChanged(nameof(TurretsToolTip));
                RaisePropertyChanged(nameof(ShieldsToolTip));
            }
        }


        /// <summary>
        /// 装備を編集
        /// </summary>
        private void EditEquipment()
        {
            // 変更前
            var turretsOld = ModuleEquipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();
            var shieldsOld = ModuleEquipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x).ToArray();

            var window = new EditEquipmentWindow(this);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();

            bool equipmentChanged = false;

            // 変更があった場合のみ通知
            if (!turretsOld.SequenceEqual(ModuleEquipment.Turret.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(TurretsCount));
                RaisePropertyChanged(nameof(TurretsToolTip));
                equipmentChanged = true;
            }

            if (!shieldsOld.SequenceEqual(ModuleEquipment.Shield.AllEquipments.Select(x => x.EquipmentID).OrderBy(x => x)))
            {
                RaisePropertyChanged(nameof(ShieldsCount));
                RaisePropertyChanged(nameof(ShieldsToolTip));
                equipmentChanged = true;
            }

            if (equipmentChanged)
            {
                RaisePropertyChangedEx(turretsOld.Concat(shieldsOld), ModuleEquipment.AllEquipments.Select(x => x.EquipmentID).ToArray(), nameof(ModuleEquipment));
                EditStatus = EditStatus.Edited;
            }
        }

        /// <summary>
        /// 装備のツールチップ文字列を作成
        /// </summary>
        /// <returns></returns>
        private string MakeEquipmentToolTipString(ModuleEquipmentCollection equipmentCollections)
        {
            var sb = new StringBuilder();

            foreach (var size in equipmentCollections.Sizes)
            {
                var cnt = 1;

                foreach (var eq in equipmentCollections.GetEquipment(size))
                {
                    if (cnt == 1)
                    {
                        if (sb.Length != 0)
                        {
                            sb.AppendLine();
                        }
                        sb.AppendLine($"【{size.Name}】");
                    }
                    sb.AppendLine($"{cnt++:D2} ： {eq.Name}");
                }
            }

            if (sb.Length == 0)
            {
                sb.Append((string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:NotEquippedToolTipText", null, null));
            }

            return sb.ToString();
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Module, ModuleCount, ModuleEquipment, SelectedMethod);
        }
    }
}
