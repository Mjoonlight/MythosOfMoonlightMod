using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ObjectData;

namespace MythosOfMoonlight.Tiles.Furniture.MusicBoxes.PurpleComet
{
    internal class PurpleCometMusicBoxITEM : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanGetPrefixes[Type] = false;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox;
        }

        public override void SetDefaults() => Item.DefaultToMusicBox(TileType<PurpleCometMusicBox>(), 0);
        
    }

    class PurpleCometMusicBox : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleLineSkip = 2;
            TileObjectData.addTile(Type);

            TileID.Sets.DisableSmartCursor[Type] = true;

            AddMapEntry(Color.DarkViolet, Language.GetText("ItemName.MusicBox"));
        }

        public override void MouseOver(int i, int j)
        {
            Player p = Main.LocalPlayer;

            p.noThrow = 2;
            p.cursorItemIconEnabled = true;
            p.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type);
        }

        public override bool CreateDust(int i, int j, ref int type) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) 
        {
            if (Main.gamePaused || !Main.instance.IsActive || Main.netMode == NetmodeID.Server || Lighting.UpdateEveryFrame && !Main.rand.NextBool(8))
                return;

            Tile tile = Main.tile[i, j];

            if (tile.TileFrameX == 36 && tile.TileFrameY % 36 == 0 && (int)Main.timeForVisualEffects % 11 == 0)
            {
                int gore = Main.rand.Next(570, 573);

                Vector2 pos = new (i * 16f + 8f, j * 16f - 8f); //above the center
                Vector2 vel = new (Main.WindForVisuals * -2f * Main.rand.NextFloatDirection(), -0.5f);

                vel.X *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);
                vel.Y *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);

                //account for their size

                if (gore == 572)
                    pos.X -= 8f;

                if (gore == 571)
                    pos.X -= 4f;

                Gore g = Gore.NewGoreDirect(new EntitySource_TileUpdate(i, j), pos, vel, gore, 0.78f);
                g.alpha = Main.rand.Next(0, 20);
                g.rotation += Main.rand.NextFloat(-0.001f, 0.0011f);
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            return base.PreDraw(i, j, spriteBatch);
        }
    }
}
