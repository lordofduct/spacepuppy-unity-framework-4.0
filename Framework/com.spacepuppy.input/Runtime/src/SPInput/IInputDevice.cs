using UnityEngine;
using System.Collections.Generic;

namespace com.spacepuppy.SPInput
{

    public interface IInputDevice : IInputSignature
    {

        bool Active { get; set; }

        bool Contains(string id);

        IInputSignature GetSignature(string id);

        IEnumerable<IInputSignature> GetSignatures();

        ButtonState GetButtonState(string id, bool consume = false);
        
        float GetAxleState(string id);

        Vector2 GetDualAxleState(string id);

        Vector2 GetCursorState(string id);

        bool GetAnyInputActivated()
        {
            foreach (var sig in this.GetSignatures())
            {
                if (sig.GetInputIsActivated()) return true;
            }
            return false;
        }

        bool GetAnyButtonDown()
        {
            var e = this.GetSignatures().GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current is IButtonInputSignature bsig && bsig.CurrentState == ButtonState.Down) return true;
            }
            return false;
        }

    }

    public interface IInputSignatureCollection : ICollection<IInputSignature>
    {
        bool Contains(string id);
        IInputSignature GetSignature(string id);
        bool Remove(string id);
        void Sort();

        void Update(bool isFixed);
    }

}
