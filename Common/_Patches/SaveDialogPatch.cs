using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ModLoader;

namespace Nightshade.Common;

internal sealed class SaveDialogPatch : ILoadable
{
    void ILoadable.Load(Mod mod) {
        IL_nativefiledialog.NFD_SaveDialog += Patch;
    }

    void ILoadable.Unload() {
        IL_nativefiledialog.NFD_SaveDialog -= Patch;
    }

    private void Patch(ILContext il) {
        var method = typeof(nativefiledialog).GetMethod("UTF8_ToManaged", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        var cursor = new ILCursor(il);

        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCall(method))) {
            throw new Exception();
        }

        cursor.Emit(OpCodes.Pop);
        cursor.Emit(OpCodes.Ldc_I4_0);
    }
}
