using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.QuizSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TextSetter : UdonSharpBehaviour
    {
        [SerializeField] TextMeshProUGUI tmpText;

        public string text
        {
            get
            {
                if (tmpText != null)
                {
                    return tmpText.text ?? "";
                }
                return "";
            }
            set
            {
                if (tmpText != null)
                {
                    tmpText.text = value;
                }
            }
        }
    }
}
