namespace VSMC
{
    /// <summary>
    /// A VSMC2 edit mode. Only one edit mode can be active at any one time.
    /// </summary>
    public enum VSEditMode
    {
        None = -1,
        View = 0,
        Model = 1,
        Texture = 2,
        Animation = 3
    }
}