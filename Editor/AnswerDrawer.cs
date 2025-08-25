using UnityEngine;
using UnityEditor;
using Narazaka.VRChat.QuizSystem.Runtime;

namespace Narazaka.VRChat.QuizSystem.Editor
{
    [CustomPropertyDrawer(typeof(Answer))]
    public class AnswerDrawer : PropertyDrawer
    {
        static Color correctColor = new Color(0.3f, 0.6f, 0.3f, 0.3f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var textProp = property.FindPropertyRelative(nameof(Answer.text));
            var isCorrectProp = property.FindPropertyRelative(nameof(Answer.isCorrect));
            if (isCorrectProp.boolValue)
            {
                var bgRect = position;
                var margin = EditorGUIUtility.standardVerticalSpacing;
                bgRect.xMin -= margin;
                bgRect.xMax += margin;
                bgRect.yMin -= margin;
                bgRect.yMax += margin;
                EditorGUI.DrawRect(bgRect, correctColor);
            }
            EditorGUI.BeginProperty(position, label, property);
            position.height -= (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.PropertyField(position, textProp);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, isCorrectProp);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var textProp = property.FindPropertyRelative(nameof(Answer.text));
            var isCorrectProp = property.FindPropertyRelative(nameof(Answer.isCorrect));
            return EditorGUI.GetPropertyHeight(textProp) + EditorGUI.GetPropertyHeight(isCorrectProp) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
