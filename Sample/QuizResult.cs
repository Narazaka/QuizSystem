using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.QuizSystem.Sample
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    internal class QuizResult : QuizResultListener
    {
        [SerializeField] GameObject successObject;
        [SerializeField] GameObject failureObject;

        public override void _OnQuizSuccess()
        {
            successObject.SetActive(true);
        }

        public override void _OnQuizFailure()
        {
            failureObject.SetActive(true);
        }
    }
}
