using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BezierCurve bezierCurve = (BezierCurve)target;

        serializedObject.Update();

        SerializedProperty property = serializedObject.FindProperty("controlPointsPositions");

        // Accessing the entire list object
        object obj = property.serializedObject.targetObject;
        System.Type type = obj.GetType();

        // Using reflection to get the value of the property
        System.Reflection.FieldInfo field = type.GetField(property.propertyPath);
        Vector3[] propertyValue = (Vector3[])field.GetValue(obj);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Add Curve"))
        {
            bezierCurve.CreateControlPoints(propertyValue);
        }

        if (GUILayout.Button("Align tangents"))
        {
            bezierCurve.AlignTangents();
        }

    }
}
