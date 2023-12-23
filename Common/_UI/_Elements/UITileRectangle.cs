using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;

namespace Nightshade.Common;

public class UITileRectangle : UIElement
{
    /// <summary>
    ///     Represents the top left start of the rectangle in tile coordinates.
    /// </summary>
    public Vector2 TileStart;
    
    /// <summary>
    ///     Represents the bottom right end of the rectangle in tile coordinates.
    /// </summary>
    public Vector2 TileEnd;

    /// <summary>
    ///     Represents the top left start of the rectangle in screen coordinates, automatically calculated from the tile coordinates.
    /// </summary>
    public Vector2 ScreenStart => TileStart * 16f - Main.screenPosition;
    
    /// <summary>
    ///     Represents the bottom right end of the rectangle in screen coordinates, automatically calculated from the tile coordinates.
    /// </summary>
    public Vector2 ScreenEnd => TileEnd * 16f - Main.screenPosition;
    
    public override void Recalculate() {
        Top.Set(MathF.Min(ScreenStart.Y, ScreenEnd.Y), 0f);
        Left.Set(MathF.Min(ScreenStart.X, ScreenEnd.X), 0f);

        Width.Set(MathF.Abs(ScreenStart.X - ScreenEnd.X), 0f);
        Height.Set(MathF.Abs(ScreenStart.Y - ScreenEnd.Y), 0f);
        
        base.Recalculate();
    }
}
