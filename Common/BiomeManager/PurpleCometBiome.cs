using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using MythosOfMoonlight.Common.Graphics.Particles;
using MythosOfMoonlight.Common.Graphics;

namespace MythosOfMoonlight.Common.BiomeManager
{
    public class PurpleCometBiome : ModBiome
    {
        public static PurpleCometBiome Instance { get; set; }
        public PurpleCometBiome()
        {
            Instance = this;
        }
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Comet Flyby");
        }
        public override string BestiaryIcon
        {
            get
            {
                return base.BestiaryIcon;
            }
        }
        public override string BackgroundPath
        {
            get
            {
                return base.BackgroundPath;
            }
        }
        public override string MapBackground
        {
            get
            {
                return BackgroundPath;
            }
        }
        public override int Music
        {
            get
            {
                return MusicLoader.GetMusicSlot(Mod, "Assets/Music/PurpleComet");
            }
        }
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
        public override bool IsBiomeActive(Player player)
        {
            return PurpleCometEvent.PurpleComet && (Main.LocalPlayer.ZoneOverworldHeight || Main.LocalPlayer.ZoneSkyHeight || Main.LocalPlayer.ZoneDirtLayerHeight);
        }
        public override void OnInBiome(Player player)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(2) && (player.ZoneOverworldHeight || player.ZoneSkyHeight))
                {
                    Particle.Spawn<PurpurineParticle>(Main.screenPosition + Main.rand.NextVector2FromRectangle(new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2)), Main.rand.NextVector2Circular(3, 3), Color.White);
                }
                player.gravity = .2f;
                player.maxFallSpeed *= .75f;
            }
        }
    }
}
