using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Collections;

namespace com.spacepuppy.SPInput
{
    public class CursorContext : SPComponent
    {

        #region Multiton Interface

        public static readonly MultitonPool<CursorContext> Pool = new MultitonPool<CursorContext>();

        public static CursorContext GetContext(CursorInputLogic cursor, object token)
        {
            var e = Pool.GetEnumerator();
            while(e.MoveNext())
            {
                if((e.Current._pointerFilter == null || e.Current._pointerFilter.IsValid(cursor)) && object.Equals(e.Current._token, token))
                {
                    return e.Current;
                }
            }
            return null;
        }

        public static IEnumerable<CursorContext> GetAllContexts(CursorInputLogic cursor, object token)
        {
            var e = Pool.GetEnumerator();
            while (e.MoveNext())
            {
                if ((e.Current._pointerFilter == null || e.Current._pointerFilter.IsValid(cursor)) && object.Equals(e.Current._token, token))
                {
                    yield return e.Current;
                }
            }
        }

        public static int GetAllContexts(CursorInputLogic cursor, object token, ICollection<CursorContext> buffer)
        {
            if (buffer == null) throw new System.ArgumentNullException(nameof(buffer));

            int cnt = 0;
            var e = Pool.GetEnumerator();
            while (e.MoveNext())
            {
                if ((e.Current._pointerFilter == null || e.Current._pointerFilter.IsValid(cursor)) && object.Equals(e.Current._token, token))
                {
                    cnt++;
                    buffer.Add(e.Current);
                }
            }
            return cnt;
        }

        #endregion

        #region Fields

        [SerializeField]
        private PointerFilter _pointerFilter;
        [SerializeField]
        private UnityEngine.Object _token;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            Pool.AddReference(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Pool.RemoveReference(this);
            base.OnDisable();
        }

        #endregion

        #region Properties

        public PointerFilter PointerFilter
        {
            get => _pointerFilter;
            set => _pointerFilter = value;
        }

        public UnityEngine.Object Token
        {
            get => _token;
            set => _token = value;
        }

        #endregion

        #region Methods

        #endregion

    }
}
