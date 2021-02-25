using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Utilities {
    public class EnumDropdown<T> where T : Enum {
        public T EnumFrom(int index) {
            Array x = Enum.GetValues(typeof(T));
            object value = null;
            if (index >= 0 && x.Length > index) {
                value = x.GetValue(index);
            }
            return (T) value;
        }

        public void PopulateOptions(TMP_Dropdown dropdown, int defaultValue = 0) {
            if (dropdown == null) return;
            string[] enumNames = Enum.GetNames(typeof(T));
            List<string> namesList = enumNames.ToList();
            dropdown.ClearOptions();
            dropdown.AddOptions(namesList);
            dropdown.value = defaultValue;
        }

        public void PopulateOptions(TMP_Dropdown dropdown, T defaultOption) {
            if (dropdown == null) return;
            int index = Convert.ToInt32(defaultOption); 
            PopulateOptions(dropdown, index );
        }
    }
}