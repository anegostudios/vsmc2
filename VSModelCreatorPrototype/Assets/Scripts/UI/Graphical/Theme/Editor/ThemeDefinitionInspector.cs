using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ThemeDefinition))]
public class ThemeDefinitionInspector : PropertyDrawer
{

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        var popup = new UnityEngine.UIElements.PopupWindow();
        SerializedProperty themeName = property.FindPropertyRelative("themeName");
        popup.Add(new PropertyField(themeName, "Theme Name"));
        popup.text = themeName.stringValue;
        property.FindPropertyRelative("themeColors").arraySize = Enum.GetValues(typeof(ThemeElementEnum)).Length;
        int ind = 0;
        foreach (string elem in Enum.GetNames(typeof(ThemeElementEnum)))
        {
            popup.Add(new PropertyField(property.FindPropertyRelative("themeColors").GetArrayElementAtIndex(ind), elem));
            ind++;
        }
        container.Add(popup);
        property.serializedObject.ApplyModifiedProperties();
        return container;
    }

}
