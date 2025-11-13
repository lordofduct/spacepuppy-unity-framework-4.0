namespace com.spacepuppy
{

    /// <summary>
    /// By implementing this interface, the RadicalCoroutine knows to call Dispose on the instruction when it is 
    /// done yielding for its duration. The instruction can than clean itself up on complete or pool itself if it's 
    /// a pooled instruction.
    /// 
    /// A recycled/pooled instruction should return IsComplete = true until it is recycled again.
    /// </summary>
    public interface IDisposableYieldInstruction : IRadicalYieldInstruction, System.IDisposable
    {

    }

}
