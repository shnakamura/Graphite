using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Nightshade.Common.UI;

[Autoload(Side = ModSide.Client)]
public sealed class UIStructureCreatorSystem : ModSystem
{
    public static UIStructureCreator CreatorState { get; private set; }
    public static UserInterface CreatorInterface { get; private set; }

    public override void Load() {
        CreatorState = new UIStructureCreator();
        CreatorState.Activate();

        CreatorInterface = new UserInterface();
        CreatorInterface.SetState(CreatorState);
    }

    public override void Unload() {
        CreatorState.Deactivate();
        CreatorState = null;

        CreatorInterface.SetState(null);
        CreatorInterface = null;
    }

    public override void UpdateUI(GameTime gameTime) {
        CreatorInterface.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        var index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index == -1) {
            return;
        }

        layers.Insert(index + 1, new LegacyGameInterfaceLayer("Nightshade:Selector", DrawSelectorUI));
    }

    private static bool DrawSelectorUI() {
        CreatorInterface.Draw(Main.spriteBatch, new GameTime());

        return true;
    }
}
