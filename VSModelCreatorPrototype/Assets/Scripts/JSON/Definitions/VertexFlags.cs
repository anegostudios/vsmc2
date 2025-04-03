/// <summary>
/// On the graphics card we have only one reflective bit, but we can store the mode in the wind data bits
/// </summary>
public enum EnumReflectiveMode
{
    /// <summary>
    /// Not reflective
    /// </summary>
    None = 0,
    /// <summary>
    /// Sun-Position independent reflectivity
    /// </summary>
    Weak = 1,
    /// <summary>
    /// Sun-Position dependent weak reflectivity
    /// </summary>
    Medium = 2,
    /// <summary>
    /// Sun-Position dependent weak reflectivity
    /// </summary>
    Strong = 3,
    /// <summary>
    /// Many small sparkles
    /// </summary>
    Sparkly = 4,

    Mild = 5,

}

public enum EnumWindBitMode
{
    /// <summary>
    /// Not affected by wind
    /// </summary>
    NoWind = 0,
    /// <summary>
    /// Slightly affected by wind. Wiggle + Height bend based on ground distance.
    /// </summary>
    WeakWind = 1,
    /// <summary>
    /// Normally affected by wind. Wiggle + Height bend based on ground distance.
    /// </summary>
    NormalWind = 2,
    /// <summary>
    /// Same as normal wind, but with some special behavior for leaves. Wiggle + Height bend based on ground distance.
    /// </summary>
    Leaves = 3,
    /// <summary>
    /// Same as normal wind, but no wiggle. Weak height bend based on ground distance.
    /// </summary>
    Bend = 4,
    /// <summary>
    /// Bend behavior for tall plants
    /// </summary>
    TallBend = 5,
    /// <summary>
    /// Vertical wiggle
    /// </summary>
    Water = 6,
    ExtraWeakWind = 7,
    Fruit = 8,
    WeakWindNoBend = 9,
    WeakWindInverseBend = 10,
    WaterPlant = 11
}
