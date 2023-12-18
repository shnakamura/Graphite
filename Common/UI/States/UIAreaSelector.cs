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

namespace Nightshade.Common.UI.States;

public sealed class UIAreaSelector : UIState
{
    public bool Selecting { get; private set; }
    
    public Vector2 FirstPoint { get; private set; }
    public Vector2 LastPoint { get; private set; }
    
    public bool HasSelectedFirstPoint { get; private set; }
    public bool HasSelectedLastPoint { get; private set; }

    public UIAreaSelector() {
        HAlign = 0.5f;
        VAlign = 2f;
        
        Width.Set(32f, 0f);
        Height.Set(32f, 0f);

        SetPadding(4f);
    }
    
    public override void OnInitialize() {
        var panel = new UIPanel() {
            Width = StyleDimension.FromPixels(32f),
            Height = StyleDimension.FromPixels(32f)
        };
        
        panel.OnLeftClick += PanelOnLeftClick;
        
        Append(panel);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        
        VAlign = MathHelper.Lerp(VAlign, 1f, 0.1f);

        var justRightClicked = PlayerInput.MouseInfo.RightButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.RightButton == ButtonState.Released;

        if (justRightClicked) {
            ClearSelection();
            return;
        }
        
        var justLeftClicked = PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released;

        if (!justLeftClicked) {
            return;
        }

        if (!HasSelectedFirstPoint) {
            FirstPoint = (Main.MouseWorld / 16f).Floor();
            HasSelectedFirstPoint = true;
            return;
        }

        if (!HasSelectedLastPoint) {
            LastPoint = (Main.MouseWorld / 16f).Floor();
            HasSelectedLastPoint = true;
            return;
        }

        var area = new Rectangle((int)MathF.Min(FirstPoint.X, LastPoint.X),
            (int)MathF.Min(FirstPoint.Y, LastPoint.Y),
            (int)MathF.Abs(FirstPoint.X - LastPoint.X),
            (int)MathF.Abs(FirstPoint.Y - LastPoint.Y));
        
        for (var i = area.Left; i < area.Right; i++) {
            for (var j = area.Top; j < area.Bottom; j++) {
                var tile = Framing.GetTileSafely(i, j);

                tile.Get<LiquidData>();
                tile.Get<TileTypeData>();
                tile.Get<WallTypeData>();
                tile.Get<TileWallWireStateData>();
                tile.Get<TileWallBrightnessInvisibilityData>();
            }
        }
        
        ClearSelection();
    }

    public override void Draw(SpriteBatch spriteBatch) {
        base.Draw(spriteBatch);

        if (!Selecting || !HasSelectedFirstPoint) {
            return;
        }
        
        var rectangleStart = FirstPoint * 16f - Main.screenPosition;
        var rectangleEnd = (HasSelectedLastPoint ? LastPoint : (Main.MouseWorld / 16f).Floor()) * 16f - Main.screenPosition;

        var rectangle = new Rectangle((int)MathF.Min(rectangleStart.X, rectangleEnd.X),
            (int)MathF.Min(rectangleStart.Y, rectangleEnd.Y),
            (int)MathF.Abs(rectangleStart.X - rectangleEnd.X),
            (int)MathF.Abs(rectangleStart.Y - rectangleEnd.Y));
        
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, Color.White * 0.5f);
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
