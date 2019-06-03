using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		string typeName = this.fieldInfo.ReflectedType.Name;
		if (typeName != "TowerData" && typeName != "LineData") {
			HexCoordinates coordinates = new HexCoordinates(
				property.FindPropertyRelative("x").intValue,
				property.FindPropertyRelative("y").intValue
			);

			position = EditorGUI.PrefixLabel(position, label);
			GUI.Label(position, coordinates.ToString());
		}
		else {
			var x = property.FindPropertyRelative("x");
			var y = property.FindPropertyRelative("y");
			Vector2 v = EditorGUI.Vector2Field(position, label, new Vector2(x.intValue, y.intValue));
			x.intValue = (int)v.x;
			y.intValue = (int)v.y;
			property.serializedObject.ApplyModifiedProperties();
		}
	}
}