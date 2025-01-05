using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using MythosOfMoonlight.Dusts;
using System.Collections.Generic;
using MythosOfMoonlight.Items.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace MythosOfMoonlight.Tiles
{
    public class Galactite : ModTile
    {
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D glow = Helper.GetTex("MythosOfMoonlight/Tiles/Galactite_Glow");
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            spriteBatch.Draw(glow, new Vector2(i * 16, j * 16 + 2) - Main.screenPosition + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White);
        }
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = false;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 410;
            Main.tileShine2[Type] = false;
            Main.tileShine[Type] = 975;
            Main.tileMergeDirt[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;

            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Galactite Ore");
            AddMapEntry(new Color(94, 71, 142), name);
            DustType = ModContent.DustType<PurpurineStoneDust>();
            HitSound = SoundID.Tink;
            MinPick = 55;
        }
        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(ModContent.ItemType<GalactiteOre>());
        }
    }
    public class GalactiteManuallyPlaced : ModTile
    {
        public override string Texture => "MythosOfMoonlight/Tiles/Galactite";
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = false;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 410;
            Main.tileShine2[Type] = false;
            Main.tileShine[Type] = 975;
            Main.tileMergeDirt[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Galactite Ore");
            AddMapEntry(new Color(94, 71, 142), name);
            DustType = ModContent.DustType<PurpurineStoneDust>();
            HitSound = SoundID.Tink;
            MinPick = 55;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D glow = Helper.GetTex("MythosOfMoonlight/Tiles/Galactite_Glow");
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            spriteBatch.Draw(glow, new Vector2(i * 16, j * 16 + 2) - Main.screenPosition + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White);
        }
    }
}