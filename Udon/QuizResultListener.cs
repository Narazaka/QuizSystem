using UdonSharp;

namespace Narazaka.VRChat.QuizSystem
{
    public abstract class QuizResultListener : UdonSharpBehaviour
    {
        public virtual void _OnQuizFinished() { }
        public virtual void _OnQuizSuccess() { }
        public virtual void _OnQuizFailure() { }
    }
}
