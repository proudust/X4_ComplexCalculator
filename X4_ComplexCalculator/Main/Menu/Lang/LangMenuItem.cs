﻿using Prism.Mvvm;
using System.Globalization;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.Menu.Lang
{
    /// <summary>
    /// 言語メニュー1レコード分
    /// </summary>
    public class LangMenuItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 言語
        /// </summary>
        private readonly CultureInfo _CultureInfo;


        /// <summary>
        /// チェックされたか
        /// </summary>
        private bool _IsChecked;
        #endregion


        #region プロパティ
        /// <summary>
        /// チェックされたか
        /// </summary>
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (SetProperty(ref _IsChecked, value))
                {
                    if (IsChecked)
                    {
                        LocalizeDictionary.Instance.Culture = _CultureInfo;

                        Configuration.SetValue("AppSettings.Language", _CultureInfo.Name);
                    }
                }
            }
        }


        /// <summary>
        /// 言語名
        /// </summary>
        public string Name => _CultureInfo.NativeName;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cultureInfo">言語情報</param>
        public LangMenuItem(CultureInfo cultureInfo)
        {
            _CultureInfo = cultureInfo;

            if (cultureInfo.Name == LocalizeDictionary.CurrentCulture.Name)
            {
                IsChecked = true;
            }
        }
    }
}
