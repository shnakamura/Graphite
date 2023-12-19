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
    public Vector2 FirstPoint;

    /// <summary>
    ///     Represents the last point selected for the structure area.
    /// </summary>
    public Vector2 LastPoint;

    private Player LocalPlayer => Main.LocalPlayer;

    /// <summary>
    ///     Indicates whether the structure area is currently being selected or not.
    /// </summary>
    public bool IsSelectingArea { get; private set; }

    /// <summary>
    ///     Indicates whether the structure area has already been selected or not.
    /// </summary>
    public bool HasSelectedArea { get; private set; }

    private UIImageButton saveButtonUI;
    private UIImageButton closeButtonUI;
    private UIImageButton retryButtonUI;
    
    public override void OnActivate() {
        base.OnActivate();

        ClearSelection();
    }

    public override void OnDeactivate() {
        base.OnDeactivate();

        ClearSelection();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        HandleText();
        HandleEscaping();
        HandleSelection();
        HandleButtons();
    }

    public override void Draw(SpriteBatch spriteBatch) {
        base.Draw(spriteBatch);

        var borderColor = Color.Black * 0.75f;

        if (!HasSelectedArea && !IsSelectingArea) {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), borderColor);
            return;
        }

        var screenFirstPoint = FirstPoint * 16f - Main.screenPosition;
        var screenLastPoint = LastPoint * 16f - Main.screenPosition;

        var rectangle = new Rectangle((int)MathF.Min(screenFirstPoint.X, screenLastPoint.X),
            (int)MathF.Min(screenFirstPoint.Y, screenLastPoint.Y),
            (int)MathF.Abs(screenFirstPoint.X - screenLastPoint.X),
            (int)MathF.Abs(screenFirstPoint.Y - screenLastPoint.Y));

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
            FirstPoint = Main.MouseWorld.SnapToTileCoordinates();

            IsSelectingArea = true;
        }

        if (justLeftReleased) {
            IsSelectingArea = false;
            HasSelectedArea = true;
        }

        if (!IsSelectingArea) {
            return;
        }

        LastPoint = Main.MouseWorld.SnapToTileCoordinates();
    }

    private void HandleButtons() {
        if (!HasChild(saveButtonUI)) {
            saveButtonUI = new UIImageButton(Nightshade.Instance.Assets.Request<Texture2D>("Assets/Textures/UI/SaveButton"));
            
            saveButtonUI.Top.Set(16f, 1f);
            saveButtonUI.Left.Set(0f, 1f);
            
            Append(saveButtonUI);
        }
    }

    private void ClearSelection() {
        FirstPoint = Vector2.Zero;
        LastPoint = Vector2.Zero;

        IsSelectingArea = false;
        HasSelectedArea = false;
    }
}
