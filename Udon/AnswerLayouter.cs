using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Narazaka.VRChat.QuizSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AnswerLayouter : UdonSharpBehaviour
    {
        [SerializeField] GameObject[] rows;
        [SerializeField] TextSetter[] answerButtons;

        public void _Layout()
        {
            // textが空のボタンは非表示にする
            // 全てのボタンが非表示になった行は非表示にする
            foreach (var button in answerButtons)
            {
                button.gameObject.SetActive(!string.IsNullOrEmpty(button.text));
            }
            foreach (var row in rows)
            {
                bool anyActive = false;
                foreach (Transform child in row.transform)
                {
                    if (child.gameObject.activeSelf)
                    {
                        anyActive = true;
                        break;
                    }
                }
                row.SetActive(anyActive);
            }
        }
    }
}
