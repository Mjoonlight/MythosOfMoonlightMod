using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using MythosOfMoonlight.Items.Materials;
using MythosOfMoonlight.Dusts;
using Terraria.DataStructures;
using MythosOfMoonlight.Tiles;

namespace MythosOfMoonlight.Items.PurpleComet
{
    public class PurpleCometSpawn : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Purple Comet's Offering");
            /* Tooltip.SetDefault("Use at nighttime to call upon the Purple Comet\n" +
                "Not Consumable"); */
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 18;
            Item.rare = ItemRarityID.Pink;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 10;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.UseSound = SoundID.Item43;
        }

        public override bool CanUseItem(Player player)
        {
            if (PurpleCometEvent.PurpleComet || Main.dayTime)
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            Main.NewText("You feel like you're levitating...", 179, 0, 255);
            SoundEngine.PlaySound(SoundID.Roar, new Vector2((int)player.position.X, (int)player.position.Y));
            PurpleCometEvent.PurpleComet = true;

            Star.starfallBoost = 0;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);


            int limit = 100;
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (Main.tile[i, j].TileType == TileID.Meteorite && Main.tile[i, j].HasTile)
                        Main.tile[i, j].TileType = (ushort)ModContent.TileType<Tiles.Galactite>();
                    if (limit > 0)
                        for (int k = 0; k < 3; k++)
                        {
                            Dust.NewDustDirect(new Point16(i, j).ToVector2(), 16, 16, ModContent.DustType<PurpurineDust>(), Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
                            limit--;
                        }
                }
            }
            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<GalactiteOre>(100)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class AsteroidSpawn : ModItem
    {
        public override string Texture => "MythosOfMoonlight/Assets/Textures/Extra/vortex";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("ASTEROID's Offering");
            /* Tooltip.SetDefault("Use at nighttime to call upon the ASTEROID\n" +
                "Not ASTEROID"); */
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 18;
            Item.rare = ItemRarityID.Pink;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 10;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.UseSound = SoundID.Item43;
        }

        public override bool CanUseItem(Player player)
        {
            if (PurpleCometEvent.PurpleComet || Main.dayTime)
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            Main.NewText("You feel like you're ASTEROID...", 179, 0, 255);
            SoundEngine.PlaySound(SoundID.Roar, new Vector2((int)player.position.X, (int)player.position.Y));
            Star.starfallBoost = 3;
            bool a = true;
            if (Main.dayTime)
                Main.UpdateTime_StartNight(ref a);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
            return true;
        }
    }
    public class AsteroidDespawn : ModItem
    {
        public override string Texture => "MythosOfMoonlight/Assets/Textures/Extra/trail";
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("ASTEROID's Offering");
            /* Tooltip.SetDefault("Use at nighttime to call upon the ASTEROID\n" +
                "Not ASTEROID"); */
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 18;
            Item.rare = ItemRarityID.Pink;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 10;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.UseSound = SoundID.Item43;
        }

        public override bool CanUseItem(Player player)
        {
            if (Main.dayTime)
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            Main.NewText("You feel like you're NO LONGER ASTEROID...", 179, 0, 255);
            SoundEngine.PlaySound(SoundID.Roar, new Vector2((int)player.position.X, (int)player.position.Y));
            Star.starfallBoost = 0;
            bool a = true;
            if (Main.dayTime)
                Main.UpdateTime_StartNight(ref a);

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.WorldData);
            return true;
        }
    }
}
