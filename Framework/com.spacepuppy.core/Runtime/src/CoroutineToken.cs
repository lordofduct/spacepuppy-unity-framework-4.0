using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy
{

    public struct CoroutineToken : System.IDisposable
    {

        public static readonly CoroutineToken Empty = new CoroutineToken();

        public MonoBehaviour Owner;
        public Coroutine Token;

        public CoroutineToken(MonoBehaviour owner, Coroutine routine)
        {
            this.Owner = owner;
            this.Token = routine;
        }

        public bool IsValid { get { return Owner != null && Token != null; } }

        public void Cancel()
        {
            if (Owner != null && Token != null) Owner.StopCoroutine(Token);
        }

        public void Dispose()
        {
            this.Cancel();
        }
    }

}
