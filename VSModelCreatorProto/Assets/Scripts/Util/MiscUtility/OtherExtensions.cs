using UnityEngine;

public static class OtherExtensions
{

    /// <summary>
    /// Checks if two Vector2s are nearly equal. 
    /// Used for 'close enough' pixel calculations, e.g. is the mouse position close enough to a certain area.
    /// </summary>
    public static bool IsNearlyEqual(this Vector2 pos1, Vector2 pos2, float range = 2)
    {
        return (pos1.x >= pos2.x - range && pos1.x <= pos2.x + range && pos1.y >= pos2.y - range && pos1.y <= pos2.y + range);
    }

}
