using System;
using System.Linq;
using System.Reflection;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    /// <summary>
    /// A trigger that can be observed by other triggers for activation. 
    /// </summary>
    public interface IObservableTrigger
    {
        /// <summary>
        /// Return any SPEvents that are observable. 
        /// This will reflectively return and BaseSPEvent that is a accessible as a public property by default unless otherwise implemented explicitly.
        /// </summary>
        /// <returns></returns>
        BaseSPEvent[] GetEvents() => this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(o => o.CanRead && typeof(BaseSPEvent).IsAssignableFrom(o.PropertyType)).Select(o => o.GetMethod.Invoke(this, ArrayUtil.Empty<object>()) as BaseSPEvent).Where(o => o != null).ToArray();
    }

    /// <summary>
    /// A trigger that can be observed by other triggers. Of which 2 of the events that may trigger are an Enter and Exit state. 
    /// Think like a collider enter/exit, or a mouse enter/exit, or other event where there is a start and end.
    /// </summary>
    public interface IOccupiedTrigger : IObservableTrigger
    {

        BaseSPEvent EnterEvent { get; }
        BaseSPEvent ExitEvent { get; }
        bool IsOccupied { get; }

    }

}
