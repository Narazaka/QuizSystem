using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.QuizSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AnswerSender : UdonSharpBehaviour
    {
        [SerializeField] QuizManager quizManager;
        [SerializeField] int answerIndex;

        public void _SendAnswer()
        {
            quizManager._Answer(answerIndex);
        }
    }
}
