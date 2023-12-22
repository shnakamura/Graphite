using System;
using Microsoft.Xna.Framework;

namespace Nightshade.Common.Generation;

public readonly struct StructureAnchor
{
    public static readonly StructureAnchor Center = new((width, height) => new Point(width / 2, height / 2));

    public static readonly StructureAnchor TopLeft = new((_, _) => new Point(0, 0));

    public static readonly StructureAnchor TopCenter = new((width, _) => new Point(width / 2, 0));

    public static readonly StructureAnchor TopRight = new((width, _) => new Point(width, 0));

    public static readonly StructureAnchor BottomLeft = new((_, height) => new Point(0, height));

    public static readonly StructureAnchor BottomCenter = new((width, height) => new Point(width / 2, height));

    public static readonly StructureAnchor BottomRight = new((width, height) => new Point(width, height));

    public readonly Func<int, int, Point> Function;

    public StructureAnchor(Func<int, int, Point> function) {
        Function = function;
    }
}
