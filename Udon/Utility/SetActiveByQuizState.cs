using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.QuizSystem.Utility
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SetActiveByQuizState : QuizStateListener
    {
        [SerializeField] GameObject[] successObjects = new GameObject[0];
        [SerializeField] GameObject[] failureObjects = new GameObject[0];

        bool success;
        bool failure;

        public override void _OnQuizStarted()
        {
            success = false;
            failure = false;
            SetActives();
        }

        public override void _OnQuizSuccess()
        {
            success = true;
            failure = false;
            SetActives();
        }

        public override void _OnQuizFailure()
        {
            success = false;
            failure = true;
            SetActives();
        }

        void SetActives()
        {
            foreach (var obj in successObjects)
            {
                obj.SetActive(success);
            }
            foreach (var obj in failureObjects)
            {
                obj.SetActive(failure);
            }
        }
    }
}
