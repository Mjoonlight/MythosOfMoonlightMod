using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using MythosOfMoonlight.Projectiles.IridicProjectiles;
using MythosOfMoonlight.Items.Materials;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using MythosOfMoonlight.Dusts;
using Microsoft.Xna.Framework.Graphics;
using MythosOfMoonlight.Assets.Effects;
using MythosOfMoonlight.Common.Base;
using Terraria.Graphics.Shaders;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Systems;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Collections.Generic;
using MythosOfMoonlight.Gores.Enemies;

namespace MythosOfMoonlight.Items.PurpleComet.IridicSet
{
    public class CDGIris2 : ModItem
    {
        public override string Texture => TryGetTextureFromOther<CDGIris>();

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("CDG-Iris");
            // Tooltip.SetDefault("Cosmos Derived Gun? Cosmos Derived Fun.");
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.noUseGraphic = false;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.width = 46;
            Item.height = 22;
            Item.useTime = 5;
            SoundStyle style = new SoundStyle("MythosOfMoonlight/Assets/Sounds/cdg");
            //style.Volume = .5f;
            style.MaxInstances = 400;

            Item.UseSound = style;
            Item.useAnimation = 15;
            Item.reuseDelay = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAmmo = AmmoID.Bullet;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(0, 0, 0, 1);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<FragmentBullet>();
            Item.shootSpeed = 1;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Helper.GetTex(Texture + "_Glow");
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }
        public override Vector2? HoldoutOffset() => new Vector2(0, 0);
        public override bool RangedPrefix()
        {
            return true;
        }
        public override bool AllowPrefix(int pre)
        {
            if (pre == PrefixID.Unreal) return true;
            return base.AllowPrefix(pre);
        }
        /*public override void UseAnimation(Player player)
        {
            Item.useAnimation = Item.useTime * 3;
        }*/
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            //if (type == ProjectileID.Bullet)
            if (player.itemAnimation > 5)
                velocity = velocity.RotatedByRandom(MathHelper.PiOver4 / 5);
            //velocity.Normalize();

            position += new Vector2(0, -4).RotatedBy(velocity.ToRotation()) * player.direction;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<FragmentBullet>(), damage, knockback, player.whoAmI);
            Helper.SpawnDust(position + new Vector2(50, 0).RotatedBy(velocity.ToRotation()), Vector2.One, ModContent.DustType<PurpurineDust>(), velocity * 3, 5);
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(Type)
                .AddIngredient(ModContent.ItemType<PurpurineQuartz>(), 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class CDGIris : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Ranged;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.width = 46;
            Item.height = 22;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = false;
            Item.channel = true;
            Item.value = Item.buyPrice(0, 0, 0, 1);
            Item.rare = ItemRarityID.Green;
            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileType<CDGIrisHeld>();
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            //Texture2D tex = Helper.GetTex(Texture + "_Glow");
            //spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ProjectileType<CDGIrisHeld>()] < 1;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (CanUseItem(player))
                Projectile.NewProjectile(source, position, velocity, ProjectileType<CDGIrisHeld>(), damage, knockback, player.whoAmI);

            return false;
        }

        public override bool RangedPrefix() => true;
    }

    public class CDGIrisHeld : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<CDGIris>();

        public ref float Time => ref Projectile.ai[0];

        public ref float CooldownTime => ref Projectile.ai[1];

        public ref float KillTimer => ref Projectile.ai[2];

        private Player Owner => Main.player[Projectile.owner];

        private Texture2D Tex, GlowTex;

        public bool die = false;

        public float RotOffset = 0f;

        public float FireCheck = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 40;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.netImportant = true;
            Projectile.Opacity = 0f;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanDamage() => false;

        public Vector2 tipPosition = Vector2.Zero;

        float mult = 1f;

        public override void AI()
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            if (Owner.dead || !Owner.active || Owner.noItems || Owner.CCed)
                Projectile.Kill();

            if (!ConsumeAmmo(Owner.HeldItem, Owner, out int no, out int no2, 1, false))
                die = true;

            Projectile.damage = Owner.GetWeaponDamage(Owner.HeldItem);
            Projectile.CritChance = Owner.GetWeaponCrit(Owner.HeldItem);
            Projectile.knockBack = Owner.GetWeaponKnockback(Owner.HeldItem, Owner.HeldItem.knockBack);

            Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, false, true);

            if (!die) //fade in
                Projectile.Opacity = Lerp(Projectile.Opacity, 1f, 0.12f);

            tipPosition = (armPos + Projectile.velocity * Projectile.width * 0.85f) + new Vector2(0f, -6f * Projectile.direction).RotatedBy(Projectile.velocity.ToRotation());

            Projectile.ManageHeldProj(armPos, new HeldprojSettings(10f, (Projectile.spriteDirection == -1 ? Pi : 0f), Owner, RotOffset));

            if (Owner.channel && !die)
            {
                Projectile.timeLeft++;

                if (CooldownTime <= 0)
                {
                    Time++;

                    if (Time == 13)
                    {
                        if (!ConsumeAmmo(Owner.HeldItem, Owner, out int type, out int damage, 1, true))
                            die = true;

                        Vector2 projVel = NormalizeBetter(Projectile.velocity) * 10f;

                        SpawnProjectle(Owner, ProjectileType<CDGIrisRound>(), tipPosition + projVel * 0.175f, projVel, Projectile.damage + damage, 3f);

                        if (FireCheck == 0f)
                            SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/cdg"), tipPosition);
                    
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 dvel = projVel.RotatedByRandom(Pi * 0.45f) * Main.rand.NextFloat(0.13f, 0.71f);
                            Vector2 dPos = tipPosition + (dvel * 0.5f);

                            CreateDust(DustType<PurpurineDust>(), dvel, dPos, Color.White, Main.rand.NextFloat(0.48f, 0.92f));
                            CreateDust(DustType<PurpurineDust>(), dvel, dPos * 0.5f, Color.White, Main.rand.NextFloat(0.48f, 0.92f) * 1.5f);
                        }
                    }

                    if (Time >= 12)
                        RotOffset = Lerp(RotOffset, -PiOver4 * 0.32f * Projectile.direction, 0.09f);

                    if (Time >= 20)
                    {
                        Time = 7;

                        if (++FireCheck >= 3)
                        {
                            CooldownTime = 40;
                            Time = 0;
                            FireCheck = 0;
                        }
                    }

                    RotOffset = Lerp(RotOffset, 0f, 0.15f);
                }

                else
                {
                    CooldownTime--;
                }
            }

            if ((!Owner.channel || die))
            {
                die = true;

                if (++KillTimer >= 20)
                    Projectile.Kill();

                Projectile.alpha += 255 / 20;
            }

        }

        #region drawing

        public override bool PreDraw(ref Color lightColor)
        {
            Tex ??= TextureAssets.Projectile[Type].Value;
            GlowTex ??= Request<Texture2D>(Texture + "_Glow").Value;

            Projectile.SimpleDrawProjectile(Tex, lightColor * Projectile.Opacity, true, 1f);
            Projectile.SimpleDrawProjectile(GlowTex, Color.White * Projectile.Opacity, true, 1f);

            return false;
        }

        #endregion
    }

    class TestLaser : BeamProjectile
    {
        public override string Texture => TryGetTextureFromOther<MOCIris>();

        public override float MaximumScale => 1.2f;

        public override float MaximumTime => 50f;

        public override float MaximumLength => 1500f;

        public override int TileCollisionDetectionCount => 12;

        public override float ElongationFactor => 0.7f;

        public override void SafeSetDefaults()
        {
            Projectile.width = 7;
            Projectile.height = 7;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.timeLeft = (int)MaximumTime;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void MiscAI()
        {
            if (Projectile.timeLeft == 49)
            {
                for(float f = 10f; f < CurrentLength; f += 5)
                {
                    Vector2 pos = Projectile.Center + Projectile.velocity * f;
                    Vector2 vel = Projectile.velocity * 0.2f;

                    CreateDust(DustType<StretchyGlow>(), vel, pos, Color.Lerp(Color.Violet, Color.Purple, LengthRatio * 3f), 1.23f, 0).customData = new Vector2(4.4f, 0.5f);
                }
            }
        }

        public override void TargetScale() => Projectile.scale = Projectile.timeLeft / MaximumTime * MaximumScale;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CanLaserGrow = false;
        }

        /*

        Color ColorFunction(float t)
        {
            return Color.Lerp(Color.Purple, Color.BlueViolet, t * 1.5f);
        }

        float WidthFunction(float t)
        {
            return Lerp(3f, 30f, t * 2.3f) * Projectile.scale;
        }

        */

        public override bool PreDraw(ref Color lightColor)
        {
            /*
             
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

            MiscShaderData data = GameShaders.Misc["BasicTrail"].UseColor(Color.Blue).UseImage1(Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/Ex1")).UseImage0(Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/Noise1"));

            data.Shader.Parameters["uWorldViewProjection"].SetValue(Helper.GetMatrix());

            Trail.DrawTrail(Projectile, data, 7f, 1.2f, ColorFunction, WidthFunction, points, rotations, -Main.screenPosition);

            Main.spriteBatch.ClearFromShaders();

            */
            return false;
        }
    }

    class CDGIrisRound : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<MOCIris>();

        Nullable<Vector2> start = null;

        bool stop = false;

        public float Misctimer, FadeTimer = 0f;

        Texture2D tex;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.width = 5;
            Projectile.height = 5;

            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.aiStyle = -1;

            Projectile.extraUpdates = 40;
        }

        public override bool PreAI()
        {
            start ??= Projectile.Center;
            return true;
        }

        public override void AI()
        {
            if (start != null)
            {
                if (stop)
                {
                    if (++FadeTimer >= 5)
                    {
                        Projectile.alpha += 1;
                        FadeTimer = 0f;
                    }

                    if (Projectile.Opacity <= 0f)
                        Projectile.Kill();

                    Misctimer += 0.25f;
                }

                else
                {
                    if (Projectile.Distance(start.Value) > 1200f)
                        stop = true;

                    Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
                }

                ProjectileID.Sets.DrawScreenCheckFluff[Type] = 5000;
            }
        }

        public override bool ShouldUpdatePosition() => !stop;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for(int i = 0; i < 8; i++)
            {
                Vector2 vel = (Projectile.velocity * 0.45f * Main.rand.NextFloat(0.7f, 1.5f)).RotatedByRandom(Pi / 3.4f);
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(3f, 3f);

                CreateDust(DustType<PurpurineDust>(), vel, pos, default, Main.rand.NextFloat(0.9f, 1.4f));
            }

            stop = true;
            Projectile.velocity = Vector2.Zero;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (-Projectile.velocity * 0.34f * Main.rand.NextFloat(0.7f, 1.5f)).RotatedByRandom(Pi / 3.4f);
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(3f, 3f);

                CreateDust(DustType<PurpurineDust>(), vel, pos, default, Main.rand.NextFloat(0.9f, 1.4f));
            }

            stop = true;
            Projectile.velocity = Vector2.Zero;

            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 1;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            tex ??= Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/GlowSuperSmall").Value;

            if (start != null)
            {
                float totalDistance = Vector2.Distance(Projectile.Center, start.Value);

                Main.spriteBatch.Reload(BlendState.Additive);

                for (float dist = 0f; dist < totalDistance; dist += 10f)
                {
                    Vector2 drawPos = Vector2.Lerp(start.Value, Projectile.Center, dist / totalDistance);

                    MythosOfMoonlight.StarryDrawCache.Add(() =>
                    {
                        Projectile.SimpleDrawProjectile(tex, drawPos - Main.screenPosition, Color.Lerp(Color.Lerp(Color.Violet, Color.Magenta, 0.4f), Color.Gray, (float)Misctimer / 200f) * Projectile.Opacity, true, 1f * new Vector2(0.1f, 1f));
                    });

                    Projectile.SimpleDrawProjectile(tex, drawPos - Main.screenPosition, Color.Lerp(Color.Lerp(Color.Violet, Color.Magenta, 0.4f), Color.Gray, (float)Misctimer / 200f) * Projectile.Opacity, true, 1f * new Vector2(0.1f, 1f));
                }

                Main.spriteBatch.Reload(BlendState.AlphaBlend);
            }

            return false;
        }
    }
}
