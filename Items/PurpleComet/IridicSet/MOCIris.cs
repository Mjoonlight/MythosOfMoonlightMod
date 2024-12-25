using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using MythosOfMoonlight.Projectiles.IridicProjectiles;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Systems;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Items.Weapons.Ranged;
using System;
using MythosOfMoonlight.Common.Crossmod;
using MythosOfMoonlight.Common.Globals;
using MythosOfMoonlight.Common.Utilities;
using System.Collections.Generic;
using MythosOfMoonlight.Assets.Effects;

namespace MythosOfMoonlight.Items.PurpleComet.IridicSet
{
    public class MOCIris : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("MOC-Iris");
            /* Tooltip.SetDefault("Mars Originated Cannon, still slightly radioactive.\n" +
                " Fires homing Embers after charging up. "); */
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 27;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 2;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3f;
            Item.width = 60;
            Item.height = 20;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = false;
            Item.channel = true;
            Item.value = Item.buyPrice(0, 0, 0, 1);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<MOCIrisProj2>();
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Helper.GetTex(Texture + "_Glow");
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }
        public override bool CanShoot(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<MOCIrisProj>()] < 1 && player.statMana > 5;
        }
        public override bool MagicPrefix()
        {
            return true;
        }
    }

    public class MOCIrisProj2 : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<MOCIris>();

        public ref float Time => ref Projectile.ai[0];

        public ref float KillTimer => ref Projectile.ai[1];

        private Player Owner => Main.player[Projectile.owner];

        private Texture2D Tex, GlowTex;

        public bool die, fired, actuallyFired = false;

        public float RotOffset = 0f;

        public float MaxChargeTime = 360f; //holding it up to here causes it to auto fire

        public float FullyChargedTime = 180f;

        float FireCheck, dustTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 30;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.netImportant = true;
            Projectile.Opacity = 0f;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanDamage() => false;

        public Vector2 tipPosition = Vector2.Zero;

        float mult = 1f;

        public override void AI()
        {
            void FiringSound()
            {

            }

            if (Projectile.owner != Main.myPlayer)
                return;

            if (Owner.dead || !Owner.active || Owner.noItems || Owner.CCed || !Owner.CheckMana(Owner.HeldItem.mana, false))
                Projectile.Kill();

            Projectile.damage = Owner.GetWeaponDamage(Owner.HeldItem);
            Projectile.CritChance = Owner.GetWeaponCrit(Owner.HeldItem);
            Projectile.knockBack = Owner.GetWeaponKnockback(Owner.HeldItem, Owner.HeldItem.knockBack);

            Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, false, true);

            if (!die) //fade in
                Projectile.Opacity = Lerp(Projectile.Opacity, 1f, 0.12f);

            tipPosition = (armPos + Projectile.velocity * Projectile.width * 0.85f);

            Projectile.ManageHeldProj(armPos, new HeldprojSettings(26f, (Projectile.spriteDirection == -1 ? Pi : 0f), Owner, RotOffset));

            if (Owner.channel && !die)
            {
                Projectile.timeLeft++;

                Time++;

                if (Time % 12f == 0)
                    Owner.CheckMana(Owner.HeldItem.mana, true);

                if (Time >= MaxChargeTime)
                    die = true;

                if (Main.rand.NextBool())
                {
                    Vector2 dustPosition = tipPosition + (new Vector2(Main.rand.NextFloat(55f, 80f) * (1.15f - (Time / FullyChargedTime)), Main.rand.NextFloat(-30f, 30f) * (1.3f - (Time / FullyChargedTime))).RotatedBy(Projectile.velocity.ToRotation()));
                    Vector2 dustVelocity = NormalizeBetter(dustPosition.DirectionTo(tipPosition)) * Main.rand.NextFloat(3f, 7f) * (1.2f - (Time / FullyChargedTime));

                    //CreateDust(DustType<PurpurineDust>(), dustVelocity, dustPosition, Color.White, Main.rand.NextFloat(0.8f, 1.2f) * (1.2f - (Time / FullyChargedTime)));
                }

                if (Time > 3f)
                {
                    float timeValue = (float)Main.time * 0.02f + Projectile.whoAmI;

                    for (int i = 0; i < 8; i++)
                    {
                        float rot = timeValue * (30 - i * 2);
                        float theta = i + timeValue;
                        float phi = MathF.Sin(i * timeValue * 0.06f) * 2;
                        float size = (MathF.Cos(theta - timeValue * 0.43f) + 2f) / 3f;

                        for (int x = 0; x <= 5; x++)
                        {
                            Vector2 v0 = new Vector2(0, 40f + i * 2).RotatedBy(x / 2f + rot) * (1f - (Time / FullyChargedTime));
                            Vector2 newVelocity = v0.RotatedBy(PiOver2);

                            v0.X *= MathF.Cos(theta);
                            newVelocity.X *= MathF.Cos(theta);
                            v0 = v0.RotatedBy(phi);
                            newVelocity = newVelocity.RotatedBy(phi);
                            v0 *= size;
                            newVelocity *= size;
                            newVelocity *= 0.1f;

                            if (++dustTimer % 20 == 0f)
                            {
                                CreateDust(DustType<PurpurineDust>(), newVelocity, tipPosition + v0 - newVelocity, Color.White, Main.rand.NextFloat(0.8f, 1.2f) * (1.2f - (Time / FullyChargedTime)));
                            }
                        }
                    }
                }

                if(Time == FullyChargedTime)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        float rotation = TwoPi * i / 40;

                        Vector2 vel = rotation.ToRotationVector2() * ((i % 3 == 0) ? 0.5f : 1.5f);

                        Vector2 pos = tipPosition + vel * 15f;

                        CreateDust(DustType<PurpurineDust>(), (vel * 2.5f).RotatedBy(Sin(vel.X)), pos, default);
                    }

                    SoundEngine.PlaySound(SoundID.NPCHit5 with { Pitch = -0.23f, Volume = 0.6f }, tipPosition);
                }
            }

            if (!Owner.channel || die)
            {
                die = true;

                if (!fired)
                {
                    float percent = Time / FullyChargedTime;

                    if (percent < 0.333f)
                        fired = true; //end here, dont do anything

                    Vector2 projVel = NormalizeBetter(Projectile.velocity) * 10f;

                    if (percent >= 0.333f && percent < 0.666f) // 1 blast
                    {
                        if (!actuallyFired)
                        {
                            SpawnProjectle(Owner, ProjectileType<MOCIrisProj3>(), tipPosition + projVel * 1.75f, projVel * 3f, Projectile.damage, 3f);

                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 dvel = projVel.RotatedByRandom(Pi * 0.45f) * Main.rand.NextFloat(0.13f, 0.71f);
                                Vector2 dPos = tipPosition + (dvel * 0.5f);

                                CreateDust(DustType<PurpurineDust>(), dvel, dPos, Color.White, Main.rand.NextFloat(0.48f, 0.92f));
                                CreateDust(DustType<PurpurineDust>(), dvel, dPos * 0.5f, Color.White, Main.rand.NextFloat(0.48f, 0.92f) * 1.5f);
                            }

                            actuallyFired = true;
                        }

                        RotOffset = Lerp(RotOffset, -PiOver4 * 0.4f * Projectile.direction, 0.15f);

                        if (++FireCheck >= 18)
                            fired = true;
                    }

                    if (percent >= 0.666f && percent < 1f) // 2 blasts
                    {
                        if (FireCheck == 0f || FireCheck == 18f)
                        {
                            SpawnProjectle(Owner, ProjectileType<MOCIrisProj3>(), tipPosition + projVel * 1.75f, projVel * 3f, Projectile.damage, 3f);

                            for (int i = 0; i < 8 + (1 + (FireCheck / 30f)); i++)
                            {
                                Vector2 dvel = projVel.RotatedByRandom(Pi * 0.45f) * Main.rand.NextFloat(0.13f, 0.78f) * ((FireCheck / 30f) + 1);
                                Vector2 dPos = tipPosition + (dvel * 0.5f);

                                CreateDust(DustType<PurpurineDust>(), dvel, dPos, Color.White, Main.rand.NextFloat(0.68f, 1.02f));
                                CreateDust(DustType<PurpurineDust>(), dvel, dPos * 0.5f, Color.White, Main.rand.NextFloat(0.68f, 1.092f) * 1.5f);
                            }

                            mult = Main.rand.NextFloat(0.9f, 1.2f);
                        }

                        if (FireCheck < 9f || FireCheck > 18f)
                            RotOffset = Lerp(RotOffset, -PiOver4 * 0.4f * Projectile.direction * mult, 0.2f);
                    
                        if(FireCheck > 9f && FireCheck < 18f)
                            RotOffset = Lerp(RotOffset, 0f, 0.21f);

                        if (++FireCheck >= 45)
                            fired = true;
                    }

                    if (percent >= 1f) // 3 blasts
                    {
                        if (FireCheck == 0f || FireCheck == 18f || FireCheck == 36f)
                        {
                            SpawnProjectle(Owner, ProjectileType<MOCIrisProj3>(), tipPosition + projVel * 1.75f, projVel * 3f, Projectile.damage, 3f);

                            for (int i = 0; i < 8 + (1 + (FireCheck / 30f)); i++)
                            {
                                Vector2 dvel = projVel.RotatedByRandom(Pi * 0.45f) * Main.rand.NextFloat(0.3f, 1f) * ((FireCheck / 30f) + 1);
                                Vector2 dPos = tipPosition + (dvel * 0.5f);

                                CreateDust(DustType<PurpurineDust>(), dvel, dPos, Color.White, Main.rand.NextFloat(0.8f, 1.2f));
                                CreateDust(DustType<PurpurineDust>(), dvel * 0.5f, dPos, Color.White, Main.rand.NextFloat(0.8f, 1.2f) * 1.5f);
                            }

                            mult += 0.2f;
                        }

                        if (FireCheck < 9f || (FireCheck > 18f && FireCheck < 27f) || FireCheck > 36f)
                            RotOffset = Lerp(RotOffset, -PiOver4 * 0.4f * Projectile.direction * mult, 0.2f);

                        if ((FireCheck > 9f && FireCheck < 18f) || (FireCheck < 36f && FireCheck > 27f))
                            RotOffset = Lerp(RotOffset, 0f, 0.18f);

                        if (++FireCheck >= 45f)
                            fired = true;
                    }
                }

                else
                {
                    Projectile.Opacity = Lerp(Projectile.Opacity, 0f, 0.3f);

                    RotOffset = Lerp(RotOffset, 0f, 0.15f);

                    if (++KillTimer >= 16)
                        Projectile.Kill();
                }
            }

        }

        #region drawing

        public override bool PreDraw(ref Color lightColor)
        {
            Tex ??= TextureAssets.Projectile[Type].Value;
            GlowTex ??= Request<Texture2D>(Texture + "_Glow").Value;

            Projectile.SimpleDrawProjectile(Tex, lightColor * Projectile.Opacity, true, 1f);
            Projectile.SimpleDrawProjectile(GlowTex, Color.Lerp(lightColor, Color.White, Time / (float)FullyChargedTime) * Projectile.Opacity, true, 1f);

            VFXManager.DrawCache.Add(() =>
            {
                Projectile.SimpleDrawProjectile(GlowTex, Color.Lerp(Color.Transparent, Color.Lerp(Color.Violet, Color.White, 0.6f), Time / (float)FullyChargedTime) * Projectile.Opacity, true, 1f);
            });

            return false;
        }

        #endregion
    }

    class MOCIrisProj3 : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<MOCIris>();

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Iris Ember");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;

            Projectile.AddElement(CrossModHelper.Celestial);
            Projectile.AddElement(CrossModHelper.Fire);
        }

        Texture2D trail;

        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;
            Projectile.netUpdate = true;
            Projectile.netImportant = true;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        Color ColorFunction(float t)
        {
            return Color.Lerp(Color.Lerp(Color.Purple, Color.White, 0.2f), Color.Lerp(Color.Purple, Color.Violet, 0.6f), t);
        }

        float WidthFunction(float t)
        {
            return Lerp(20f, 10f, 1f - t);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            trail ??= Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/slash").Value;

            if (Projectile.timeLeft < 298)
            {
                Trail.DrawTrail(Projectile, 1f, -104f, ColorFunction, WidthFunction);
                Trail.DrawTrail(Projectile, 1f, 104f, ColorFunction, WidthFunction);

                VFXManager.DrawCache.Add(() =>
                {
                    Trail.DrawTrail(Projectile, 1f, -104f, ColorFunction, WidthFunction);
                    Trail.DrawTrail(Projectile, 1f, 104f, ColorFunction, WidthFunction);
                });
            }

            Main.spriteBatch.Reload(BlendState.Additive);

           // Projectile.DrawTrail(TextureAssets.Extra[98], new Vector2(1.2f, 0.3f), Color.Violet, Color.Purple, 0f, 0.25f, 0.1f);
           //Projectile.DrawTrail(TextureAssets.Extra[98], new Vector2(1.2f, 0.3f), Color.Red, Color.BurlyWood, 0f, 0.25f, 0.1f);

            return false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.timeLeft < 290f)
                Projectile.GetGlobalProjectile<MoMGlobalProj>().HomingActions(Projectile, .135f + (++Projectile.localAI[0] / 100f), 25f, 500f);
            
            else Projectile.velocity *= 0.998f;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SoundEngine.PlaySound(SoundID.Item25, Projectile.Center);
          //  for (int i = 1; i <= 7; i++)
          //  {
             //   Vector2 vel = -Utils.SafeNormalize(Projectile.oldVelocity, Vector2.Zero).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 2f);
              //  Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<PurpurineDust>(), vel.X, vel.Y, 0, default, Main.rand.NextFloat(.6f, 1.8f));
              //  dust.noGravity = true;
            //}
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item25, Projectile.Center);
            for (int i = 1; i <= 7; i++)
            {
                Vector2 vel = -Utils.SafeNormalize(Projectile.oldVelocity, Vector2.Zero).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 2f);
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<PurpurineDust>(), vel.X, vel.Y, 0, default, Main.rand.NextFloat(.6f, 1.8f));
                dust.noGravity = true;
            }
            return base.OnTileCollide(oldVelocity);
        }
    }
}
