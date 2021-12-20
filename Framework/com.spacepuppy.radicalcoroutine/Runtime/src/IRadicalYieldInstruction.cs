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
        /// Process the tick of the coroutine, returning true if the instruction should continue blocking, false if it should stop blocking.
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

        #region Fields

        private bool _complete;

        #endregion

        #region Properties

        public bool IsComplete { get { return _complete; } }

        #endregion

        #region Methods

        protected virtual void SetSignal()
        {
            _complete = true;
        }

        protected void ResetSignal()
        {
            _complete = false;
        }

        protected virtual bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !_complete;
        }

        #endregion

        #region IRadicalYieldInstruction Interface

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            if (_complete)
            {
                yieldObject = null;
                return false;
            }

            return this.Tick(out yieldObject);
        }

        #endregion

        #region IEnumerator Interface

        object System.Collections.IEnumerator.Current => null;

        bool System.Collections.IEnumerator.MoveNext()
        {
            if (_complete)
            {
                return false;
            }

            object inst;
            return this.Tick(out inst);
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
