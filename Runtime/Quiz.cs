using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Narazaka.VRChat.QuizSystem.Runtime
{
    public class Quiz : MonoBehaviour, IEditorOnly
    {
        [TextArea(3, 3)]
        public string question;
        public Answer[] answers;
    }
}
