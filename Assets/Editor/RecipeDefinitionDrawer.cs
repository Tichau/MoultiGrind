using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(RecipeDefinition))]
public class RecipeDefinitionDrawer : Editor
{
    private SerializedProperty inputProperty;
    private ReorderableList inputList;

    private SerializedProperty descriptionProperty;
    private SerializedProperty durationProperty;
    private SerializedProperty outputProperty;
    private ReorderableList outputList;

    private void OnEnable()
    {
        this.descriptionProperty = this.serializedObject.FindProperty("Description");
        this.durationProperty = this.serializedObject.FindProperty("fixedPointDuration");

        this.inputProperty = this.serializedObject.FindProperty("Inputs");
        this.inputList = new ReorderableList(this.serializedObject, inputProperty, true, false, true, true);
        this.inputList.drawElementCallback += this.DrawInputElement;

        this.outputProperty = this.serializedObject.FindProperty("Outputs");
        this.outputList = new ReorderableList(this.serializedObject, outputProperty, true, false, true, true);
        this.outputList.drawElementCallback += this.DrawOutputElement;
    }

    private void OnDisable()
    {
        this.durationProperty = null;

        this.inputProperty = null;
        this.inputList.drawElementCallback -= this.DrawInputElement;

        this.outputProperty = null;
        this.outputList.drawElementCallback -= this.DrawOutputElement;
    }

    private void DrawInputElement(Rect rect, int index, bool active, bool focused)
    {
        var elementRect = new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6);
        EditorGUI.PropertyField(elementRect, this.inputProperty.GetArrayElementAtIndex(index));
    }

    private void DrawOutputElement(Rect rect, int index, bool active, bool focused)
    {
        var elementRect = new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6);
        EditorGUI.PropertyField(elementRect, this.outputProperty.GetArrayElementAtIndex(index));
    }

    public override void OnInspectorGUI()
    {
        RecipeDefinition myTarget = (RecipeDefinition)target;
        
        EditorGUILayout.PropertyField(descriptionProperty);

        var previousColor = GUI.color;
        if (this.durationProperty.longValue == 0)
        {
            GUI.color = new Color(1f, 0.53f, 0.49f);
        }

        double ratio = this.durationProperty.longValue / 1000.0;
        ratio = EditorGUILayout.DoubleField("Duration", ratio);
        this.durationProperty.longValue = (long)Math.Round(ratio * 1000);

        GUI.color = previousColor;

        EditorGUILayout.LabelField(this.inputProperty.name, EditorStyles.boldLabel);
        this.inputList.DoLayoutList();

        EditorGUILayout.LabelField(this.outputProperty.name, EditorStyles.boldLabel);
        this.outputList.DoLayoutList();

        this.serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}
