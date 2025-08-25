using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using VRC.SDKBase;

[assembly: InternalsVisibleTo("Narazaka.VRChat.QuizSystem.Editor")]

namespace Narazaka.VRChat.QuizSystem.Runtime
{
    public class QuizSystem : MonoBehaviour, IEditorOnly
    {
        [SerializeField] internal int maxChoiceCount = 2;
        [SerializeField] internal int choiceColumnCount = 2;
        [SerializeField] internal QuizManager quizManager;
        [SerializeField] internal AnswerLayouter answerLayouter;
        [SerializeField] internal GameObject answerRowPrefab;
        [SerializeField] internal GameObject answerButtonPrefab;
        [SerializeField] internal Transform quizData;
        [SerializeField] internal GameObject quizPrefab;

        internal Quiz[] quizzes
        {
            get
            {
                return quizData == null ? new Quiz[0] : quizData.GetComponentsInChildren<Quiz>();
            }
        }
    }
}
