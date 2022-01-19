namespace com.spacepuppy
{

    /// <summary>
    /// Just a name for contract purposes. You should probably inherit from RadicalYieldInstruciton, or composite it, unless 
    /// you know what you're doing.
    /// </summary>
    public interface IRadicalYieldInstruction
    {
        /// <summary>
        /// The instruction completed, but not necessarily successfully.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Called every tick of the coroutine and handing out a potential yield instruction. 
        /// This should behave just like IsComplete but offering out some yield instruction. 
        /// The instruction should not rely on this being called as Manual/Task/UniTask won't necessarily 'tick' the instruction. 
        /// </summary>
        /// <param name="yieldObject">An object to treat as the yield object between now and the next call to Tick.</param>
        /// <returns>True to continue blocking, false to stop blocking.</returns>
        bool Tick(out object yieldObject);

    }

    /// <summary>
    /// Base abstract class that implements IRadicalYieldInstruction. It implements IRadicalYieldInstruction in the most 
    /// commonly used setup. You should only ever implement IRadicalYieldInstruction directly if you can't inherit from this 
    /// in your inheritance chain, or you want none standard behaviour.
    /// </summary>
    public abstract class RadicalYieldInstruction : IRadicalYieldInstruction, System.Collections.IEnumerator
    {

        #region Properties

        public bool IsComplete
        {
            get
            {
                object obj;
                return this.SafeIsComplete || this.TestIfComplete(out obj);
            }
        }

        /// <summary>
        /// Testing IsComplete actually calls the polling method TestIfComplete. 
        /// This is property returns the state value rather than calling the polling method.
        /// </summary>
        protected bool SafeIsComplete
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        protected virtual void SetSignal()
        {
            this.SafeIsComplete = true;
        }

        protected void ResetSignal()
        {
            this.SafeIsComplete = false;
        }

        protected virtual bool TestIfComplete(out object yieldObject)
        {
            yieldObject = null;
            return !this.SafeIsComplete;
        }

        #endregion

        #region IRadicalYieldInstruction Interface

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            if (this.SafeIsComplete)
            {
                yieldObject = null;
                return false;
            }

            return this.TestIfComplete(out yieldObject);
        }

        #endregion

        #region IEnumerator Interface

        object System.Collections.IEnumerator.Current => null;

        bool System.Collections.IEnumerator.MoveNext()
        {
            if (this.SafeIsComplete)
            {
                return false;
            }

            object inst;
            return !this.TestIfComplete(out inst);
        }

        void System.Collections.IEnumerator.Reset()
        {
            //do nothing
        }

        #endregion

        #region Static Interface

        public static IProgressingYieldInstruction Null
        {
            get
            {
                return NullYieldInstruction.Null;
            }
        }

        #endregion

    }

}
