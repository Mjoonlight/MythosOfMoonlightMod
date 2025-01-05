using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Microsoft.Xna.Framework;
using MythosOfMoonlight.Items.Armor;
using Terraria.ID;
using MythosOfMoonlight.Items.Jungle;
using MythosOfMoonlight.Items.PurpleComet.IridicSet;
using MythosOfMoonlight.Items.Materials;

namespace MythosOfMoonlight.Common.Utilities
{
    public class MoMGlowmaskItem : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {

            List<int> types = new List<int>()
        {
            ModContent.ItemType<CDGIris>(), ModContent.ItemType<PlantGun>(), ModContent.ItemType<GalactiteOre>()
        };
            if (drawInfo.heldItem != null)
                if (types.Contains(drawInfo.heldItem.type) && !drawInfo.drawPlayer.ItemTimeIsZero)
                {

                    Vector2 offset = Vector2.Zero;
                    Vector2 origin = Vector2.Zero;
                    float rotOffset = 0;
                    Item item = drawInfo.heldItem;
                    Texture2D texture = Helper.GetTex(item.ModItem.Texture + "_Glow");
                    if (item.useStyle == ItemUseStyleID.Shoot)
                    {
                        if (Item.staff[item.type])
                        {
                            rotOffset = 0.785f * drawInfo.drawPlayer.direction;
                            if (drawInfo.drawPlayer.gravDir == -1f)
                                rotOffset -= 1.57f * drawInfo.drawPlayer.direction;

                            origin = new Vector2(texture.Width * 0.5f * (1 - drawInfo.drawPlayer.direction), drawInfo.drawPlayer.gravDir == -1f ? 0 : texture.Height);

                            int oldOriginX = -(int)origin.X;
                            ItemLoader.HoldoutOrigin(drawInfo.drawPlayer, ref origin);
                            offset = new Vector2(origin.X + oldOriginX, 0);
                        }
                        else
                        {
                            offset = new Vector2(0, texture.Height / 2);
                            ItemLoader.HoldoutOffset(drawInfo.drawPlayer.gravDir, item.type, ref offset);
                            origin = new Vector2(-offset.X, texture.Height / 2);
                            if (drawInfo.drawPlayer.direction == -1)
                                origin.X = texture.Width + offset.X;

                            offset = new Vector2(texture.Width / 2, offset.Y);
                        }
                    }
                    else
                    {
                        origin = new Vector2(texture.Width * 0.5f * (1 - drawInfo.drawPlayer.direction), drawInfo.drawPlayer.gravDir == -1f ? 0 : texture.Height);
                    }

                    drawInfo.DrawDataCache.Add(new DrawData(
                        texture,
                        drawInfo.ItemLocation - Main.screenPosition + offset - new Vector2(texture.Width / 2, 0),
                        texture.Bounds,
                        Color.White * ((255f - item.alpha) / 255f),
                        drawInfo.drawPlayer.itemRotation + rotOffset,
                        origin,
                        item.scale,
                        drawInfo.playerEffect,
                        0
                    ));
                }
        }
    }
}
