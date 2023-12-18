using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nightshade.Common.UI.States;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Nightshade.Common.UI;

[Autoload(Side = ModSide.Client)]
public sealed class NightshadeUISystem : ModSystem
{
    public static UIAreaSelector SelectorState { get; private set; }
    public static UserInterface SelectorInterface { get; private set; }
    
    public override void Load() {
        SelectorState = new UIAreaSelector();
        SelectorState.Activate();

        SelectorInterface = new UserInterface();
        SelectorInterface.SetState(SelectorState);
    }

    public override void Unload() {
        SelectorState.Deactivate();
        SelectorState = null;
        
        SelectorInterface.SetState(null);
        SelectorInterface = null;
    }

    public override void UpdateUI(GameTime gameTime) {
        SelectorInterface.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        var index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index == -1) {
            return;
        }
        
        layers.Insert(index + 1, new LegacyGameInterfaceLayer("Nightshade:Selector", DrawSelectorUI));
    }

    private static bool DrawSelectorUI() {
        SelectorInterface.Draw(Main.spriteBatch, new GameTime());
        
        return true;
    }
}
