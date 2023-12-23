using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;

namespace Nightshade.Common;

public static class StructureSerializer 
{
    public static void SerializeStructure(string path) {
        using var stream = new FileStream(path + ".ngtsh", FileMode.CreateNew);
        using var writer = new StreamWriter(stream);

        var rectangle = new Rectangle(0, 0, 0, 0);

        for (var i = rectangle.Left; i < rectangle.Left + rectangle.Width; i++) {
            for (var j = rectangle.Top; j < rectangle.Top + rectangle.Height; j++) {
                
            }
        }
        
        writer.Write("ahoy mexico!");
        writer.Flush();
    }
}
