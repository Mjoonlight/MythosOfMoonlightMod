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
using Terraria.Graphics.Shaders;
using MythosOfMoonlight.NPCs.Minibosses.RupturedPilgrim.Projectiles;
using MythosOfMoonlight.Common.Base;

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

        public bool die, die2, fired, actuallyFired = false;

        public float RotOffset = 0f;

        public float MaxChargeTime = 245f; //holding it up to here causes it to auto fire

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
                SoundEngine.PlaySound(SoundID.Item68 with { Volume = 0.83f, Pitch = 0.3f - (Time / FullyChargedTime) }, tipPosition);
                //SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.53f, Pitch = 0.2f - (Time / FullyChargedTime) }, tipPosition);
                SoundEngine.PlaySound(SoundID.Item40 with { Pitch = -0.2f, Volume = 0.7f }, tipPosition);
            }

            if (Projectile.owner != Main.myPlayer)
                return;

            if (Owner.dead || !Owner.active || Owner.noItems || Owner.CCed)
                Projectile.Kill();

            if (!Owner.CheckMana(Owner.HeldItem.mana, false))
                die = true;

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

                if (Time == FullyChargedTime || Time == FullyChargedTime / 3f || Time == (2 * FullyChargedTime) / 3f)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        float rotation = TwoPi * i / 40;

                        Vector2 vel = rotation.ToRotationVector2() * ((i % 3 == 0) ? 0.5f : 1.5f);

                        Vector2 pos = tipPosition + vel * 15f;

                        CreateDust(DustType<PurpurineDust>(), (vel * 0.5f * (1.5f + (Time / FullyChargedTime))).RotatedBy(Sin(vel.X)), pos, default);
                    }

                    SoundEngine.PlaySound(SoundID.NPCHit5 with { Pitch = -2.1f + (1f + (Time / FullyChargedTime)), Volume = 0.6f }, tipPosition);
                }
            }

            if (!Owner.channel || die)
            {
                die = true;

                if (!fired)
                {
                    float percent = Time / FullyChargedTime;

                    float damage = (Projectile.damage * 1.2f) * (1.2f + (Time / MaxChargeTime));

                    if (percent < 0.333f)
                        fired = true; //end here, dont do anything

                    Vector2 projVel = NormalizeBetter(Projectile.velocity) * 10f;

                    if (percent >= 0.333f && percent < 0.666f) // 1 blast
                    {
                        if (!actuallyFired)
                        {
                            SpawnProjectle(Owner, ProjectileType<MOCIrisProj3>(), tipPosition + projVel * 1.75f, projVel * 3f, damage, 3f);
                            FiringSound();

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
                            SpawnProjectle(Owner, ProjectileType<MOCIrisProj3>(), tipPosition + projVel * 1.75f, projVel * 3f, damage, 3f);
                            FiringSound();

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

                        if (FireCheck > 9f && FireCheck < 18f)
                            RotOffset = Lerp(RotOffset, 0f, 0.21f);

                        if (++FireCheck >= 45)
                            fired = true;
                    }

                    if (percent >= 1f) // 3 blasts
                    {
                        if (FireCheck == 0f || FireCheck == 18f || FireCheck == 36f)
                        {
                            SpawnProjectle(Owner, ProjectileType<TestLaser>(), tipPosition + projVel * 1.75f, projVel * 3f, damage, 3f);
                            FiringSound();

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
                    if (Owner.channel && !die2)
                    {
                        RotOffset = Lerp(RotOffset, 0f, 0.15f);

                        if (RotOffset.CloseTo(0f, 0.1f))
                        {
                            actuallyFired = false;
                            die = false;
                            fired = false;
                            FireCheck = 0f;
                            die2 = false;
                            mult = 1f;
                            Time = 0f;
                        }
                    }

                    if (!Owner.channel)
                    {
                        die2 = true;

                        Projectile.Opacity = Lerp(Projectile.Opacity, 0f, 0.3f);

                        RotOffset = Lerp(RotOffset, 0f, 0.15f);

                        if (++KillTimer >= 16)
                            Projectile.Kill();
                    }
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
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;

            Projectile.AddElement(CrossModHelper.Celestial);
            Projectile.AddElement(CrossModHelper.Fire);
        }

        Texture2D trail, star;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;
            Projectile.netUpdate = true;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        Color ColorFunction(float t)
        {
            return Color.Lerp(Color.Lerp(Color.Purple, Color.White, 0.2f), Color.Lerp(Color.MediumPurple, Color.Violet, 0.6f), t);
        }

        float WidthFunction(float t)
        {
            return Lerp(30f, 0f, 1f - t) * Projectile.scale;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            trail ??= Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/slash").Value;
            star ??= Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/GlowyStar").Value;

            var shader = GameShaders.Misc["RainbowRod"];

            Main.spriteBatch.Reload(BlendState.Additive);

            if (Projectile.timeLeft < 298)
            {
                Trail.DrawTrail(Projectile, shader, 1f, -104f, ColorFunction, WidthFunction, Projectile.oldPos, Projectile.oldRot, -Main.screenPosition + (Projectile.Size / 2f));
                Trail.DrawTrail(Projectile, shader, 1f, 104f, ColorFunction, WidthFunction, Projectile.oldPos, Projectile.oldRot, -Main.screenPosition + (Projectile.Size / 2f));

                VFXManager.DrawCache.Add(() =>
                {
                    Trail.DrawTrail(Projectile, shader, 1f, -104f, ColorFunction, WidthFunction, Projectile.oldPos, Projectile.oldRot, -Main.screenPosition + (Projectile.Size / 2f));
                    Trail.DrawTrail(Projectile, shader, 1f, 104f, ColorFunction, WidthFunction, Projectile.oldPos, Projectile.oldRot, -Main.screenPosition + (Projectile.Size / 2f));
                });

                Projectile.DrawTrail(trail, new Vector2(0.14f, 1.3f), Color.Violet, Color.Purple, 0f, 0.1f, 0.1f);
                Projectile.DrawTrail(trail, new Vector2(0.125f, 1.25f), Color.DarkBlue, Color.White, 0f, 0.1f, 0.1f);
            }

            Projectile.SimpleDrawProjectile_Offset(star, Main.rand.NextVector2Circular(5f, 5f), Color.Violet * 0.25f, true, 1f, Main.GlobalTimeWrappedHourly * 4f);
            Projectile.SimpleDrawProjectile(star, Color.Violet, true, 0.7f, 0f);

            return false;
        }

        NPC homingTarget = null;
        bool chase, died = false;
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            //wait 10 ticks to home so the proj can actually go where it should!!!!
            if (Projectile.timeLeft < 290)
            {
                if (homingTarget == null && !chase)
                {
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && npc.CanBeChasedBy() && !npc.CountsAsACritter && !npc.friendly && npc.Distance(Projectile.Center) < 600f)
                        {
                            homingTarget = npc;
                            chase = true;
                            break;

                        }
                    }

                    if (homingTarget == null)
                    {
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.14f);

                        if (Projectile.velocity.Length().CloseTo(0.15f, 0.03f))
                        {
                            died = true; //no regular onkill stuff

                            for (int i = 0; i < 19; i++)
                            {
                                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);

                                CreateDust(DustType<PurpurineDust>(), vel, Projectile.Center + vel, Color.White, Main.rand.NextFloat(0.75f, 1.26f), 0);
                            }

                            Projectile.Kill();
                        }

                        chase = false;
                    }
                }

                if (homingTarget != null && chase)
                {
                    Vector2 pos = homingTarget.Center + (Projectile.velocity * 0.015f * Projectile.velocity.Length());

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(pos) * ((++Projectile.ai[0] / 20f) + 20f), 0.2f);

                    if (homingTarget == null || !homingTarget.active || homingTarget.life <= 0)
                        chase = false;
                }
            }

            for (int i = -1; i <= 1; i += 2)
            {
                Vector2 vel = Projectile.velocity * 0.03f * Main.rand.NextFloat(-1.15f, -0.51f);

                CreateDust(DustType<PurpurineDust>(), vel, Projectile.Center + Main.rand.NextVector2Circular(0.5f, 0.5f) + new Vector2(0f, 2f * i).RotatedBy(Projectile.velocity.ToRotation()), Color.White, Main.rand.NextFloat(0.75f, 1.26f), 0);
            }
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.Center + Projectile.velocity, Projectile.velocity, 4, 4);
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

            if (!died)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = new Vector2(0f, 7f * Main.rand.NextFloat(0.5f, 1.2f)).RotatedBy(Projectile.velocity.ToRotation() + Main.rand.NextFloat(-Pi / 4f, Pi / 4f) - PiOver2);

                    CreateDust(DustType<PurpurineDust>(), vel, Projectile.Center + Projectile.velocity, Color.White, Main.rand.NextFloat(0.75f, 1.26f), 0);
                }
            }
        }
    }

    class TestLaser : BeamProjectile
    {
        public override string Texture => TryGetTextureFromOther<MOCIris>();

        public override float MaximumScale => 1.2f;

        public override float MaximumTime => 50f;

        public override float MaximumLength => 1500f;

        public override void SafeSetDefaults()
        {
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.timeLeft = (int)MaximumTime;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void MiscAI()
        {

        }

        public override void TargetScale() => Projectile.scale = Projectile.timeLeft / MaximumTime * MaximumScale;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
        }

        Color ColorFunction(float t)
        {
            return Color.Lerp(Color.Purple, Color.BlueViolet, t * 1.5f);
        }

        float WidthFunction(float t)
        {
            return Lerp(3f, 30f, t * 2.3f) * Projectile.scale;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.PrepareForShaders(BlendState.Additive);

            Vector2 end = Projectile.Center + NormalizeBetter(Projectile.velocity) * CurrentLength;

            Vector2[] points = new Vector2[16];

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 randOffset = points[i];

                points[i] = Vector2.Lerp(Projectile.Center + randOffset, end, i / (float)(points.Length - 1f));
            }

            float[] rotations = new float[16];

            for (int i = 0; i < rotations.Length; i++)
            {
                rotations[i] = Projectile.velocity.ToRotation();
            }

            MiscShaderData data = GameShaders.Misc["FlameLash"].UseColor(Color.Blue).UseImage1(Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/Ex1")).UseImage0(Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/Noise1"));

            Trail.DrawTrail(Projectile, data, 7f, 1.2f, ColorFunction, WidthFunction, points, rotations, -Main.screenPosition);

            Main.spriteBatch.ClearFromShaders();

            return false;
        }
    }
}
