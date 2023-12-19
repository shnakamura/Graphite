using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;

namespace Nightshade.Common.UI;

/// <summary>
///     Provides a draggable user interface element.
/// </summary>
public class DraggableUIElement : UIElement
{
    private Vector2 dragStartPosition;

    /// <summary>
    ///     Indicates whether this element should be constrained to the parent's bounds or not.
    /// </summary>
    public readonly bool Constrained;

    /// <summary>
    ///     Indicates whether this element is currently being dragged or not.
    /// </summary>
    public bool Dragging { get; protected set; }

    public DraggableUIElement(bool constrained) {
        Constrained = constrained;
    }

    public override void LeftMouseDown(UIMouseEvent evt) {
        base.LeftMouseDown(evt);

        dragStartPosition = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);

        Dragging = true;
    }

    public override void LeftMouseUp(UIMouseEvent evt) {
        base.LeftMouseUp(evt);

        var endMousePosition = evt.MousePosition;

        Left.Set(endMousePosition.X - dragStartPosition.X, 0f);
        Top.Set(endMousePosition.Y - dragStartPosition.Y, 0f);

        Recalculate();

        Dragging = false;
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (ContainsPoint(Main.MouseScreen)) {
            Main.LocalPlayer.mouseInterface = true;
        }

        HandleDragging();

        if (!Constrained) {
            return;
        }

        HandleConstraint();
    }

    private void HandleDragging() {
        if (!Dragging) {
            return;
        }

        Left.Set(Main.mouseX - dragStartPosition.X, 0f);
        Top.Set(Main.mouseY - dragStartPosition.Y, 0f);

        Recalculate();
    }

    private void HandleConstraint() {
        var parentSpace = Parent.GetDimensions().ToRectangle();

        if (GetDimensions().ToRectangle().Intersects(parentSpace)) {
            return;
        }

        Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
        Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);

        Recalculate();
    }
}
