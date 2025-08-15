using Terraria.ModLoader;
using TerrariaAmbience.Sounds.SFXEffects;

namespace TerrariaAmbience.Content.Players;

public class ReverbPlayer : ModPlayer
{
    public FilterParams LatestParams { get; set; }
}
