using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nightshade.Utilities;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;

namespace Nightshade.Common;

public sealed class UIScreenshotStructure : UIState
{
    private UIImageButton[] resizePins = new UIImageButton[4];

    public Vector2 Start;
    public Vector2 End;

    private Player LocalPlayer => Main.LocalPlayer;

    public bool IsSelectingArea { get; private set; }
    public bool HasSelectedArea { get; private set; }

    public bool IsResizingTopLeft { get; private set; }
    public bool IsResizingTopRight { get; private set; }
    public bool IsResizingBottomLeft { get; private set; }
    public bool IsResizingBottomRight { get; private set; }

    public bool IsResizingAny => IsResizingTopLeft || IsResizingTopRight || IsResizingBottomRight || IsResizingBottomLeft;

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
        HandleResizing();
    }

    public override void Draw(SpriteBatch spriteBatch) {
        var borderColor = Color.Black * 0.75f;

        if (!HasSelectedArea && !IsSelectingArea) {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), borderColor);
            return;
        }

        var screenFirstPoint = Start * 16f - Main.screenPosition;
        var screenLastPoint = End * 16f - Main.screenPosition;

        var rectangle = GetDimensions().ToRectangle();

        // Top border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, rectangle.Top), borderColor);

        // Bottom border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, rectangle.Bottom, Main.screenWidth, (int)MathF.Abs(Main.screenHeight - rectangle.Bottom)), borderColor);

        // Left border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, rectangle.Top, rectangle.Left, rectangle.Height), borderColor);

        // Right border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.Right, rectangle.Top, (int)MathF.Abs(Main.screenWidth - rectangle.Left), rectangle.Height), borderColor);

        if (Lighting.NotRetro) { }
        else {
            var outlineWidth = 2;
            var outlineColor = Color.White;

            // Top outline.
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X, rectangle.Y - outlineWidth, rectangle.Width, outlineWidth), outlineColor);

            // Bottom outline.
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X, rectangle.Bottom, rectangle.Width, outlineWidth), outlineColor);

            // Left outline.
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.X - outlineWidth, rectangle.Y, outlineWidth, rectangle.Height), outlineColor);

            // Right outline.
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rectangle.Right, rectangle.Y, outlineWidth, rectangle.Height), outlineColor);
        }

        base.Draw(spriteBatch);
    }

    private void HandleText() {
        if (HasSelectedArea || IsSelectingArea) {
            return;
        }

        Main.instance.MouseText(Language.GetTextValue($"Mods.{nameof(Nightshade)}.UI.Screenshot.Select"));
    }

    private void HandleEscaping() {
        var justRightClicked = !LocalPlayer.mouseInterface && PlayerInput.MouseInfo.RightButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.RightButton == ButtonState.Released;
        var justEscaped = Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape);

        if (!justRightClicked && !justEscaped) {
            return;
        }

        UIScreenshotStructureSystem.Disable();

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
            SetupResizePins();

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

    private void HandleResizing() {
        if (!HasSelectedArea) {
            return;
        }

        Array.Sort(resizePins,
            (first, last) => {
                var firstPosition = first.GetOuterDimensions().Position();
                var lastPosition = last.GetOuterDimensions().Position();

                return firstPosition.Y == lastPosition.Y ? firstPosition.X.CompareTo(lastPosition.X) : firstPosition.Y.CompareTo(lastPosition.Y);
            });
        
        if (!IsResizingAny) {
            return;
        }

        var snappedMousePosition = Main.MouseWorld.SnapToTileCoordinates();

        var topLeftPin = resizePins[0];
        var topRightPin = resizePins[1];
        var bottomLeftPin = resizePins[2];
        var bottomRightPin = resizePins[3];

        if (IsResizingTopLeft) {
            Start = snappedMousePosition;
        }
        else if (IsResizingTopRight) {
            Start.Y = snappedMousePosition.Y;
            End.X = snappedMousePosition.X;
        }
        else if (IsResizingBottomLeft) {
            Start.X = snappedMousePosition.X;
            End.Y = snappedMousePosition.Y;
        }
        else if (IsResizingBottomRight) {
            End = snappedMousePosition;
        }

        Recalculate();
    }

    private void StartResizing(UIMouseEvent evt, UIElement element) {
        var topLeftPin = resizePins[0];
        var topRightPin = resizePins[1];
        var bottomLeftPin = resizePins[2];
        var bottomRightPin = resizePins[3];

        if (element == topLeftPin) {
            IsResizingTopLeft = true;
            IsResizingTopRight = false;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = false;
        }
        else if (element == topRightPin) {
            IsResizingTopLeft = false;
            IsResizingTopRight = true;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = false;
        }
        else if (element == bottomLeftPin) {
            IsResizingTopLeft = false;
            IsResizingTopRight = false;
            IsResizingBottomLeft = true;
            IsResizingBottomRight = false;
        }
        else if (element == bottomRightPin) {
            IsResizingTopLeft = false;
            IsResizingTopRight = false;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = true;
        }
    }

    private void StopResizing(UIMouseEvent evt, UIElement element) {
        var topLeftPin = resizePins[0];
        var bottomRightPin = resizePins[3];

        Start = (topLeftPin.GetDimensions().Center() + Main.screenPosition).SnapToTileCoordinates();
        End = (bottomRightPin.GetDimensions().Center() + Main.screenPosition).SnapToTileCoordinates();

        IsResizingTopLeft = false;
        IsResizingTopRight = false;
        IsResizingBottomLeft = false;
        IsResizingBottomRight = false;
    }

    private void SetupResizePins() {
        var texture = Nightshade.Instance.Assets.Request<Texture2D>("Assets/Textures/UI/ResizePin");

        resizePins = new UIImageButton[4];

        if (!HasChild(resizePins[0])) {
            resizePins[0] = new UIImageButton(texture);
            resizePins[0].Top.Set(-resizePins[0].Height.Pixels / 2f, 0f);
            resizePins[0].Left.Set(-resizePins[0].Width.Pixels / 2f, 0f);

            resizePins[0].OnLeftMouseDown += StartResizing;
            resizePins[0].OnLeftMouseUp += StopResizing;

            Append(resizePins[0]);
        }

        if (!HasChild(resizePins[1])) {
            resizePins[1] = new UIImageButton(texture);
            resizePins[1].Top.Set(-resizePins[1].Height.Pixels / 2f, 0f);
            resizePins[1].Left.Set(-resizePins[1].Width.Pixels / 2f, 1f);

            resizePins[1].OnLeftMouseDown += StartResizing;
            resizePins[1].OnLeftMouseUp += StopResizing;

            Append(resizePins[1]);
        }

        if (!HasChild(resizePins[2])) {
            resizePins[2] = new UIImageButton(texture);
            resizePins[2].Top.Set(-resizePins[2].Height.Pixels / 2f, 1f);
            resizePins[2].Left.Set(-resizePins[2].Width.Pixels / 2f, 0f);

            resizePins[2].OnLeftMouseDown += StartResizing;
            resizePins[2].OnLeftMouseUp += StopResizing;

            Append(resizePins[2]);
        }

        if (!HasChild(resizePins[3])) {
            resizePins[3] = new UIImageButton(texture);
            resizePins[3].Top.Set(-resizePins[3].Height.Pixels / 2f, 1f);
            resizePins[3].Left.Set(-resizePins[3].Width.Pixels / 2f, 1f);

            resizePins[3].OnLeftMouseDown += StartResizing;
            resizePins[3].OnLeftMouseUp += StopResizing;

            Append(resizePins[3]);
        }
    }

    private void ClearSelection() {
        Start = Vector2.Zero;
        End = Vector2.Zero;

        IsSelectingArea = false;
        HasSelectedArea = false;
    }
}
