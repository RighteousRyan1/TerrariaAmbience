using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using TerrariaAmbience.Core;
using System.Linq;
using System;
using TerrariaAmbience.Content;
using TerrariaAmbience.Helpers;
using Terraria.Audio;
using TerrariaAmbience.Content.AmbientAndMore;
using TerrariaAmbience.Content.Players;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Collections.Generic;
using Terraria.ModLoader.Core;
using System.Reflection;
using TerrariaAmbience.Sounds;
using TerrariaAmbience.Sounds.SoundFilters;
using TerrariaAmbience.Common.Systems;
using TerrariaAmbience.Content.AddedNPCSounds;

namespace TerrariaAmbience;

public partial class TerrariaAmbience : Mod {
    // TODO: add all other types to the call

    public const float DEFAULT_FPS = 60f;
    public static float WorkaroundDeltaTime { get; private set; }
    public override object Call(params object[] args) {
        try {
            string message = args[0] as string;
            if (message == "AddTilesToList") {
                Mod mod = args[1] as Mod;
                string listName = args[2] as string; // Can be Stone, Grass, Sand, Snow, or Dirt (FOR NOW, OR EVER) (UPDATE THIS)
                object boxedInstance = args[3];

                if (!Main.dedServ) {
                    if (mod != null && boxedInstance is string[] s) {
                        if (listName == "Grass")
                            TileDetection.AddTilesToList(mod, FootstepHandler.GrassBlocks, s);
                        else if (listName == "Stone")
                            TileDetection.AddTilesToList(mod, FootstepHandler.StoneBlocks, s);
                        else if (listName == "Sand")
                            TileDetection.AddTilesToList(mod, FootstepHandler.SandBlocks, s);
                        else if (listName == "Snow")
                            TileDetection.AddTilesToList(mod, FootstepHandler.SnowBlocks, s);
                        else if (listName == "Dirt")
                            TileDetection.AddTilesToList(mod, FootstepHandler.DirtBlocks, s);
                        else if (listName == "Metal")
                            TileDetection.AddTilesToList(mod, FootstepHandler.MetalBlocks, s);
                        else if (listName == "Ice")
                            TileDetection.AddTilesToList(mod, FootstepHandler.WeakIceBlocks, s);
                        else if (listName == "Leaf")
                            TileDetection.AddTilesToList(mod, FootstepHandler.LeafBlocks, s);
                        else if (listName == "Glass")
                            TileDetection.AddTilesToList(mod, FootstepHandler.GlassBlocks, s);
                        else if (listName == "GraniteMarble")
                            TileDetection.AddTilesToList(mod, FootstepHandler.Marbles, s);
                        else if (listName == "SmoothStone")
                            TileDetection.AddTilesToList(mod, FootstepHandler.SmoothStones, s);
                        else if (listName == "Sticky")
                            TileDetection.AddTilesToList(mod, FootstepHandler.StickyBlocks, s);
                        else if (listName == "Gem")
                            TileDetection.AddTilesToList(mod, FootstepHandler.GemBlocks, s);
                        else {
                            Logger.Info("Mod.Call failure: Unknown tile list specified.");
                            return "Unknown tile list specified.";
                        }
                        Logger.Info($"Successfully added modded tiles ({string.Join(", ", s)}) to the {listName} tile list.");
                    }
                    else if (mod == null && boxedInstance is int[] i) {
                        if (listName == "Grass")
                            TileDetection.AddTilesToList(FootstepHandler.GrassBlocks, i);
                        else if (listName == "Stone")
                            TileDetection.AddTilesToList(FootstepHandler.StoneBlocks, i);
                        else if (listName == "Sand")
                            TileDetection.AddTilesToList(FootstepHandler.SandBlocks, i);
                        else if (listName == "Snow")
                            TileDetection.AddTilesToList(FootstepHandler.SnowBlocks, i);
                        else if (listName == "Dirt")
                            TileDetection.AddTilesToList(FootstepHandler.DirtBlocks, i);
                        else if (listName == "Metal")
                            TileDetection.AddTilesToList(FootstepHandler.MetalBlocks, i);
                        else if (listName == "Ice")
                            TileDetection.AddTilesToList(FootstepHandler.WeakIceBlocks, i);
                        else if (listName == "Leaf")
                            TileDetection.AddTilesToList(FootstepHandler.LeafBlocks, i);
                        else if (listName == "Glass")
                            TileDetection.AddTilesToList(FootstepHandler.GlassBlocks, i);
                        else if (listName == "GraniteMarble")
                            TileDetection.AddTilesToList(FootstepHandler.Marbles, i);
                        else if (listName == "SmoothStone")
                            TileDetection.AddTilesToList(FootstepHandler.SmoothStones, i);
                        else if (listName == "Sticky")
                            TileDetection.AddTilesToList(FootstepHandler.StickyBlocks, i);
                        Logger.Info($"Successfully added modded tiles ({string.Join(", ", i.Select(id => TileID.Search.GetName(id)))} to the {listName} tile list.");
                    }
                }

                return "Tiles added successfully!";
            }
            else if (message == "AddTilesToCraftingStations") {
                Mod mod = args[1] as Mod;
                string listName = args[2] as string; // use all crafting station stuff
                object boxedInstance = args[3];
            }
            else
                Logger.Error("Call Error: Unknown Message: " + message);
        }
        catch (Exception e) {
            Logger.Error("Mod.Call Error: " + e.StackTrace + e.Message);
        }
        return "Call Failed";
    }

    string _versCache;

    public static AmbientHandler DefaultAmbientHandler;
    public static FootstepHandler DefaultFootstepHandler;

    internal static List<ModBiome> dbg_modBiomes;

    public override void Load() {
        _versCache = Main.versionNumber;
        Main.versionNumber += $", Terraria Ambience v{Version}";

        AmbientHandler.InitializeAllAmbienceToAvoidRuntimeOverhead();
        AmbientPlayer.DetermineNonArmoryArmors();
        SlimeSounds.PopulateSlimes();
        // calls DefaultAmbientHandler.Initialize() via the ctor
        // Ambience.Initialize();

        On_Main.Update += Main_Update;

        MethodDetours.DetourAll();
    }
    private void Main_Update(On_Main.orig_Update orig, Main self, GameTime gameTime) {
        orig(self, gameTime);

        if (Main.dedServ) return;
        GeneralHelpers.ClickHandling();
        GeneralHelpers.UpdateButtons();

        DefaultAmbientHandler?.Update();
        DefaultFootstepHandler?.Update();

        GradientGlobals.Update();

        var ambCfg = ModContent.GetInstance<AmbientConfig>();
        if (ambCfg.bossFightMusicAdjust) {
            bool foundBoss = false;
            foreach (var npc in Main.ActiveNPCs) {
                if (!npc.boss) continue;

                foundBoss = true;
                break;
            }
            const float epsilon = 0.002f;
            var max = MathF.Round(ambCfg.bossFightMusicVolume, 2);
            var min = MathF.Round(ambCfg.bossFightMusicMin, 2);
            Main.musicVolume = MathHelper.Lerp(Main.musicVolume, foundBoss ? max : min, 0.02f * WorkaroundDeltaTime);

            if (Main.musicVolume - min < epsilon)
                Main.musicVolume = 0;
            else if (max - Main.musicVolume < epsilon)
                Main.musicVolume = max;
        }

        if (Main.gameMenu)
            return;

        WorkaroundDeltaTime = MathF.Round(60f * (float)gameTime.ElapsedGameTime.TotalSeconds * 1000) / 1000;
    }
    public override void PostSetupContent() {
        DefaultAmbientHandler = new();

        dbg_modBiomes = [.. ModContent.GetContent<ModBiome>()];
    }

    // sadly we put this here since ModSystem.PostSetupContent runs after the mod's
    public override void PostAddRecipes() {
        if (Main.dedServ) return;

        MenuDetours.Init();
        SoundChanges.Init();

        DefaultFootstepHandler = new();

        NPCFootstepHandler.InitializeNPCStepping();

        var campfireCrackle = Assets.Request<SoundEffect>("Sounds/Custom/ambient/environment/campfire_crackle", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        DefaultAmbientHandler.CampfireCrackleInstance = campfireCrackle.CreateInstance(); // what's crashing?
        DefaultAmbientHandler.CampfireCrackleInstance.IsLooped = true;
    }

    public override void Unload() {
        UnloadAllSFXSoWeCanAvoidStupidCrashesThatHappenOnUnload();
        MenuDetours.AddMenuButtonsHook = null;
        Main.versionNumber = _versCache;
    }

    static void UnloadAllSFXSoWeCanAvoidStupidCrashesThatHappenOnUnload() {
        // if null, nothing in this mod has even loaded yet xd
        if (DefaultAmbientHandler == null) return;
        DefaultAmbientHandler.CampfireCrackleInstance?.Dispose();
        DefaultAmbientHandler.CampfireCrackleInstance = null;

        AmbientPlayer.soundSlippyRoughInst?.Dispose();
        AmbientPlayer.soundSlippyRoughInst = null;
        AmbientPlayer.soundSlippySmoothInst?.Dispose();
        AmbientPlayer.soundSlippySmoothInst = null;
    }

    public float lastPlayerPositionOnGround;
    public float delta_lastPos_playerBottom;

    public override void HandlePacket(BinaryReader reader, int whoAmI) {
        var packet = reader.ReadInt32();

        switch (packet) {
            case TAPID.SEND_CRAFT:
                if (Main.dedServ) {
                    var p = GetPacket();
                    p.Write(packet);
                    p.WriteVector2(reader.ReadVector2());
                    p.Write(reader.ReadString());
                    p.Send(ignoreClient: whoAmI);
                }
                else {
                    var path = reader.ReadString();
                    var pos = reader.ReadVector2();
                    var audioCfg = ModContent.GetInstance<AudioConfig>();
                    var volScale = audioCfg.craftingSoundsVolume;
                    SoundEngine.PlaySound(new SoundStyle(path).WithVolumeScale(CraftSounds.UniversalSoundScale * volScale), pos);
                }
                break;
            case TAPID.SEND_AM_AMB:
                if (Main.dedServ) {
                    var p = GetPacket();
                    p.Write(packet);
                    p.Write(reader.ReadInt32());
                    p.Write(reader.ReadInt32());
                }
                else {
                    DefaultAmbientHandler.morningAmbienceForTheDay = reader.ReadInt32();
                    DefaultAmbientHandler.dayAmbienceForTheDay = reader.ReadInt32();

                    DefaultAmbientHandler.ForestMorning.ChangeTrack(AmbientHandler.AmbientPath + 
                        $"biome/forest/morning_{DefaultAmbientHandler.morningAmbienceForTheDay}");
                    DefaultAmbientHandler.ForestDay.ChangeTrack(AmbientHandler.AmbientPath + 
                        $"biome/forest/day_{DefaultAmbientHandler.dayAmbienceForTheDay}");

                    SyncAmbienceSystem.curEveningAmb = DefaultAmbientHandler.morningAmbienceForTheDay;
                    SyncAmbienceSystem.curNightAmb = DefaultAmbientHandler.dayAmbienceForTheDay;
                }
                break;
            case TAPID.SEND_PM_AMB:
                if (Main.dedServ) {
                    var p = GetPacket();
                    p.Write(packet);
                    // peak readability
                    p.Write(reader.ReadInt32());
                    p.Write(reader.ReadInt32());
                }
                else {
                    DefaultAmbientHandler.eveningAmbienceForTheDay = reader.ReadInt32();
                    DefaultAmbientHandler.nightAmbienceForTheDay = reader.ReadInt32();

                    DefaultAmbientHandler.ForestEvening.ChangeTrack(AmbientHandler.AmbientPath + 
                        $"biome/forest/evening_{DefaultAmbientHandler.eveningAmbienceForTheDay}");
                    DefaultAmbientHandler.ForestNight.ChangeTrack(AmbientHandler.AmbientPath + 
                        $"biome/forest/night_{DefaultAmbientHandler.nightAmbienceForTheDay}");

                    SyncAmbienceSystem.curEveningAmb = DefaultAmbientHandler.eveningAmbienceForTheDay;
                    SyncAmbienceSystem.curNightAmb = DefaultAmbientHandler.nightAmbienceForTheDay;
                }
                break;
            case TAPID.SEND_AMB_SFX:
                if (Main.dedServ) {
                    var p = GetPacket();
                    p.Write(packet);
                    p.Write(reader.ReadString());
                    p.WriteVector2(reader.ReadVector2());
                    p.Write(reader.ReadSingle());
                    p.Send();
                }
                else {
                    var path = reader.ReadString();
                    var pos = reader.ReadVector2();
                    var pitch = reader.ReadSingle();

                    ImmersiveSoundsSystem.PlaySound(path, pos, pitch);
                }
                break;
            case TAPID.AMB_ASK:
                if (Main.dedServ) {
                    var mp = ModContent.GetInstance<TerrariaAmbience>().GetPacket();
                    mp.Write(TAPID.AMB_GET);
                    mp.Write(SyncAmbienceSystem.curMorningAmb);
                    mp.Write(SyncAmbienceSystem.curDayAmb);
                    mp.Write(SyncAmbienceSystem.curEveningAmb);
                    mp.Write(SyncAmbienceSystem.curNightAmb);

                    mp.Send(toClient: whoAmI);
                }
                // only handle on the server lol
                break;
            case TAPID.AMB_GET:
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    var m = reader.ReadInt32();
                    var d = reader.ReadInt32();
                    var e = reader.ReadInt32();
                    var n = reader.ReadInt32();

                    if (m > 0)
                    DefaultAmbientHandler.ForestMorning.ChangeTrack(AmbientHandler.AmbientPath +
                        $"biome/forest/morning_{m}");
                    if (d > 0)
                    DefaultAmbientHandler.ForestDay.ChangeTrack(AmbientHandler.AmbientPath +
                        $"biome/forest/day_{d}");
                    if (e > 0)
                    DefaultAmbientHandler.ForestEvening.ChangeTrack(AmbientHandler.AmbientPath +
                        $"biome/forest/evening_{e}");
                    if (n > 0)
                    DefaultAmbientHandler.ForestNight.ChangeTrack(AmbientHandler.AmbientPath +
                        $"biome/forest/night_{n}");

                    SyncAmbienceSystem.curMorningAmb = m;
                    SyncAmbienceSystem.curDayAmb = d;
                    SyncAmbienceSystem.curEveningAmb = e;
                    SyncAmbienceSystem.curNightAmb = n;

                    //Main.NewText($"Synced ambiences: {m}, {d}, {e}, {n}");
                }
                break;
            default:
                throw new Exception("Unidentified.");
        }
    }
}