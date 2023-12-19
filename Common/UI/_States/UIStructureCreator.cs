using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nightshade.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace Nightshade.Common.UI;

public sealed class UIStructureCreator : UIState
{
    public Vector2 FirstPoint;
    public Vector2 LastPoint;

    public bool SelectingArea { get; private set; }

    public bool HasSelectedFirstPoint { get; private set; }
    public bool HasSelectedLastPoint { get; private set; }

    public UIStructureCreator() : base() {
        HAlign = 0.5f;
        VAlign = 2f;

        Width.Set(64f, 0f);
        Height.Set(64f, 0f);
    }

    public override void OnInitialize() {
        base.OnInitialize();
        
        var panel = new UIPanel {
            Width = StyleDimension.FromPercent(1f),
            Height = StyleDimension.FromPercent(1f)
        };

        panel.OnLeftClick += PanelOnLeftClick;

        Append(panel);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        VAlign = MathHelper.Lerp(VAlign, 1f, 0.1f);

        HandleSelection();
        HandleResizing();
    }

    public override void Draw(SpriteBatch spriteBatch) {
        base.Draw(spriteBatch);

        if (!SelectingArea || !HasSelectedFirstPoint) {
            return;
        }

        var screenFirstPoint = FirstPoint * 16f - Main.screenPosition;
        var screenLastPoint = (HasSelectedLastPoint ? LastPoint : Main.MouseWorld.SnapToTileCoordinates()) * 16f - Main.screenPosition;

        var rectangle = new Rectangle((int)MathF.Min(screenFirstPoint.X, screenLastPoint.X),
            (int)MathF.Min(screenFirstPoint.Y, screenLastPoint.Y),
            (int)MathF.Abs(screenFirstPoint.X - screenLastPoint.X),
            (int)MathF.Abs(screenFirstPoint.Y - screenLastPoint.Y));

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.White * (0.1f + MathF.Abs(MathF.Sin(Main.GameUpdateCount * 0.05f)) * 0.025f));

        var borderColor = Color.Black * 0.75f;

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

        var pinSize = TextureAssets.Heart.Value.Size();
        var pinColor = Color.White;

        // Top left pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.X, rectangle.Y) - pinSize / 2f, pinColor);

        // Top middle pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.X + rectangle.Width / 2f, rectangle.Y) - pinSize / 2f, pinColor);

        // Top right pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.Right, rectangle.Y) - pinSize / 2f, pinColor);

        // Middle left pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.X, rectangle.Y + rectangle.Height / 2f) - pinSize / 2f, pinColor);

        // Middle right pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.Right, rectangle.Y + rectangle.Height / 2f) - pinSize / 2f, pinColor);

        // Bottom left pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.X, rectangle.Bottom) - pinSize / 2f, pinColor);

        // Bottom middle pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.X + rectangle.Width / 2f, rectangle.Bottom) - pinSize / 2f, pinColor);

        // Bottom right pin.
        spriteBatch.Draw(TextureAssets.Heart.Value, new Vector2(rectangle.Right, rectangle.Bottom) - pinSize / 2f, pinColor);
    }

    private void HandleSelection() {
        var player = Main.LocalPlayer;

        var justRightClicked = !player.mouseInterface && PlayerInput.MouseInfo.RightButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.RightButton == ButtonState.Released;

        if (justRightClicked) {
            ClearSelection();
            return;
        }

        var justLeftClicked = !player.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released;

        if (!SelectingArea || !justLeftClicked) {
            return;
        }

        if (!HasSelectedFirstPoint) {
            FirstPoint = Main.MouseWorld.SnapToTileCoordinates();
            HasSelectedFirstPoint = true;

            player.mouseInterface = true;

            SoundEngine.PlaySound(in SoundID.MenuTick);
            return;
        }

        if (!HasSelectedLastPoint) {
            LastPoint = Main.MouseWorld.SnapToTileCoordinates();
            HasSelectedLastPoint = true;

            player.mouseInterface = true;

            SoundEngine.PlaySound(in SoundID.Unlock);
        }
    }

    private void HandleResizing() {
        if (!SelectingArea || !HasSelectedFirstPoint || !HasSelectedLastPoint) { }
    }

    private void ClearSelection() {
        FirstPoint = Vector2.Zero;
        LastPoint = Vector2.Zero;

        HasSelectedLastPoint = false;
        HasSelectedFirstPoint = false;
    }

    private void PanelOnLeftClick(UIMouseEvent evt, UIElement listeningelement) {
        SoundEngine.PlaySound(in SoundID.MenuTick);

        ClearSelection();

        SelectingArea = !SelectingArea;
    }
}
