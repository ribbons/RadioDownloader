/*
 * This file is part of Radio Downloader.
 * Copyright © 2007-2012 by the authors - see the AUTHORS file for details.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
