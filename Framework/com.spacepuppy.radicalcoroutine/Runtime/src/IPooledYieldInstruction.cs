namespace com.spacepuppy
{

    /// <summary>
    /// By implementing this interface, the RadicalCoroutine knows to call Dispose on the instruction when it is 
    /// done yielding for its duration. The instruction can than return itself to a object cache pool during Dispose 
    /// for reuse later.
    /// 
    /// A recycled/pooled instruction should return IsComplete = true until it is recycled again.
    /// </summary>
    public interface IPooledYieldInstruction : IRadicalYieldInstruction, System.IDisposable
    {

    }

}
