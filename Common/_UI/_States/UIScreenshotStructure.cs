using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Nightshade.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Nightshade.Common;

public sealed class UIScreenshotStructure : UIState
{
    public static readonly Asset<Texture2D> ResizePinTexture = ModContent.Request<Texture2D>($"{nameof(Nightshade)}/Assets/Textures/UI/ResizePin", AssetRequestMode.ImmediateLoad);

    public readonly UIImageButton[] ResizePins = new[] {
        new UIImageButton(ResizePinTexture) {
            Top = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Height() / 2f, 0f),
            Left = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Width() / 2f, 0f),
        },
        new UIImageButton(ResizePinTexture) {
            Top = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Height() / 2f, 0f),
            Left = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Width() / 2f, 1f),
        },
        new UIImageButton(ResizePinTexture) {
            Top = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Height() / 2f, 1f),
            Left = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Width() / 2f, 0f),
        },
        new UIImageButton(ResizePinTexture) {
            Top = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Height() / 2f, 1f),
            Left = StyleDimension.FromPixelsAndPercent(-ResizePinTexture.Width() / 2f, 1f),
        }
    };

    private nativefiledialog.nfdresult_t nfdResult;
    private string nfdPath;
    
    public Vector2 Start;
    public Vector2 End;

    private Player LocalPlayer => Main.LocalPlayer;

    public ref UIImageButton TopLeftPin => ref ResizePins[0];
    public ref UIImageButton TopRightPin => ref ResizePins[1];
    public ref UIImageButton BottomLeftPin => ref ResizePins[2];
    public ref UIImageButton BottomRightPin => ref ResizePins[3];

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
        var screenStart = Start * 16f - Main.screenPosition;
        var screenEnd = End * 16f - Main.screenPosition;

        var start = (int)MathF.Min(screenStart.X, screenEnd.X);
        var end = (int)MathF.Min(screenStart.Y, screenEnd.Y);

        var width = (int)MathF.Abs(screenStart.X - screenEnd.X);
        var height = (int)MathF.Abs(screenStart.Y - screenEnd.Y);

        Top.Set(end, 0f);
        Left.Set(start, 0f);

        Width.Set(width, 0f);
        Height.Set(height, 0f);
        
        base.Recalculate();
    }

    public override void RecalculateChildren() {
        Array.Sort(ResizePins,
            (first, last) => {
                var firstPosition = first.GetOuterDimensions().Position();
                var lastPosition = last.GetOuterDimensions().Position();

                return firstPosition.Y == lastPosition.Y ? firstPosition.X.CompareTo(lastPosition.X) : firstPosition.Y.CompareTo(lastPosition.Y);
            });
        
        base.RecalculateChildren();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (nfdResult == nativefiledialog.nfdresult_t.NFD_OKAY) {
            StructureSerializer.SerializeStructure(nfdPath);
            UIScreenshotStructureSystem.Disable();
            return;
        }

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
            Start = Main.MouseWorld.SnapToTileCoordinates();
            IsSelectingArea = true;
            
            SetupResizePins();
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
        if (!IsResizingAny) {
            return;
        }

        var snappedMousePosition = Main.MouseWorld.SnapToTileCoordinates();

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
        if (element == TopLeftPin) {
            IsResizingTopLeft = true;
            IsResizingTopRight = false;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = false;
        }
        else if (element == TopRightPin) {
            IsResizingTopLeft = false;
            IsResizingTopRight = true;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = false;
        }
        else if (element == BottomLeftPin) {
            IsResizingTopLeft = false;
            IsResizingTopRight = false;
            IsResizingBottomLeft = true;
            IsResizingBottomRight = false;
        }
        else if (element == BottomRightPin) {
            IsResizingTopLeft = false;
            IsResizingTopRight = false;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = true;
        }
    }

    private void StopResizing(UIMouseEvent evt, UIElement element) {
        Start = (TopLeftPin.GetDimensions().Center() + Main.screenPosition).SnapToTileCoordinates();
        End = (BottomRightPin.GetDimensions().Center() + Main.screenPosition).SnapToTileCoordinates();

        IsResizingTopLeft = false;
        IsResizingTopRight = false;
        IsResizingBottomLeft = false;
        IsResizingBottomRight = false;
    }

    private void SetupResizePins() {
        var list = new UIList() {
            Top = StyleDimension.FromPercent(0f),
            Left = StyleDimension.FromPixelsAndPercent(32f, 1f), 
            Width = StyleDimension.FromPixels(32f),
            Height = StyleDimension.FromPercent(1f)
        };

        list.ListPadding = 16;

        var close = new UIImageButton(Nightshade.Instance.Assets.Request<Texture2D>("Assets/Textures/UI/CloseButton"));
        close.OnLeftClick += (evt, element) => UIScreenshotStructureSystem.Disable();

        var save = new UIImageButton(Nightshade.Instance.Assets.Request<Texture2D>("Assets/Textures/UI/SaveButton"));
        save.OnLeftClick += (evt, element) => nfdResult = nativefiledialog.NFD_SaveDialog("ngtsh", Main.SavePath, out nfdPath);
        
        list.Add(close);
        list.Add(save);

        Append(list);
        
        if (!HasChild(TopLeftPin)) {
            TopLeftPin.OnLeftMouseDown += StartResizing;
            TopLeftPin.OnLeftMouseUp += StopResizing;

            Append(TopLeftPin);
        }

        if (!HasChild(TopRightPin)) {
            TopRightPin.OnLeftMouseDown += StartResizing;
            TopRightPin.OnLeftMouseUp += StopResizing;

            Append(TopRightPin);
        }

        if (!HasChild(BottomLeftPin)) {
            BottomLeftPin.OnLeftMouseDown += StartResizing;
            BottomLeftPin.OnLeftMouseUp += StopResizing;

            Append(BottomLeftPin);
        }

        if (!HasChild(BottomRightPin)) {
            BottomRightPin.OnLeftMouseDown += StartResizing;
            BottomRightPin.OnLeftMouseUp += StopResizing;

            Append(BottomRightPin);
        }
    }

    private void ClearSelection() {
        Start = Vector2.Zero;
        End = Vector2.Zero;

        IsSelectingArea = false;
        HasSelectedArea = false;
        
        IsResizingTopLeft = false;
        IsResizingTopRight = false;
        IsResizingBottomLeft = false;
        IsResizingBottomRight = false;
    }
}
