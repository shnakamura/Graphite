using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nightshade.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace Nightshade.Common.UI;

/// <summary>
///     Provides a user interface state that allows the user to select world tiles for saving a structure file.
/// </summary>
public sealed class UIStructureCreator : UIState
{
    /// <summary>
    ///     Represents the first point selected for the structure area.
    /// </summary>
    public Vector2 Start;

    /// <summary>
    ///     Represents the last point selected for the structure area.
    /// </summary>
    public Vector2 End;

    private Player LocalPlayer => Main.LocalPlayer;

    /// <summary>
    ///     Indicates whether the structure area is currently being selected or not.
    /// </summary>
    public bool IsSelectingArea { get; private set; }

    /// <summary>
    ///     Indicates whether the structure area has already been selected or not.
    /// </summary>
    public bool HasSelectedArea { get; private set; }
    
    public override void OnActivate() {
        base.OnActivate();

        ClearSelection();
    }

    public override void OnDeactivate() {
        base.OnDeactivate();

        ClearSelection();
    }
    
    public override void Recalculate() {
        var screenFirstPoint = Start * 16f - Main.screenPosition;
        var screenLastPoint = End * 16f - Main.screenPosition;

        var start = (int)MathF.Min(screenFirstPoint.X, screenLastPoint.X);
        var end = (int)MathF.Min(screenFirstPoint.Y, screenLastPoint.Y);

        var width = (int)MathF.Abs(screenFirstPoint.X - screenLastPoint.X);
        var height = (int)MathF.Abs(screenFirstPoint.Y - screenLastPoint.Y);
        
        Top.Set(end, 0f);
        Left.Set(start, 0f);
        
        Width.Set(width, 0f);
        Height.Set(height, 0f);
        
        base.Recalculate();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        HandleText();
        HandleEscaping();
        HandleSelection();
    }

    public override void Draw(SpriteBatch spriteBatch) {
        base.Draw(spriteBatch);

        var borderColor = Color.Black * 0.75f;

        if (!HasSelectedArea && !IsSelectingArea) {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), borderColor);
            return;
        }

        var screenFirstPoint = Start * 16f - Main.screenPosition;
        var screenLastPoint = End * 16f - Main.screenPosition;

        var rectangle = GetDimensions().ToRectangle();

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.White * (0.1f + MathF.Abs(MathF.Sin(Main.GameUpdateCount * 0.05f)) * 0.025f));

        // Top border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, rectangle.Top), borderColor);

        // Bottom border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, rectangle.Bottom, Main.screenWidth, (int)MathF.Abs(Main.screenHeight - rectangle.Bottom)), borderColor);

        // Left border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, rectangle.Top, rectangle.Left, rectangle.Height), borderColor);

        // Right border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.Right, rectangle.Top, (int)MathF.Abs(Main.screenWidth - rectangle.Left), rectangle.Height), borderColor);

        var outlineWidth = 2;
        var outlineColor = Color.Black;

        // Top outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X, rectangle.Y - outlineWidth, rectangle.Width, outlineWidth), outlineColor);

        // Bottom outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X, rectangle.Bottom, rectangle.Width, outlineWidth), outlineColor);

        // Left outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X - outlineWidth, rectangle.Y, outlineWidth, rectangle.Height), outlineColor);

        // Right outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.Right, rectangle.Y, outlineWidth, rectangle.Height), outlineColor);
    }
    
    private void HandleText() {
        if (HasSelectedArea || IsSelectingArea) {
            return;
        }

        Main.instance.MouseText("Select area");
    }
    
    private void HandleEscaping() {
        var justRightClicked = !LocalPlayer.mouseInterface && PlayerInput.MouseInfo.RightButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.RightButton == ButtonState.Released;
        var justEscaped = Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape);

        if (!justRightClicked && !justEscaped) {
            return;
        }

        UIStructureCreatorSystem.Disable();

        LocalPlayer.releaseInventory = false;
        LocalPlayer.mouseInterface = true;
    }

    private void HandleSelection() {
        if (HasSelectedArea) {
            return;
        }
        
        var justLeftClicked = !LocalPlayer.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released;
        var justLeftReleased = !LocalPlayer.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Released && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Pressed;

        if (justLeftClicked) {
            Start = Main.MouseWorld.SnapToTileCoordinates();

            IsSelectingArea = true;
        }

        if (justLeftReleased) {
            IsSelectingArea = false;
            HasSelectedArea = true;
        }

        if (!IsSelectingArea) {
            return;
        }

        End = Main.MouseWorld.SnapToTileCoordinates();
    }

    private void ClearSelection() {
        Start = Vector2.Zero;
        End = Vector2.Zero;

        IsSelectingArea = false;
        HasSelectedArea = false;
    }
}
