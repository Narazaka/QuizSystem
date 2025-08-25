using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Narazaka.VRChat.QuizSystem.Editor
{
    public class QuizSystemBuilder : IProcessSceneWithReport
    {
        public int callbackOrder => -2048;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var quizSystems = Object.FindObjectsByType<Runtime.QuizSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var quizSystem in quizSystems)
            {
                var quizManager = quizSystem.quizManager;
                if (quizManager == null)
                {
                    Debug.LogError($"QuizSystem '{quizSystem.name}' has no QuizManager assigned.", quizSystem);
                    continue;
                }

                var quizzes = quizSystem.quizzes;
                quizManager.questions = quizzes.Select(q => q.question).ToArray();
                quizManager.answers = quizzes.Select(q => q.answers.Select(a => a.text).ToArray()).ToArray();
                quizManager.correctAnswers = quizzes.Select(q => q.answers.Select(a => a.isCorrect).ToArray()).ToArray();
            }
        }
    }
}
