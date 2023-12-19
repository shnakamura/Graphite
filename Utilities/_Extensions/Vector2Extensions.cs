using Microsoft.Xna.Framework;
using Terraria;

namespace Nightshade.Utilities;

public static class Vector2Extensions
{
    public static Vector2 SnapToTileCoordinates(this Vector2 vector) {
        return (vector / 16f).Floor();
    }
}
