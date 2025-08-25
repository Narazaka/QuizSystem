using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

[assembly: InternalsVisibleTo("Narazaka.VRChat.QuizSystem.Editor")]

namespace Narazaka.VRChat.QuizSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class QuizManager : UdonSharpBehaviour
    {
        [SerializeField] bool autoStart = true;
        [SerializeField] public bool randomizeQuestionOrder = true;
        [SerializeField] public bool randomizeAnswerOrder = true;
        // クリアに必要な正解数
        [SerializeField] public int requiredCorrectAnswers = 3;
        // 間違い許容数
        [SerializeField] public int allowedIncorrectAnswers = 3;
        [SerializeField] UdonBehaviour[] listeners = new UdonBehaviour[0];

        [SerializeField] GameObject quizView;
        [SerializeField] GameObject successView;
        [SerializeField] GameObject failureView;
        [SerializeField] TextSetter questionText;
        [SerializeField] TextSetter[] answerTexts;
        [SerializeField] AnswerLayouter answerLayouter;

        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip correctSound;
        [SerializeField] AudioClip incorrectSound;

        [SerializeField] internal string[] questions;
        [SerializeField] internal string[][] answers;
        [SerializeField] internal bool[][] correctAnswers;

        bool started = false;
        byte currentQuestionIndex = 0;
        byte correctAnswerCount = 0;
        byte incorrectAnswerCount = 0;

        bool finished => currentQuestionIndex >= questions.Length || correctAnswerCount >= requiredCorrectAnswers || incorrectAnswerCount > allowedIncorrectAnswers;
        bool finishNotified;

        [PublicAPI]
        public bool IsSuccess => correctAnswerCount >= requiredCorrectAnswers && incorrectAnswerCount <= allowedIncorrectAnswers;

        void OnEnable()
        {
            if (autoStart && !started)
            {
                _StartQuiz();
            }
        }

        [PublicAPI]
        public void _StartQuiz()
        {
            if (started)
            {
                // 既に開始している場合は無視
                return;
            }
            // 問題と回答をランダム化
            if (randomizeQuestionOrder)
            {
                RandomizeQuestions();
            }
            started = true;
            currentQuestionIndex = 0;
            correctAnswerCount = 0;
            incorrectAnswerCount = 0;
            finishNotified = false;
            UpdateView();
            Notify(nameof(QuizStateListener._OnQuizStarted));
        }

        [PublicAPI]
        public void _ReStartQuiz()
        {
            started = false;
            _StartQuiz();
        }

        public void _Answer(int index)
        {
            if (finished)
            {
                // 既に終了している場合は無視
                return;
            }
            var currentCorrectAnswers = correctAnswers[currentQuestionIndex];
            if (index < 0 || index >= currentCorrectAnswers.Length)
            {
                // 範囲外の回答は無視
                return;
            }
            var isCorrect = currentCorrectAnswers[index];
            if (isCorrect)
            {
                correctAnswerCount++;
                if (audioSource != null && correctSound != null)
                {
                    audioSource.PlayOneShot(correctSound);
                }
            }
            else
            {
                incorrectAnswerCount++;
                if (audioSource != null && incorrectSound != null)
                {
                    audioSource.PlayOneShot(incorrectSound);
                }
            }
            currentQuestionIndex++;
            UpdateView();
        }

        [PublicAPI]
        public void _AddListener(UdonBehaviour listener)
        {
            if (listener == null) return;
            foreach (var l in listeners)
            {
                if (l == listener) return;
            }
            var newListeners = new UdonBehaviour[listeners.Length + 1];
            Array.Copy(listeners, newListeners, listeners.Length);
            newListeners[listeners.Length] = listener;
            listeners = newListeners;
        }

        void RandomizeQuestions()
        {
            var rng = new System.Random();
            var n = questions.Length;
            // Fisher-Yates shuffle
            for (var i = n - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                // Swap questions
                var tempQuestion = questions[i];
                questions[i] = questions[j];
                questions[j] = tempQuestion;
                // Swap answers
                var tempAnswers = answers[i];
                answers[i] = answers[j];
                answers[j] = tempAnswers;
                // Swap correct answer indices
                var tempCorrectAnswers = correctAnswers[i];
                correctAnswers[i] = correctAnswers[j];
                correctAnswers[j] = tempCorrectAnswers;
            }
        }

        void UpdateView()
        {
            if (finished)
            {
                NotifyFinish();
                _ShowResult();
                return;
            }
            ShowCurrentQuestion();
        }

        void NotifyFinish()
        {
            if (!finishNotified)
            {
                finishNotified = true;
                foreach (var listener in listeners)
                {
                    if (listener != null)
                    {
                        listener.SendCustomEvent(nameof(QuizStateListener._OnQuizFinished));
                        listener.SendCustomEvent(IsSuccess ? nameof(QuizStateListener._OnQuizSuccess) : nameof(QuizStateListener._OnQuizFailure));
                    }
                }
            }
        }

        void Notify(string eventName)
        {
            foreach (var listener in listeners)
            {
                if (listener != null)
                {
                    listener.SendCustomEvent(eventName);
                }
            }
        }

        void ShowCurrentQuestion()
        {
            quizView.SetActive(true);
            successView.SetActive(false);
            failureView.SetActive(false);

            questionText.text = questions[currentQuestionIndex];
            var currentAnswers = answers[currentQuestionIndex];
            var currentCorrectAnswers = correctAnswers[currentQuestionIndex];
            // 回答をランダム化
            if (randomizeAnswerOrder)
            {
                var rng = new System.Random();
                var n = currentAnswers.Length;
                // Fisher-Yates shuffle
                for (var i = n - 1; i > 0; i--)
                {
                    var j = rng.Next(i + 1);
                    // Swap answers
                    var tempAnswer = currentAnswers[i];
                    currentAnswers[i] = currentAnswers[j];
                    currentAnswers[j] = tempAnswer;
                    // Swap correct answers
                    var tempCorrectAnswer = currentCorrectAnswers[i];
                    currentCorrectAnswers[i] = currentCorrectAnswers[j];
                    currentCorrectAnswers[j] = tempCorrectAnswer;
                }
            }
            for (var i = 0; i < answerTexts.Length; i++)
            {
                if (i < currentAnswers.Length)
                {
                    answerTexts[i].text = currentAnswers[i];
                }
                else
                {
                    answerTexts[i].text = "";
                }
            }
            answerLayouter._Layout();
        }

        void _ShowResult()
        {
            quizView.SetActive(false);
            var success = IsSuccess;
            successView.SetActive(success);
            failureView.SetActive(!success);
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        [CustomEditor(typeof(QuizManager))]
        public class QuizManagerEditor : UnityEditor.Editor
        {
            internal bool skipUdonSharpGUI;

            SerializedProperty autoStart;
            SerializedProperty randomizeQuestionOrder;
            SerializedProperty randomizeAnswerOrder;
            SerializedProperty requiredCorrectAnswers;
            SerializedProperty allowedIncorrectAnswers;
            SerializedProperty listeners;
            SerializedProperty quizView;
            SerializedProperty successView;
            SerializedProperty failureView;
            SerializedProperty questionText;
            SerializedProperty answerTexts;
            SerializedProperty answerLayouter;
            SerializedProperty audioSource;
            SerializedProperty correctSound;
            SerializedProperty incorrectSound;
            SerializedProperty questions;
            SerializedProperty answers;
            SerializedProperty correctAnswers;

            bool detail;

            void OnEnable()
            {
                autoStart = serializedObject.FindProperty(nameof(QuizManager.autoStart));
                randomizeQuestionOrder = serializedObject.FindProperty(nameof(QuizManager.randomizeQuestionOrder));
                randomizeAnswerOrder = serializedObject.FindProperty(nameof(QuizManager.randomizeAnswerOrder));
                requiredCorrectAnswers = serializedObject.FindProperty(nameof(QuizManager.requiredCorrectAnswers));
                allowedIncorrectAnswers = serializedObject.FindProperty(nameof(QuizManager.allowedIncorrectAnswers));
                listeners = serializedObject.FindProperty(nameof(QuizManager.listeners));
                quizView = serializedObject.FindProperty(nameof(QuizManager.quizView));
                successView = serializedObject.FindProperty(nameof(QuizManager.successView));
                failureView = serializedObject.FindProperty(nameof(QuizManager.failureView));
                questionText = serializedObject.FindProperty(nameof(QuizManager.questionText));
                answerTexts = serializedObject.FindProperty(nameof(QuizManager.answerTexts));
                answerLayouter = serializedObject.FindProperty(nameof(QuizManager.answerLayouter));
                audioSource = serializedObject.FindProperty(nameof(QuizManager.audioSource));
                correctSound = serializedObject.FindProperty(nameof(QuizManager.correctSound));
                incorrectSound = serializedObject.FindProperty(nameof(QuizManager.incorrectSound));
                questions = serializedObject.FindProperty(nameof(QuizManager.questions));
                answers = serializedObject.FindProperty(nameof(QuizManager.answers));
                correctAnswers = serializedObject.FindProperty(nameof(QuizManager.correctAnswers));
            }

            public override void OnInspectorGUI()
            {
                if (!skipUdonSharpGUI)
                {
                    if (UdonSharpEditor.UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
                }
                serializedObject.UpdateIfRequiredOrScript();
                EditorGUILayout.PropertyField(autoStart);
                EditorGUILayout.PropertyField(randomizeQuestionOrder);
                EditorGUILayout.PropertyField(randomizeAnswerOrder);
                EditorGUILayout.PropertyField(requiredCorrectAnswers);
                EditorGUILayout.PropertyField(allowedIncorrectAnswers);
                EditorGUILayout.PropertyField(correctSound);
                EditorGUILayout.PropertyField(incorrectSound);
                EditorGUILayout.PropertyField(listeners, true);

                if (detail = EditorGUILayout.Foldout(detail, "Detail"))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(quizView);
                        EditorGUILayout.PropertyField(successView);
                        EditorGUILayout.PropertyField(failureView);
                        EditorGUILayout.PropertyField(questionText);
                        EditorGUILayout.PropertyField(answerTexts, true);
                        EditorGUILayout.PropertyField(answerLayouter);
                        EditorGUILayout.PropertyField(audioSource);
                        // EditorGUILayout.PropertyField(questions, true);
                        // EditorGUILayout.PropertyField(answers, true);
                        // EditorGUILayout.PropertyField(correctAnswers, true);
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
