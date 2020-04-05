using UnityEngine;

namespace com.spacepuppy
{

    public enum VariantType : sbyte
    {
        Object = -1,
        Null = 0,
        String = 1,
        Boolean = 2,
        Integer = 3,
        Float = 4,
        Double = 5,
        Vector2 = 6,
        Vector3 = 7,
        Vector4 = 8,
        Quaternion = 9,
        Color = 10,
        DateTime = 11,
        GameObject = 12,
        Component = 13,
        LayerMask = 14,
        Rect = 15,
        Numeric = 16
    }

    public enum QuitState
    {
        None,
        BeforeQuit,
        Quit
    }

    public enum EnableMode
    {
        Enable = 0,
        Disable = 1,
        Toggle = 2
    }

    /// <summary>
    /// Search parameter type
    /// </summary>
    public enum SearchBy
    {
        Nothing = 0,
        Tag = 1,
        Name = 2,
        Type = 3
    }

    public enum AudioInterruptMode
    {
        StopIfPlaying = 0,
        DoNotPlayIfPlaying = 1,
        PlayOverExisting = 2
    }

}