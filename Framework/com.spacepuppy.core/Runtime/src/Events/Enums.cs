using System;

namespace com.spacepuppy.Events
{

    public enum EnableMode
    {
        Enable = 0,
        Disable = 1,
        Toggle = 2
    }

    public enum DisableMode
    {
        None,
        DisableComponent,
        DisableGameObject
    }

    [System.Flags()]
    public enum ActivateEvent
    {
        None = 0,
        OnStart = 1,
        OnEnable = 2,
        OnStartOrEnable = 3,
        Awake = 4
    }

    public enum TriggerActivationType
    {
        TriggerAllOnTarget = 0,
        TriggerSelectedTarget = 1,
        SendMessage = 2,
        CallMethodOnSelectedTarget = 3,
        EnableTarget = 4,
        DestroyTarget = 5
    }

}
