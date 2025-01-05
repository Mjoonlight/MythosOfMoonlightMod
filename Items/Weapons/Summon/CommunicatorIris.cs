using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MythosOfMoonlight.Items.Pets;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace MythosOfMoonlight.Items.Weapons.Summon
{
    public class CommunicatorIris : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 23;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 22;
            Item.height = 30;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<CommunicatorIrisP>();
            Item.buffType = ModContent.BuffType<CommunicatorIrisB>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack)
        {
            player.AddBuff(Item.buffType, 2);
            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockBack, Main.myPlayer);
            projectile.originalDamage = Item.damage;
            return false;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Helper.GetTex(Texture + "_Glow");
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }
    }
    public class CommunicatorIrisP : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 17;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.FennecFox);
            Projectile.Size = new Vector2(42, 40);
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.minionSlots = 1;
            AIType = 0;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            Player player = Main.player[Projectile.owner];
            fallThrough = player.position.Y > Projectile.position.Y + Projectile.height*1.5f;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
            return false;
        }
        public override bool? CanDamage() => false;
        Rectangle frames;
        float rotation;
        int frameCounter;
        public override bool PreDraw(ref Color lightColor)
        {
            frames.Width = 42;
            frames.Height = 42;
            Texture2D tex = Helper.GetTex(Texture);
            Texture2D glow = Helper.GetTex(Texture + "_Glow");
            Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0, 2) - Main.screenPosition, frames, lightColor, rotation, Projectile.Size / 2, Projectile.scale, (Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            Main.spriteBatch.Draw(glow, Projectile.Center + new Vector2(0, 2) - Main.screenPosition, frames, Color.White, rotation, Projectile.Size / 2, Projectile.scale, (Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
            return false;
        }
        void Idle(Player player)
        {
            Vector2 pos = player.Center;
            int offset = 0;
            for (int k = 0; k < Projectile.whoAmI; k++)
            {
                if (Main.projectile[k].active && Main.projectile[k].owner == Projectile.owner && Main.projectile[k].type == Projectile.type)
                {
                    offset++;
                }
            }
            pos += new Vector2(20, 0) * offset * -player.direction;
            if (offset > 0)
            pos.Y = TRay.Cast(pos - new Vector2(0, player.height/2), Vector2.UnitY, 200, true).Y;
            if (Projectile.Center.Distance(pos) > 1000)
                Projectile.Center += Helper.FromAToB(Projectile.Center, pos, false) *0.01f;
            Lighting.AddLight(Projectile.Center, 0.05f + ((MathF.Sin(Main.GlobalTimeWrappedHourly * 0.75f) + 1) * 0.5f) * 0.2f,0, 0.5f + ((MathF.Sin(Main.GlobalTimeWrappedHourly * 0.75f) + 1) * 0.5f) * 0.2f);
            frameCounter++;
            int height = frames.Height;
            Projectile.tileCollide = (Collision.CanHit(pos-new Vector2(player.width, player.height)/2 - new Vector2(0, 42), player.width, player.height, Projectile.position - new Vector2(0, 42), Projectile.width, Projectile.height) && Player.GetFloorTile(Projectile.Top.ToTileCoordinates().X, Projectile.Top.ToTileCoordinates().Y) == null && (pos.X.CloseTo(Projectile.Center.X, 600) && player.Center.Y.CloseTo(Projectile.Center.Y, player.height)));

                if ((Projectile.velocity.Y += 0.2f) > 12f) Projectile.velocity.Y = 12f;

            if (!Projectile.tileCollide)
            {
                Projectile.ai[2]++;
                Vector2 to = Helper.FromAToB(Projectile.Center, pos - new Vector2(0, 60));
                float s = MathHelper.Clamp(MathHelper.Lerp(5, 10, pos.Distance(Projectile.Center) / 150), 4, 10);
                if (Projectile.ai[2] % 40 <= 10)
                {
                    Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, to.X * s, 0.1f);
                }
                if (Projectile.ai[2] % 40 >= 35)
                    Projectile.velocity.X *= 0.9f;
                if (Projectile.velocity.Y > -10)
                    Projectile.velocity.Y += (to.Y * s) * 0.05f;

                if (frameCounter % 10 == 5)
                    frames.Y = 0;
                else
                    frames.Y = height;
            }
            else
            {
                int frameLimit = 16;
                while (Collision.SolidCollision(Projectile.position + new Vector2(-10, Projectile.height-6), Projectile.width + 20, 6) && --frameLimit > 0)
                    Projectile.Center -= Vector2.UnitY ;

                if ((double)Projectile.velocity.X < -0.8 || (double)Projectile.velocity.X > 0.8)
                {
                    Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
                    if (frameCounter % 5 == 0)
                    {
                        frames.Y += height;
                        if (frames.Y > 4 * height)
                            frames.Y = 0 * height;
                    }
                }
                else
                {
                    frames.Y = 0;
                    Projectile.spriteDirection = pos.X < Projectile.Center.X ? -1 : 1;
                }
                    rotation = Helper.LerpAngle(rotation, 0, 0.3f);
                

                float range = 6f;
                float speed = 0.2f;
                if (range < Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y))
                {
                    range = Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y);
                    speed = 0.3f;
                }

                float posX = pos.X + -50 * player.direction;
                if (posX < Projectile.position.X + (float)(Projectile.width / 2) - (float)42)
                {
                    if ((double)Projectile.velocity.X > -3.5)
                    {
                        Projectile.velocity.X -= speed;
                    }
                    else
                    {
                        Projectile.velocity.X -= speed * 0.25f;
                    }
                }
                else if (posX > Projectile.position.X + (float)(Projectile.width / 2) + (float)42)
                {
                    if ((double)Projectile.velocity.X < 3.5)
                    {
                        Projectile.velocity.X += speed;
                    }
                    else
                    {
                        Projectile.velocity.X += speed * 0.25f;
                    }
                }
                else
                {
                    Projectile.velocity.X *= 0.9f;
                    if (Projectile.velocity.X >= 0f - speed && Projectile.velocity.X <= speed)
                    {
                        Projectile.velocity.X = 0f;
                    }
                }
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.dead && player.HasBuff(ModContent.BuffType<CommunicatorIrisB>()))
            {
                Projectile.timeLeft = 2;
            }
            Idle(player);
        }
    }
    public class CommunicatorIrisB : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CommunicatorIrisP>()] > 0)
                player.buffTime[buffIndex] = 60;
        }
    }
}
