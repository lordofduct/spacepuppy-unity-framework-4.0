using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace com.spacepuppy.SPInput
{

    [CreateAssetMenu(fileName = "PointerFilter", menuName = "Spacepuppy/PointerFilter")]
    public sealed class PointerFilter : ScriptableObject
    {

        #region Fields

        [SerializeReference]
        [SerializeRefPicker(typeof(IPointerFilterTest), AlwaysExpanded = true)]
        private IPointerFilterTest _mode = new EventSystemPointerMode();

        #endregion

        #region Methods

        public bool IsValid(PointerEventData data)
        {
            return _mode?.IsValid(data) ?? true;
        }

        public bool IsValid(CursorInputLogic cursor)
        {
            return _mode?.IsValid(cursor) ?? true;
        }

        #endregion

        #region Special Types

        public interface IPointerFilterTest
        {
            bool IsValid(PointerEventData data);
            bool IsValid(CursorInputLogic cursor);
        }

        public enum PointerId
        {
            Any = int.MinValue,
            Touch = 0,
            LeftButton = -1,
            RightButton = -2,
            MiddleButton = -3,
        }

        [System.Serializable]
        public class EventSystemPointerMode : IPointerFilterTest
        {
            [SerializeField]
            private PointerId _pointerId = PointerId.Any;

            public PointerId PointerId
            {
                get => _pointerId;
                set => _pointerId = value;
            }

            public bool IsValid(PointerEventData data)
            {
                switch (_pointerId)
                {
                    case PointerId.Any:
                        return data != null;
                    default:
                        return (data?.pointerId ?? int.MinValue) == (int)_pointerId;
                }
            }

            public bool IsValid(CursorInputLogic logic)
            {
                return false;
            }
        }

        [System.Serializable]
        public class CursorInputLogicMode : IPointerFilterTest
        {
            [SerializeField]
            private string _cursorId;

            public string Id
            {
                get => _cursorId;
                set => _cursorId = value;
            }

            public bool IsValid(PointerEventData data)
            {
                return false;
            }

            public bool IsValid(CursorInputLogic cursor)
            {
                return string.IsNullOrEmpty(_cursorId) || string.Equals(cursor?.Id, _cursorId);
            }
        }

        #endregion

    }
}
