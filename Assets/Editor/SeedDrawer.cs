using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Seed))]
public class SeedDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

		EditorGUI.BeginChangeCheck();

		/***** USING PROPERTY FIELDS *****/
		//var fix = property.FindPropertyRelative("FixedSeed");
		//EditorGUI.PropertyField(
		//	new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + 2),
		//	fix, 
		//	new GUIContent("Fixed Seed"));
		//if (fix.boolValue) {
		//	EditorGUI.indentLevel++;
		//	var seedVal = property.FindPropertyRelative("SeedNum");
		//	EditorGUI.PropertyField(
		//		new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight + 2), 
		//		seedVal,
		//		new GUIContent("Seed Number"));
		//}

		/***** USING PARTICULAR CONTROLS *****/
		var fixedSeed = property.FindPropertyRelative("FixedSeed");
		fixedSeed.boolValue = EditorGUI.Toggle(
			new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + 2), 
			"Fixed Seed", 
			fixedSeed.boolValue);
		if (fixedSeed.boolValue) {
			EditorGUI.indentLevel++;
			var seedVal = property.FindPropertyRelative("SeedNum");
			seedVal.intValue = EditorGUI.IntField(
				new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight),
				"Seed Number",
				seedVal.intValue);
		}

		if (EditorGUI.EndChangeCheck()) {
			property.serializedObject.ApplyModifiedProperties();
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		if(property.FindPropertyRelative("FixedSeed").boolValue) {
			return EditorGUIUtility.singleLineHeight * 2 + 2;
		}
		else {
			return EditorGUIUtility.singleLineHeight + 2;
		}
	}
}