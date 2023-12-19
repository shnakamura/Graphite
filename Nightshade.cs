using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Nightshade;

public sealed class Nightshade : Mod
{
    public static readonly string PersonalPath = Path.Combine(Main.SavePath, "Nightshade");

    public override void Load() {
        Directory.CreateDirectory(PersonalPath);
    }
}