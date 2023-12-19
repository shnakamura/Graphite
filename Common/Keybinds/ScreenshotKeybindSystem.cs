using Nightshade.Common.UI;
using Terraria;
using Terraria.ModLoader;

namespace Nightshade.Common.Keybinds;

public sealed class ScreenshotKeybindSystem : ModSystem
{
    public static ModKeybind ScreenshotKeybind { get; private set; }

    public override void Load() {
        ScreenshotKeybind = KeybindLoader.RegisterKeybind(Mod, "Screenshot", "P");
    }

    public override void Unload() {
        ScreenshotKeybind = null;
    }

    public override void PostUpdateInput() {
        if (!ScreenshotKeybind.JustPressed) {
            return;
        }
        
        UIStructureCreatorSystem.Enable();
    }
}
