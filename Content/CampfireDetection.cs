using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using TerrariaAmbience.Content.Players;
using TerrariaAmbience.Core;
using Microsoft.Xna.Framework;

namespace TerrariaAmbience.Content;

public class CampfireDetection : GlobalTile
{
    public Vector2 originOfCampfire;

    public float distanceToCampfire;

    public bool isOnRight;
    public override void NearbyEffects(int i, int j, int type, bool closer) {
        if (Main.dedServ)
            return;
        originOfCampfire.X = i * 16;
        originOfCampfire.Y = j * 16;

        Player player = Main.player[Main.myPlayer].GetModPlayer<AmbientPlayer>().Player;

        if (!ModContent.GetInstance<GeneralConfig>().campfireSounds)
            return;
        if (type == TileID.Campfire && closer && player.HasBuff(BuffID.Campfire)) {
            /*var t = Main.tile[i, j];
            var orig = new Vector2(i * 16, j * 16);
            Main.NewText(t.frameY);*/
            //if (t.frameY <= 18 && t.frameY >= 0)
            {
                distanceToCampfire = Vector2.Distance(originOfCampfire, player.Center);
                isOnRight = originOfCampfire.X < player.Center.X;
                player.GetModPlayer<AmbientPlayer>().IsNearCampfire = true;
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
                player.GetModPlayer<AmbientPlayer>().IsNearCampfire = false;
            }
        }
    }
}
