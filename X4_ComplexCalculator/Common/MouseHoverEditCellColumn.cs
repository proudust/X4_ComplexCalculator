﻿using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// マウスホバー時に編集モードになるセル
    /// </summary>
    public class MouseHoverEditCellColumn : DataGridTemplateColumn
    {
        /// <summary>
        /// セル作成
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var behavior = new DataGridMouseEnterEditModeBehavior();

            Interaction.GetBehaviors(cell).Add(behavior);

            return base.GenerateElement(cell, dataItem);
        }
    }
}