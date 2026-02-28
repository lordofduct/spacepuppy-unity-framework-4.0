using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace com.spacepuppy.SPInput
{

    public class NullInputDevice : IInputDevice
    {

        public static readonly NullInputDevice Null = new();

        public bool Active
        {
            get => false;
            set { } //do nothing
        }

        public string Id => "NULL";

        public float Precedence
        {
            get => 0f;
            set { } //do nothing
        }

        public bool Contains(string id) => false;

        public void FixedUpdate() { } //do nothing

        public float GetAxleState(string id) => default;

        public ButtonState GetButtonState(string id, bool consume = false) => default;

        public Vector2 GetCursorState(string id) => default;

        public Vector2 GetDualAxleState(string id) => default;

        public IInputSignature GetSignature(string id) => default;

        public IEnumerable<IInputSignature> GetSignatures() => Enumerable.Empty<IInputSignature>();

        public void Reset() { } //do nothing

        public void Update() { } //do nothing
    }

}
