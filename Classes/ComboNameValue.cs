/*
 * Copyright © 2011-2012 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    public class ComboNameValue<T>
    {
        public ComboNameValue(string displayName, T value)
        {
            this.DisplayName = displayName;
            this.Value = value;
        }

        public string DisplayName { get; set; }

        public T Value { get; set; }

        // Overridden to return the display name, as this is what the combobox shows
        public override string ToString()
        {
            return this.DisplayName;
        }

        // This allows setting the selected item with just the value, as it checkes if the
        // value is of the same type as the itemValue and if so, does a direct comparison
        public override bool Equals(object obj)
        {
            if (obj is T)
            {
                return ((T)obj).Equals(this.Value);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            // Return GetHashCode for the value that this instance represents
            return this.Value.GetHashCode();
        }
    }
}
