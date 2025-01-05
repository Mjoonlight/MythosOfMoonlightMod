using FullSerializer;
using Microsoft.Xna.Framework;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Items.Materials;
using MythosOfMoonlight.NPCs.Field;
using MythosOfMoonlight.NPCs.Minibosses.RupturedPilgrim;
using MythosOfMoonlight.Tiles;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MythosOfMoonlight.Common.Systems
{
    public class GenericSystem : ModSystem
    {
        bool hasChecked, galactiteReady;
        public static bool BeenThereDoneThatPilgrim;
        public static bool[] melissaQuest = new bool[3]; // 0 = Fertilizer 1 = SoR 2 = IGP
        public override void Load()
        {
            BeenThereDoneThatPilgrim = false;
            for (int i = 0; i < 3; i++)
                melissaQuest[i] = false;
        }
        public override void SaveWorldData(TagCompound tag)
        {
            for (int i = 0; i < 3; i++)
            {
                if (melissaQuest[i])
                    tag["MelissaQuest" + i] = true;
            }
        }
        public override void LoadWorldData(TagCompound tag)
        {
            for (int i = 0; i < 3; i++)
            {
                melissaQuest[i] = tag.ContainsKey("MelissaQuest" + i);
            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            BitsByte flags1 = new BitsByte();
            for (int i = 0; i < 3; i++)
            {
                flags1[i] = melissaQuest[i];
            }
            writer.Write(flags1);
        }
        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags1 = reader.ReadByte();
            for (int i = 0; i < 3; i++)
            {
                melissaQuest[i] = flags1[i];
            }
        }
        public override void PostUpdateEverything()
        {
            if (FieldSpawnRateNPC.rateDecrease && !NPC.AnyNPCs(ModContent.NPCType<SunflowerLady>())) //ADD REST OF FIELD NPCS HERE (&& NOT ||)
            {
                FieldSpawnRateNPC.rateDecrease = false;
                FieldSpawnRateNPC.activeNPC = -1;
            }
            if (!NPC.AnyNPCs(ModContent.NPCType<Starine_Symbol>()))
            {
                if (Main.dayTime)
                {
                    if (!hasChecked)
                    {
                        for (int i = 1; i < Main.maxTilesX; i++)
                        {
                            for (int j = 1; j < Main.maxTilesY; j++)
                            {
                                Tile tile = Main.tile[i, j];
                                if (tile.TileType == ModContent.TileType<SymbolPointTile>())
                                {
                                    if (!NPC.AnyNPCs(ModContent.NPCType<Starine_Symbol>()))
                                    {
                                        NPC npc = NPC.NewNPCDirect(null, (i - 1) * 16, (j - 2) * 16, ModContent.NPCType<Starine_Symbol>());
                                        npc.homeTileX = i - 2;
                                        npc.homeTileY = j - 1;
                                        if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC);
                                    }
                                    break;
                                }
                            }
                        }
                        hasChecked = true;
                    }
                }
                else
                    hasChecked = false;
            }
            if (!Main.dayTime && PurpleCometEvent.PurpleComet)
            {
                galactiteReady = false;
                if (Main.GameUpdateCount % 60 * 5 == 0)
                {
                    for (int i = 0; i < Main.maxTilesX; i++)
                    {
                        for (int j = 0; j < Main.maxTilesY; j++)
                        {
                            if (Main.tile[i, j].TileType == TileID.Meteorite && Main.tile[i, j].HasTile)
                                Main.tile[i, j].TileType = (ushort)ModContent.TileType<Galactite>();
                        }
                    }
                }
            }
            else if (!PurpleCometEvent.PurpleComet)
            {
                if (!galactiteReady)
                {
                    for (int i = 0; i < Main.maxTilesX; i++)
                    {
                        for (int j = 0; j < Main.maxTilesY; j++)
                        {
                            if (Main.tile[i, j].TileType == (ushort)ModContent.TileType<Galactite>() && Main.tile[i, j].HasTile)
                                Main.tile[i, j].TileType = TileID.Meteorite;
                        }
                    }
                    galactiteReady = true;
                }
            }
        }
    }
    public class AstralShowerMusic : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/Meteor");
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
        public override bool IsSceneEffectActive(Player player)
        {
            return !Main.dayTime && Star.starfallBoost > 2f && (player.ZoneOverworldHeight || player.ZoneSkyHeight);
        }
        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (IsSceneEffectActive(player))
            {
                if (!SkyManager.Instance["Asteroid"].IsActive())
                {
                    SkyManager.Instance.Activate("Asteroid");
                }
                if (player.ZoneOverworldHeight || player.ZoneSkyHeight)
                {
                    Filters.Scene["Asteroid"].GetShader().UseColor(Color.Blue).UseOpacity(0.1f);
                }
                player.ManageSpecialBiomeVisuals("Asteroid", isActive);
            }
            else
            {
                if (SkyManager.Instance["Asteroid"].IsActive())
                {
                    SkyManager.Instance.Deactivate("Asteroid");
                }
                player.ManageSpecialBiomeVisuals("Asteroid", false);
            }
        }
    }
}

