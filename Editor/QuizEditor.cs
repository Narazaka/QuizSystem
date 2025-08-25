using UnityEngine;
using UnityEditor;

namespace Narazaka.VRChat.QuizSystem.Editor
{
    [CustomEditor(typeof(Runtime.Quiz))]
    [CanEditMultipleObjects]
    public class QuizEditor : UnityEditor.Editor
    {
        SerializedProperty question;
        SerializedProperty answers;

        void OnEnable()
        {
            question = serializedObject.FindProperty(nameof(Runtime.Quiz.question));
            answers = serializedObject.FindProperty(nameof(Runtime.Quiz.answers));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(question);
            EditorGUILayout.PropertyField(answers, true);
            serializedObject.ApplyModifiedProperties();
            if (!HasCorrectAnswer())
            {
                EditorGUILayout.HelpBox("At least one answer must be marked as correct.", MessageType.Error);
            }
        }

        public float GetPropertyHeight()
        {
            float height = EditorGUI.GetPropertyHeight(question);
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(answers, true);
            if (!HasCorrectAnswer())
            {
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight * 2;
            }
            return height;
        }

        public void OnInspectorGUI(Rect position)
        {
            serializedObject.UpdateIfRequiredOrScript();
            var questionRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(question));
            EditorGUI.PropertyField(questionRect, question);
            var answersRect = new Rect(position.x, questionRect.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUI.GetPropertyHeight(answers, true));
            EditorGUI.PropertyField(answersRect, answers, true);
            serializedObject.ApplyModifiedProperties();
            if (!HasCorrectAnswer())
            {
                var helpRect = new Rect(position.x, answersRect.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight * 2);
                EditorGUI.HelpBox(helpRect, "At least one answer must be marked as correct.", MessageType.Error);
            }
        }

        bool HasCorrectAnswer()
        {
            for (int i = 0; i < answers.arraySize; i++)
            {
                var answer = answers.GetArrayElementAtIndex(i);
                var isCorrectProp = answer.FindPropertyRelative(nameof(Runtime.Answer.isCorrect));
                if (isCorrectProp.boolValue)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
