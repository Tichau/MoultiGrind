using System;
using Gameplay;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ResourceDefinition))]
public class ResourceDefinitionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        var labelWidth = EditorGUIUtility.labelWidth;
        var rect = new Rect(position.x, position.y, labelWidth, position.height);

        var nameProperty = property.FindPropertyRelative("Name");
        EditorGUI.PropertyField(rect, nameProperty, GUIContent.none);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        rect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

        //// Draw fields - passs GUIContent.none to each so they are drawn without labels
        var amountProperty = property.FindPropertyRelative("fixedPointAmount");
        double ratio = amountProperty.longValue / 1000.0;

        ratio = EditorGUI.DoubleField(rect, ratio);

        amountProperty.longValue = (long)Math.Round(ratio * 1000);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}