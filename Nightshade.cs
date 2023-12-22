using Terraria;
using Terraria.ModLoader;

namespace Nightshade;

public sealed class Nightshade : Mod
{
    public static Nightshade Instance => ModContent.GetInstance<Nightshade>();

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        /*
        var effect = Assets.Request<Effect>("Assets/Effects/Contrast", AssetRequestMode.ImmediateLoad).Value;
        var data = new ScreenShaderData(new Ref<Effect>(effect), "ContrastPass");

        Filters.Scene[$"{nameof(Nightshade)}:Contrast"] = new Filter(data, EffectPriority.VeryHigh);
        Filters.Scene[$"{nameof(Nightshade)}:Contrast"].Load();
         */
    }
}
