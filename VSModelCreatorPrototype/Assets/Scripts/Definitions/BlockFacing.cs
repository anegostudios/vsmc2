using Unity.VisualScripting;
using UnityEngine;

public class BlockFacing
{

    public const int NumberOfFaces = 6;
    public const int indexNORTH = 0;
    public const int indexEAST = 1;
    public const int indexSOUTH = 2;
    public const int indexWEST = 3;
    public const int indexUP = 4;
    public const int indexDOWN = 5;

    /// <summary>
    /// All horizontal blockfacing flags combined
    /// </summary>
    public static readonly byte HorizontalFlags = 1 | 2 | 4 | 8;

    /// <summary>
    /// All vertical blockfacing flags combined
    /// </summary>
    public static readonly byte VerticalFlags = 16 | 32;

    /// <summary>
    /// Faces towards negative Z
    /// </summary>
    public static readonly BlockFacing NORTH = new BlockFacing("north", 1, 0,EnumAxis.Z);
    /// <summary>
    /// Faces towards positive X
    /// </summary>
    public static readonly BlockFacing EAST = new BlockFacing("east", 2, 1, EnumAxis.X);
    /// <summary>
    /// Faces towards positive Z
    /// </summary>
    public static readonly BlockFacing SOUTH = new BlockFacing("south", 4, 2, EnumAxis.Z);
    /// <summary>
    /// Faces towards negative X
    /// </summary>
    public static readonly BlockFacing WEST = new BlockFacing("west", 8, 3, EnumAxis.X);

    /// <summary>
    /// Faces towards positive Y
    /// </summary>
    public static readonly BlockFacing UP = new BlockFacing("up", 16, 4, EnumAxis.Y);
    /// <summary>
    /// Faces towards negative Y
    /// </summary>
    public static readonly BlockFacing DOWN = new BlockFacing("down", 32, 5, EnumAxis.Y);

    /// <summary>
    /// All block faces in the order of N, E, S, W, U, D
    /// </summary>
    public static readonly BlockFacing[] ALLFACES = new BlockFacing[] { NORTH, EAST, SOUTH, WEST, UP, DOWN };

    public int index;
    public byte meshDataIndex;
    public byte flag;
    public string code;
    public EnumAxis axis;

    private BlockFacing(string code, byte flag, int index, EnumAxis axis)
    {
        this.index = index;
        this.meshDataIndex = (byte)(index + 1);
        //this.horizontalAngleIndex = horizontalAngleIndex;
        this.flag = flag;
        this.code = code;
        //this.oppositeIndex = oppositeIndex;
        //this.normali = facingVector;
        //this.normalf = new Vec3f(facingVector.X, facingVector.Y, facingVector.Z);
        //this.normald = new Vec3d((double)facingVector.X, (double)facingVector.Y, (double)facingVector.Z);
        //this.plane = plane;
        this.axis = axis;
    }

    /// <summary>
    /// Returns the face if code is 'n', 'e', 's', 'w', 'n', 'u' or 'd'. Otherwise null.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static BlockFacing FromFirstLetter(string code)
    {
        if (code.Length < 1) return null;
        char ch = char.ToLowerInvariant(code[0]);

        switch (ch)
        {
            case 'n': return NORTH;
            case 's': return SOUTH;
            case 'e': return EAST;
            case 'w': return WEST;
            case 'u': return UP;
            case 'd': return DOWN;
        }

        return null;
    }

}
