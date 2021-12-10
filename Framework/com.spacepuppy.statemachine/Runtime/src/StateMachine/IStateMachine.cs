using System.Collections.Generic;
using com.spacepuppy.Collections;

namespace com.spacepuppy.StateMachine
{

    public interface IStateMachine<T>
    {

        event StateChangedEventHandler<T> StateChanged;

        T Current { get; }
    }

    public interface IStateCollection<T> : IStateMachine<T>, IRadicalEnumerable<T>
    {

        int Count { get; }

        bool Contains(T state);


    }

    public interface IStateStack<T> : IStateCollection<T>
    {

        void PushState(T state);
        T PopState();
        void PopAllStates();

    }

    public interface IStateGroup<T> : IStateCollection<T>
    {

        T ChangeState(T state);

    }

}