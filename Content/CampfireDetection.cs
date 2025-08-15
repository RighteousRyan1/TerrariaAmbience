using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Core;
using Microsoft.Xna.Framework;

namespace TerrariaAmbience.Content;

public class CampfireDetection : GlobalTile
{
    public static Vector2 CampfirePos;

    public static float CampfireDistance;
    public static bool IsCampfireOnTheRight;

    public static bool IsNearCampfire;
    public override void NearbyEffects(int i, int j, int type, bool closer) {
        if (Main.dedServ) return;
        if (!ModContent.GetInstance<GeneralConfig>().campfireSounds) return;

        CampfirePos.X = i * 16;
        CampfirePos.Y = j * 16;

        Player player = Main.player[Main.myPlayer].GetModPlayer<AmbientPlayer>().Player;

        if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire)) {
            /*var t = Main.tile[i, j];
            var orig = new Vector2(i * 16, j * 16);
            Main.NewText(t.frameY);*/
            //if (t.frameY <= 18 && t.frameY >= 0)
            {
                CampfireDistance = Vector2.Distance(CampfirePos, player.Center);
                IsCampfireOnTheRight = CampfirePos.X < player.Center.X;
                IsNearCampfire = true;
            }
        }
        // hacky ahh...
        if ((type == TileID.Campfire && !closer) || !player.HasBuff(BuffID.Campfire)) {
            /*var t = Main.tile[i, j];
            var orig = new Vector2(i * 16, j * 16);
            Main.NewText(t.frameY);*/
            //if (t.frameY <= 54 && t.frameY >= 36)
            {
                //if (TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance is not null)
                //TerrariaAmbience.DefaultAmbientHandler.CampfireCrackleInstance.Volume = 0f;
                IsNearCampfire = false;
            }
        }
    }
}
