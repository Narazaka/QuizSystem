using UdonSharp;

namespace Narazaka.VRChat.QuizSystem
{
    public abstract class QuizStateListener : UdonSharpBehaviour
    {
        public virtual void _OnQuizStarted() { }
        public virtual void _OnQuizFinished() { }
        public virtual void _OnQuizSuccess() { }
        public virtual void _OnQuizFailure() { }
    }
}
