using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Narazaka.VRChat.QuizSystem.Runtime;
using System.Collections.Generic;
using UnityEngine.UI;
using static Narazaka.VRChat.QuizSystem.QuizManager;

namespace Narazaka.VRChat.QuizSystem.Editor
{
    [CustomEditor(typeof(Runtime.QuizSystem))]
    public class QuizSystemEditor : UnityEditor.Editor
    {
        SerializedProperty maxChoiceCount;
        SerializedProperty choiceColumnCount;

        SerializedProperty quizManager;
        SerializedProperty answerLayouter;
        SerializedProperty answerRowPrefab;
        SerializedProperty answerButtonPrefab;
        SerializedProperty quizData;
        SerializedProperty quizPrefab;

        Transform quizDataTransform => quizData.objectReferenceValue as Transform;
        GameObject quizPrefabGameObject => quizPrefab.objectReferenceValue as GameObject;

        UnityEditor.Editor quizManagerEditor = null;

        List<Quiz> quizzes = new List<Quiz>();
        Dictionary<Quiz, UnityEditor.Editor> editors = new Dictionary<Quiz, UnityEditor.Editor>();
        ReorderableList _quizList;

        bool detail;

        void OnEnable()
        {
            maxChoiceCount = serializedObject.FindProperty(nameof(Runtime.QuizSystem.maxChoiceCount));
            choiceColumnCount = serializedObject.FindProperty(nameof(Runtime.QuizSystem.choiceColumnCount));

            quizManager = serializedObject.FindProperty(nameof(Runtime.QuizSystem.quizManager));
            answerLayouter = serializedObject.FindProperty(nameof(Runtime.QuizSystem.answerLayouter));
            answerRowPrefab = serializedObject.FindProperty(nameof(Runtime.QuizSystem.answerRowPrefab));
            answerButtonPrefab = serializedObject.FindProperty(nameof(Runtime.QuizSystem.answerButtonPrefab));
            quizData = serializedObject.FindProperty(nameof(Runtime.QuizSystem.quizData));
            quizPrefab = serializedObject.FindProperty(nameof(Runtime.QuizSystem.quizPrefab));
            Undo.undoRedoPerformed += GenerateQuizzes;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= GenerateQuizzes;
        }

        void GenerateQuizzes()
        {
            quizzes.Clear();
            if (quizDataTransform != null)
            {
                foreach (Transform t in quizDataTransform)
                {
                    var q = t.GetComponent<Quiz>();
                    if (q != null) quizzes.Add(q);
                }
            }
        }

        ReorderableList quizList
        {
            get
            {
                if (_quizList != null) return _quizList;
                GenerateQuizzes();
                var nameTitle = new GUIContent("Name");
                _quizList = new ReorderableList(quizzes, typeof(Quiz));
                _quizList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, nameof(Quiz));
                };
                _quizList.elementHeightCallback = (int index) =>
                {
                    var quiz = quizzes[index];
                    var editor = editors.ContainsKey(quiz) ? editors[quiz] : null;
                    CreateCachedEditor(quiz, typeof(QuizEditor), ref editor);
                    editors[quiz] = editor;
                    return (editor as QuizEditor).GetPropertyHeight() + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                };
                _quizList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var quiz = quizzes[index];

                    var nameRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.BeginChangeCheck();
                    var newName = EditorGUI.TextField(nameRect, nameTitle, quiz.gameObject.name);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(quiz.gameObject, "Rename Quiz");
                        quiz.gameObject.name = newName;
                    }
                    rect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    var editor = editors.ContainsKey(quiz) ? editors[quiz] : null;
                    CreateCachedEditor(quiz, typeof(QuizEditor), ref editor);
                    editors[quiz] = editor;
                    (editor as QuizEditor).OnInspectorGUI(rect);
                };
                _quizList.onAddCallback = (ReorderableList list) =>
                {
                    if (quizDataTransform == null || quizPrefabGameObject == null)
                    {
                        Debug.LogWarning("Quiz Data Transform or Quiz Prefab is not assigned.");
                        return;
                    }
                    var name = GameObjectUtility.GetUniqueNameForSibling(quizDataTransform, nameof(Quiz));
                    var go = PrefabUtility.InstantiatePrefab(quizPrefabGameObject, quizDataTransform) as GameObject;
                    go.name = name;
                    var q = go.GetComponent<Quiz>();
                    quizzes.Add(q);
                    Undo.RegisterCreatedObjectUndo(go, "Add Quiz");
                };
                _quizList.onRemoveCallback = (ReorderableList list) =>
                {
                    if (list.index < 0 || list.index >= quizzes.Count) return;
                    var q = quizzes[list.index];
                    quizzes.RemoveAt(list.index);
                    if (q != null)
                    {
                        Undo.DestroyObjectImmediate(q.gameObject);
                    }
                };
                _quizList.onReorderCallbackWithDetails = (ReorderableList list, int oldIndex, int newIndex) =>
                {
                    Undo.SetSiblingIndex(quizDataTransform.GetChild(oldIndex), newIndex, "Reorder Quiz");
                };
                return _quizList;
            }
        }

        public override void OnInspectorGUI()
        {
            Header("Layout");
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(maxChoiceCount);
            EditorGUILayout.PropertyField(choiceColumnCount);
            if (EditorGUI.EndChangeCheck())
            {
                DoLayoutAnswers();
            }
            if (detail = EditorGUILayout.Foldout(detail, "Detail"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(quizManager);
                EditorGUILayout.PropertyField(answerLayouter);
                EditorGUILayout.PropertyField(answerRowPrefab);
                EditorGUILayout.PropertyField(answerButtonPrefab);
                EditorGUILayout.PropertyField(quizData);
                EditorGUILayout.PropertyField(quizPrefab);
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            Header("Quiz Manager");
            EditorGUI.indentLevel++;
            CreateCachedEditor(quizManager.objectReferenceValue, typeof(QuizManager.QuizManagerEditor), ref quizManagerEditor);
            (quizManagerEditor as QuizManager.QuizManagerEditor).skipUdonSharpGUI = true;
            quizManagerEditor.OnInspectorGUI();
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            Header("Quizzes");
            var quizList = this.quizList; // ensure GenerateQuizzes

            var qm = quizManager.objectReferenceValue as QuizManager;
            if (qm.requiredCorrectAnswers > quizzes.Count)
            {
                EditorGUILayout.HelpBox($"Required Correct Answers ({qm.requiredCorrectAnswers}) cannot be greater than the number of quizzes ({quizzes.Count}).", MessageType.Error);
            }
            if (qm.requiredCorrectAnswers + qm.allowedIncorrectAnswers > quizzes.Count)
            {
                EditorGUILayout.HelpBox($"The sum of Required Correct Answers ({qm.requiredCorrectAnswers}) and Allowed Incorrect Answers ({qm.allowedIncorrectAnswers}) cannot be greater than the number of quizzes ({quizzes.Count}).", MessageType.Warning);
            }

            quizList.DoLayoutList();

            var quizSystem = target as Runtime.QuizSystem;
        }

        void Header(string title)
        {
            // colored bordered header
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 1.3f);
            var oldColor = GUI.color;
            GUI.color = Color.yellow;
            GUI.Box(rect, "");
            GUI.color = oldColor;
            rect.xMin += 4;
            EditorGUI.LabelField(rect, title, EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        internal void DoLayoutAnswers()
        {
            var quizManagerComponent = quizManager.objectReferenceValue as QuizManager;
            if (quizManagerComponent == null)
            {
                EditorUtility.DisplayDialog("Error", "Quiz Manager is not assigned.", "OK");
                return;
            }

            var answerRowPrefabGameObject = answerRowPrefab.objectReferenceValue as GameObject;
            var answerButtonPrefabGameObject = answerButtonPrefab.objectReferenceValue as GameObject;

            if (answerRowPrefabGameObject == null || answerButtonPrefabGameObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Answer Row Prefab or Answer Button Prefab is not assigned.", "OK");
                return;
            }

            var answerLayouterComponent = answerLayouter.objectReferenceValue as AnswerLayouter;
            if (answerLayouterComponent == null)
            {
                EditorUtility.DisplayDialog("Error", "Answer Layouter is not assigned.", "OK");
                return;
            }

            var answerLayouterTransform = answerLayouterComponent.transform;
            var maxChoices = Mathf.Max(1, maxChoiceCount.intValue);
            var choiceColumns = Mathf.Max(1, choiceColumnCount.intValue);
            var choiceRows = (maxChoices + choiceColumns - 1) / choiceColumns;

            var toDestroy = new List<GameObject>();
            foreach (Transform row in answerLayouterTransform)
            {
                toDestroy.Add(row.gameObject);
            }
            foreach (var go in toDestroy)
            {
                Undo.DestroyObjectImmediate(go);
            }

            var rows = new List<GameObject>();
            var answerTexts = new List<TextSetter>();

            for (var i = 0; i < choiceRows; i++)
            {
                var row = PrefabUtility.InstantiatePrefab(answerRowPrefabGameObject, answerLayouterTransform) as GameObject;
                row.name = $"Row {i}";
                rows.Add(row);
                Undo.RegisterCreatedObjectUndo(row, "Create Answer Row");
                var rowTransform = row.transform;
                for (var j = 0; j < choiceColumns; j++)
                {
                    var index = i * choiceColumns + j;
                    var button = PrefabUtility.InstantiatePrefab(answerButtonPrefabGameObject, rowTransform) as GameObject;
                    button.name = $"Answer {index + 1}";
                    var answerSender = button.GetComponent<AnswerSender>();
                    var textSetter = button.GetComponent<TextSetter>();
                    if (index >= maxChoices)
                    {
                        DestroyImmediate(answerSender);
                        DestroyImmediate(textSetter);
                        DestroyImmediate(button.GetComponent<Button>());
                        DestroyImmediate(button.GetComponent<Image>());
                    }
                    else
                    {
                        var answerSenderSo = new SerializedObject(answerSender);
                        answerSenderSo.Update();
                        answerSenderSo.FindProperty("quizManager").objectReferenceValue = quizManagerComponent;
                        answerSenderSo.FindProperty("answerIndex").intValue = index;
                        answerSenderSo.ApplyModifiedProperties();
                        answerTexts.Add(textSetter);
                    }
                    Undo.RegisterCreatedObjectUndo(button, "Create Answer Button");
                }
            }

            var quizManagerSo = new SerializedObject(quizManagerComponent);
            quizManagerSo.Update();
            var answerTextsProp = quizManagerSo.FindProperty("answerTexts");
            answerTextsProp.arraySize = answerTexts.Count;
            for (var i = 0; i < answerTexts.Count; i++)
            {
                answerTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = answerTexts[i];
            }
            quizManagerSo.ApplyModifiedProperties();

            var answerLayouterSo = new SerializedObject(answerLayouterComponent);
            answerLayouterSo.Update();
            var rowsProp = answerLayouterSo.FindProperty("rows");
            rowsProp.arraySize = answerLayouterTransform.childCount;
            for (var i = 0; i < answerLayouterTransform.childCount; i++)
            {
                rowsProp.GetArrayElementAtIndex(i).objectReferenceValue = rows[i];
            }
            var answerButtonsProp = answerLayouterSo.FindProperty("answerButtons");
            answerButtonsProp.arraySize = answerTexts.Count;
            for (var i = 0; i < answerTexts.Count; i++)
            {
                answerButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = answerTexts[i];
            }
            answerLayouterSo.ApplyModifiedProperties();
        }
    }
}
