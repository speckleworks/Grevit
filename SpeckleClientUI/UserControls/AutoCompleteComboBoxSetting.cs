﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleClientUI.UserControls
{
    /// <summary>
    /// https://github.com/vain0x/DotNetKit.Wpf.AutoCompleteComboBox
    /// </summary>
    public class AutoCompleteComboBoxSetting
    {
        /// <summary>
        /// Gets a filter function which determines whether items should be suggested or not
        /// for the specified query.
        /// Default: Gets the filter which maps an item to <c>true</c>
        /// if its text contains the query (case insensitive).
        /// </summary>
        /// <param name="query">
        /// The string input by user.
        /// </param>
        /// <param name="stringFromItem">
        /// The function to get a string which identifies the specified item.
        /// </param>
        /// <returns></returns>
        public virtual Func<object, bool>
            GetFilter(string query, Func<object, string> stringFromItem)
        {
            return
                item =>
                    stringFromItem(item)
                    .IndexOf(query, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// Gets an integer.
        /// The combobox opens the drop down
        /// if the number of suggested items is less than the value.
        /// Note that the value is larger, it's heavier to open the drop down.
        /// Default: 100.
        /// </summary>
        public virtual int MaxSuggestionCount
        {
            get { return 1000; }
        }

        /// <summary>
        /// Gets the duration to delay updating the suggestion list.
        /// Returns <c>Zero</c> if no delay.
        /// Default: 300ms.
        /// </summary>
        public virtual TimeSpan Delay
        {
            get { return TimeSpan.FromMilliseconds(200.0); }
        }

        static AutoCompleteComboBoxSetting @default =
            new AutoCompleteComboBoxSetting();

        /// <summary>
        /// Gets the default setting.
        /// </summary>
        public static AutoCompleteComboBoxSetting Default
        {
            get { return @default; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                @default = value;
            }
        }
    }
}
