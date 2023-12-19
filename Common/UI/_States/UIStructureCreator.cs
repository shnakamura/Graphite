using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    
    public bool Selecting { get; private set; }

    public bool HasSelectedFirstPoint { get; private set; }
    public bool HasSelectedLastPoint { get; private set; }

    public UIStructureCreator() {
        HAlign = 0.5f;
        VAlign = 2f;

        Width.Set(32f, 0f);
        Height.Set(32f, 0f);

        SetPadding(4f);
    }

    public override void OnInitialize() {
        var panel = new UIPanel {
            Width = StyleDimension.FromPixels(32f),
            Height = StyleDimension.FromPixels(32f)
        };

        panel.OnLeftClick += PanelOnLeftClick;

        Append(panel);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        VAlign = MathHelper.Lerp(VAlign, 1f, 0.1f);

        var player = Main.LocalPlayer;

        var justRightClicked = !player.mouseInterface && PlayerInput.MouseInfo.RightButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.RightButton == ButtonState.Released;

        if (justRightClicked) {
            ClearSelection();
            return;
        }

        var justLeftClicked = !player.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released;

        if (!Selecting || !justLeftClicked) {
            return;
        }

        if (!HasSelectedFirstPoint) {
            FirstPoint = (Main.MouseWorld / 16f).Floor();
            HasSelectedFirstPoint = true;

            player.mouseInterface = true;
            return;
        }

        if (!HasSelectedLastPoint) {
            LastPoint = (Main.MouseWorld / 16f).Floor();
            HasSelectedLastPoint = true;

            player.mouseInterface = true;
        }
    }

    public override void Draw(SpriteBatch spriteBatch) {
        base.Draw(spriteBatch);

        if (!Selecting || !HasSelectedFirstPoint) {
            return;
        }

        var screenFirstPoint = FirstPoint * 16f - Main.screenPosition;
        var screenLastPoint = (HasSelectedLastPoint ? LastPoint : (Main.MouseWorld / 16f).Floor()) * 16f - Main.screenPosition;

        var rectangle = new Rectangle((int)MathF.Min(screenFirstPoint.X, screenLastPoint.X),
            (int)MathF.Min(screenFirstPoint.Y, screenLastPoint.Y),
            (int)MathF.Abs(screenFirstPoint.X - screenLastPoint.X),
            (int)MathF.Abs(screenFirstPoint.Y - screenLastPoint.Y));

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.White * (0.1f + MathF.Abs(MathF.Sin(Main.GameUpdateCount * 0.05f)) * 0.1f));

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

    private void ClearSelection() {
        FirstPoint = Vector2.Zero;
        LastPoint = Vector2.Zero;

        HasSelectedLastPoint = false;
        HasSelectedFirstPoint = false;
    }

    private void PanelOnLeftClick(UIMouseEvent evt, UIElement listeningelement) {
        SoundEngine.PlaySound(in SoundID.MenuTick);

        ClearSelection();

        Selecting = !Selecting;
    }
}
