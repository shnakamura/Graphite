using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Nightshade;

public sealed class Nightshade : Mod
{
    public static readonly string StructuresPath = Path.Combine(Main.SavePath, "Nightshade", "Structures");

    public override void Load() {
        if (Directory.Exists(StructuresPath)) {
            return;
        }

        Directory.CreateDirectory(StructuresPath);
    }
}