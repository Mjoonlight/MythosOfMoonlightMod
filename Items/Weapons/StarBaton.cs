using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MythosOfMoonlight.Dusts;

namespace MythosOfMoonlight.Items.Weapons
{
    public class StarBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.knockBack = 10f;
            Item.width = 48;
            Item.height = 66;
            Item.crit = 2;
            Item.damage = 12;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Green;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<StarBatonP>();
        }

        public override bool? CanAutoReuseItem(Player player)
        {
            return null;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
    }

    public class StarBatonP : ModProjectile
    {
        public ref float Charge => ref Projectile.ai[0];

        public ref float Mode => ref Projectile.ai[1];

        public ref float SpinTimer => ref Projectile.ai[2];

        public override string Texture => TryGetTextureFromOther<StarBaton>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 0;
            Projectile.timeLeft = 45;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (Projectile.owner == Main.myPlayer && !player.controlUseItem)
            {
                Projectile.Kill();
                return;
            }

            if (player.dead || !player.active || player.ghost)
            {
                Projectile.Kill();
                return;
            }

            if (Mode == 0f)
            {
                SpinTimer++;

                if (SpinTimer % 20 == 0)
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing with { Volume = 0.4f }, Projectile.Center);

                Vector2 ownerMountedCenter = player.RotatedRelativePoint(player.MountedCenter);

                if (player.velocity.X != 0)
                    player.ChangeDir(Math.Sign(player.velocity.X));

                Projectile.direction = player.direction;

                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;

                //ramp up rotation speed
                const float maxRotation = Pi / 6.85f / 1.5f;
                float modifier = maxRotation * Math.Min(1f, SpinTimer / 80f);
                Projectile.timeLeft = 10;

                Projectile.numHits = 0;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
                Projectile.rotation += modifier * player.direction;
                Projectile.velocity = Projectile.rotation.ToRotationVector2();
                Projectile.position -= Projectile.velocity;

                player.itemRotation = WrapAngle(Projectile.rotation);
                Projectile.Center = ownerMountedCenter + new Vector2(7f * Projectile.direction, 5f);

                if (SpinTimer >= 20)
                {
                    //dust when at a high enough velocity
                    Vector2 pos = Projectile.Center + Projectile.velocity * (Projectile.width / 1.5f); //front
                    Vector2 pos2 = Projectile.Center - Projectile.velocity * (Projectile.width / 1.5f); //bacc
                    Dust d1 = Dust.NewDustDirect(pos, 0, 0, DustID.Enchanted_Pink);
                    d1.color = Color.Lerp(Color.SkyBlue, Color.Violet, 0.75f + Main.rand.NextFloat(0.15f));
                    d1.velocity = Projectile.rotation.ToRotationVector2() * 0.8f;
                    d1.noGravity = true;
                    d1.scale = 0.8f;

                    Dust d2 = Dust.NewDustDirect(pos2, 0, 0, DustID.Enchanted_Gold);
                    d2.color = Color.Lerp(Color.SkyBlue, Color.Violet, 0.75f + Main.rand.NextFloat(0.15f));
                    d2.velocity = Projectile.rotation.ToRotationVector2() * 0.8f;
                    d2.noGravity = true;
                    d2.scale = 0.8f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Projectile.Distance(ClosestPointInHitbox(targetHitbox, Projectile.Center)) <= Projectile.width / 2;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (!target.noTileCollide && !Collision.CanHitLine(Projectile.Center, 0, 0, target.Center, 0, 0))
                return false;

            return base.CanHitNPC(target);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            //afterimages
            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = color26 * 0.5f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, Projectile.scale, SpriteEffects.None, 0);
            }

            Projectile.SimpleDrawProjectile(Request<Texture2D>(Texture).Value, Color.White, true, 1f, 0f);

            return false;
        }
    }
}
