namespace com.spacepuppy.Render
{

    public enum MaterialPropertyValueType
    {
        Float = 0,
        Color = 1,
        Vector = 2,
        Texture = 3
    }

    public enum MaterialPropertyValueTypeMember
    {
        None = 0,
        X,
        Y,
        Z,
        W
    }

    public enum MaterialSourceUniquessModes
    {
        UseSharedMaterial = 0,
        MakeUniqueOnStart = 1,
        MakeUniqueOnAccess = 2,
    }

}
