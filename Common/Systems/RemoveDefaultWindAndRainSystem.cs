using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaAmbience.Common.Systems;

public class RemoveDefaultWindAndRainSystem : ModSystem {
    public override void Load() {
        if (!Main.dedServ)
            On_Main.Update += RemoveRainAndWind;
    }

    private void RemoveRainAndWind(On_Main.orig_Update orig, Main self, Microsoft.Xna.Framework.GameTime gameTime) {
        // is Music_45 windy and Music_28 rainy?
        orig(self, gameTime);

        // woind
        Main.musicFade[45] = -1f;
        // rain
        Main.musicFade[28] = -1f;
        Main.audioSystem.UpdateAmbientCueTowardStopping(45, 1, ref Main.musicFade[45], 0f);
        Main.audioSystem.UpdateAmbientCueTowardStopping(28, 1, ref Main.musicFade[28], 0f);
    }
}
