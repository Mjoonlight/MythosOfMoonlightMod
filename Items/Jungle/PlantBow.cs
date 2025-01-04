using MythosOfMoonlight.Assets.Sounds;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Graphics.MoMParticles;
using MythosOfMoonlight.Common.Graphics.MoMParticles.Types;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Items.PurpleComet.Galactite;
using MythosOfMoonlight.NPCs.Enemies.Jungle.Sporebloom;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Items.Jungle
{
    public class PlantBow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 50;

            Item.useTime = Item.useAnimation = 20;

            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.damage = 15;
            Item.knockBack = 3f;
            Item.shoot = ProjectileType<PlantBowP>();
            Item.shootSpeed = 1f;
            Item.rare = ItemRarityID.LightRed;

            Item.useTurn = false;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;

            Item.value = Item.sellPrice(0, 1, 20, 0);
            Item.useAmmo = AmmoID.Arrow; //forgor
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => false; //spawning it shouldnt use 1 arrow
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ProjectileType<PlantBowP>()] < 1;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, velocity, ProjectileType<PlantBowP>(), damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class PlantBowP : ModProjectile
    {
        //public override string Texture => TryGetTextureFromOther<PlantBow>();

        public ref float Time => ref Projectile.ai[0];

        public ref float KillTimer => ref Projectile.ai[1];

        private Player Owner => Main.player[Projectile.owner];

        private Texture2D tex = null, glowTex = null, arrow = null, arrowGlow = null;

        public bool die = false;
        public float ArrowLerp = 0f; //for opacity lerp + position lerp
        public float StringLerp = 0f;
        public bool HideArrow = false; 
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 30;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.netImportant = true;
            Projectile.Opacity = 0f;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanDamage() => false;

        public Vector2 tipPosition = Vector2.Zero;

        public override void AI()
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            if (Owner.dead || !Owner.active || Owner.noItems || Owner.CCed || !ConsumeAmmo(Owner.HeldItem, Owner, out int i22, out int e22, 1, false))
                Projectile.Kill();

            Projectile.damage = Owner.GetWeaponDamage(Owner.HeldItem);
            Projectile.CritChance = Owner.GetWeaponCrit(Owner.HeldItem);
            Projectile.knockBack = Owner.GetWeaponKnockback(Owner.HeldItem, Owner.HeldItem.knockBack);

            Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, false, true);

            if (!die) //fade in
                Projectile.Opacity = Lerp(Projectile.Opacity, 1f, 0.0472f);

            tipPosition = (armPos + Projectile.velocity * Projectile.width * 0.7f) + new Vector2(0f, -1f * Projectile.direction).RotatedBy(Projectile.velocity.ToRotation());

            Projectile.ManageHeldProj(armPos, new HeldprojSettings(14f, (Projectile.spriteDirection == -1 ? Pi : 0f), Owner, 0f));
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - PiOver2);

            if (Owner.channel && !die)
            {
                Projectile.timeLeft++;

                if (++Time >= 85)
                {
                    if (Time == 102f) //fire
                    {
                        HideArrow = true;
                        SoundEngine.PlaySound(AudioRegistry.SFX_BowFire with { PitchVariance = 0.2f, Volume = 0.8f }, tipPosition);
                        SoundEngine.PlaySound(SoundID.Item5 with { PitchVariance = 0.2f }, tipPosition);

                        Vector2 vel = NormalizeBetter(Projectile.velocity) * Main.rand.NextFloat(12f, 15.3f);
                        Vector2 pos = (armPos + Projectile.velocity * Projectile.width * 0.7f) + new Vector2(10f * Projectile.direction, -1f * Projectile.direction).RotatedBy(Projectile.velocity.ToRotation());
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, vel, ProjectileType<SporeBowP2>(), Projectile.damage + (int)Projectile.ai[2], 3f, Projectile.owner);
                    }

                    if (Time > 102f)
                    {
                        ArrowLerp = Lerp(ArrowLerp, 0f, 0.18f); //doesnt matter as its not drawn now
                        StringLerp = Lerp(StringLerp, 0f, 0.23f);

                        if (Time >= 120)
                        {
                            ArrowLerp = StringLerp = 0f;
                            Time = 0f;
                            HideArrow = false;
                        }
                    }
                }

                else
                {
                    if (Time == 1f)
                    {
                        if (!ConsumeAmmo(Owner.HeldItem, Owner, out int why, out int soSerious, 1))
                            die = true;

                        Projectile.ai[2] = soSerious;
                        SoundEngine.PlaySound(AudioRegistry.SFX_BowString with { PitchVariance = 0.2f, Volume = 0.57f }, tipPosition);
                    }

                    StringLerp = ArrowLerp = Lerp(ArrowLerp, 1f, 0.018f);
                }
            }

            if (!Owner.channel || die)
            {
                die = true;

                Projectile.Opacity = Lerp(Projectile.Opacity, 0f, 0.13f);
                ArrowLerp = 0f;
                if (++KillTimer >= 30)
                {
                    Projectile.Kill();
                }
            }
        }

        #region drawing

        public override bool PreDraw(ref Color lightColor)
        {
            tex ??= Request<Texture2D>(Texture).Value;
            glowTex ??= Request<Texture2D>(Texture + "_Glow").Value;

            Projectile.SimpleDrawProjectile(tex, lightColor * Projectile.Opacity, false, 1f);
            Projectile.SimpleDrawProjectile(glowTex, Color.White * Projectile.Opacity, true, 1f);

            DrawStrings();

            if (!HideArrow)
                DrawArrow(ref lightColor);

            return false;
        }

        void DrawArrow(ref Color lc)
        {
            arrow ??= Request<Texture2D>(Texture + "_Arrow").Value;
            arrowGlow ??= Request<Texture2D>(Texture + "_Arrow_Glow").Value;

            Main.spriteBatch.Draw(arrow, Projectile.Center + Vector2.Lerp(new Vector2(21f * Projectile.direction, 0).RotatedBy(Projectile.rotation), Vector2.Zero, ArrowLerp) - Main.screenPosition, null, lc * ArrowLerp, Projectile.velocity.ToRotation(), arrow.Size() / 2f, 1f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(arrowGlow, Projectile.Center + Vector2.Lerp(new Vector2(21f * Projectile.direction, 0).RotatedBy(Projectile.rotation), Vector2.Zero, ArrowLerp) - Main.screenPosition, null, Color.White * ArrowLerp, Projectile.velocity.ToRotation(), arrow.Size() / 2f, 1f, SpriteEffects.None, 0);
        }

        void DrawStrings()
        {
            Vector2 start1 = Projectile.Center - new Vector2(10, 20).RotatedBy(Projectile.velocity.ToRotation());
            Vector2 end1 = Projectile.Center - new Vector2(10f * (0.5f + StringLerp), -5f).RotatedBy(Projectile.velocity.ToRotation());

            Vector2 start2 = Projectile.Center - new Vector2(12f, -20).RotatedBy(Projectile.velocity.ToRotation());
            Vector2 end2 = Projectile.Center - new Vector2(12f * (0.5f + StringLerp), 5f).RotatedBy(Projectile.velocity.ToRotation());

            Utils.DrawLine(Main.spriteBatch, start1, end1, Color.Purple * Projectile.Opacity, Color.Violet * Projectile.Opacity, 2);
            Utils.DrawLine(Main.spriteBatch, start2, end2, Color.Purple * Projectile.Opacity, Color.Violet * Projectile.Opacity, 2);
        }

        #endregion
    }

    class SporeBowP2 : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<PlantBowP>() + "_Arrow";

        Texture2D tex = null, texGlow = null;

        float forceGravity = 9.81f;

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 14;

            Projectile.penetrate = 2;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.friendly = true;

            Projectile.timeLeft = 360;

            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.velocity.Y = Lerp(Projectile.velocity.Y, 10f, forceGravity / 981f);
            forceGravity += (Projectile.velocity.X * 0.001f) + (float)(6.673f * 10E-11);

            if (Main.rand.NextBool(4))
            {
                Vector2 pos = Projectile.Center + Projectile.velocity * 1.5f; //tip-ish
                Vector2 vel = (Projectile.velocity * 0.15f) + Main.rand.NextVector2Circular(1.6f, 1.6f);

                ParticleHandler.SpawnParticle(
                new GlowyBall(pos, vel, Color.Pink, Color.HotPink, 25, 0.8f, 0f, 0.35f, 0.67f, Vector2.One, 0.93f, 0.0045f)
                {
                   BloomColor = Color.Violet,
                   DrawWithBloom = true
                });
            }

            if (Main.rand.NextBool(14))
            {
                Vector2 pos = Projectile.Center + Projectile.velocity * 1.5f; //tip-ish
                Vector2 vel = (Projectile.velocity * 0.02f) + Main.rand.NextVector2Circular(1f, 1f);

                var p = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), pos, vel, ProjectileType<SporebloomSpore>(), Projectile.damage / 2, 2f, Projectile.owner);
                p.DamageType = DamageClass.Ranged;
                p.hostile = false;
                p.friendly = true;
                p.usesLocalNPCImmunity = true;
                p.localNPCHitCooldown = 8;
                p.penetrate = -1;
                p.timeLeft = 180;
                p.alpha = 100;
                p.scale = 0.6f;
                p.ai[2] = 1f;
                p.damage = 0; //no damage?
                p.netUpdate = true;

                Projectile.netUpdate = true;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 4;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, hit.Crit ? 360 : 240);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            tex ??= Request<Texture2D>(Texture).Value;
            texGlow ??= Request<Texture2D>(Texture + "_Glow").Value;

            Projectile.SimpleDrawProjectile(tex, lightColor, false);
            Projectile.SimpleDrawProjectile(texGlow, Color.White, true);
            return false;
        }
    }
}
