using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Nightshade.Common.UI;

[Autoload(Side = ModSide.Client)]
public sealed class UIStructureCreatorSystem : ModSystem
{
    private static GameTime lastGameTimeUpdate;
    
    public static UserInterface CreatorInterface { get; private set; }

    public override void Load() {
        CreatorInterface = new UserInterface();
    }

    public override void Unload() {
        CreatorInterface = null;
    }

    public override void UpdateUI(GameTime gameTime) {
        CreatorInterface.Update(gameTime);

        lastGameTimeUpdate = gameTime;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        var index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index == -1) {
            return;
        }

        layers.Insert(index + 1, new LegacyGameInterfaceLayer("Nightshade:Selector", DrawSelectorUI));
    }

    public static void Enable() {
        CreatorInterface.SetState(new UIStructureCreator());
        CreatorInterface.CurrentState.Activate();
    }

    public static void Disable() {
        CreatorInterface.CurrentState.Deactivate();
        CreatorInterface.SetState(null);
    }

    private static bool DrawSelectorUI() {
        CreatorInterface.Draw(Main.spriteBatch, lastGameTimeUpdate);

        return true;
    }
}
