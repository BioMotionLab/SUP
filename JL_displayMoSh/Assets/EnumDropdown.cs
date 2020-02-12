using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class EnumDropdown<T> where T : Enum {
    public T EnumFrom(int index) {
        Array x = Enum.GetValues(typeof(T));
        object value = null;
        if (index >= 0 && x.Length > index) {
            value = x.GetValue(index);
        }
        return (T) value;
    }

    public void PopulateOptions(TMP_Dropdown dropdown) {
        string[] enumNames = Enum.GetNames(typeof(T));
        List<string> namesList = enumNames.ToList();
        dropdown.ClearOptions();
        dropdown.AddOptions(namesList);
    }
}