/// <summary>
/// The various render passes available for rendering blocks
/// 
/// NOTE, IN VSMC2 THESE VALUES ARE INCREASED BY 1 TO WORK WITH THE DROPDOWN.
/// I.E. DEFAULT = -1 IN GAME.
/// </summary>
public enum EnumChunkRenderPass
{
    /// <summary>
    /// Default option for VSMC2.
    /// </summary>
    Default = 0,
    /// <summary>
    /// Backfaced culled, no alpha testing, alpha discard
    /// </summary>
    Opaque = 1,
    /// <summary>
    /// Backfaced not culled, no alpha blended but alpha discard
    /// </summary>
    OpaqueNoCull = 2,
    /// <summary>
    /// Backfaced not culled, alpha blended and alpha discard
    /// </summary>
    BlendNoCull = 3,
    /// <summary>
    /// Uses a special rendering system called Weighted Blended Order Independent Transparency for half transparent blocks
    /// </summary>
    Transparent = 4,
    /// <summary>
    /// Used for animated liquids
    /// </summary>
    Liquid = 5,
    /// <summary>
    /// Special render pass for top soil only in order to have climated tinted grass half transparently overlaid over an opaque block
    /// </summary>
    TopSoil = 6,
    /// <summary>
    /// Special render pass for meta blocks
    /// </summary>
    Meta = 7,
    /// <summary>
    /// Uses the depth buffer from the OIT pass to prevent water plants showing in sailboats 
    /// </summary>
    OpaqueWaterPlant = 8,
    /// <summary>
    /// Decor overlays need to come last because they are on top of other things; they are essentially Opaque but done last
    /// </summary>
    Decor = 9
}