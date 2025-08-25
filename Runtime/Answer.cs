using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Narazaka.VRChat.QuizSystem.Runtime
{
    [Serializable]
    public class Answer
    {
        [TextArea(1, 2)]
        public string text;
        public bool isCorrect;
    }
}
