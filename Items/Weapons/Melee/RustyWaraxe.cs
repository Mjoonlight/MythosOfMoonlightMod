using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using rail;
using MythosOfMoonlight.Projectiles;
using Terraria.Audio;
using MythosOfMoonlight.Buffs;
using MythosOfMoonlight.Common.Base;
using MythosOfMoonlight.Common.Systems;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Items.Materials;
using MythosOfMoonlight.Items.PurpleComet.Galactite;

namespace MythosOfMoonlight.Items.Weapons.Melee
{
    public class RustyWaraxe : ModItem
    {
        public override void SetDefaults()
        {
            Item.knockBack = 10f;
            Item.width = 54;
            Item.height = 54;
            Item.crit = 10;
            Item.damage = 15;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.channel = true;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Green;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<RustyWaraxeP>();

            Item.value = Item.buyPrice(0, 1, 50, 0);
        }
        int dir = 1;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            dir = -dir;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, dir);
            return false;
        }
    }
    public class RustyWaraxeP : HeldSword
    {
        public override string Texture => "MythosOfMoonlight/Items/Weapons/Melee/RustyWaraxe";
        public override void SetExtraDefaults()
        {
            swingTime = 30;
            holdOffset = 38;
            Projectile.Size = new(54, 54);
        }
        public override float Ease(float x)
        {
            return (float)(x == 0
  ? 0
  : x == 1
  ? 1
  : x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2
  : (2 - Math.Pow(2, -20 * x + 10)) / 2);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.Next(100) < 15)
            {
                //SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/rustyAxe"), Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
                for (int i = 0; i < 40; i++)
                    Dust.NewDust(target.position, target.width, target.height, DustID.Blood, Helper.FromAToB(Projectile.Center, target.Center).X * Main.rand.NextFloat(-10, 10), Helper.FromAToB(Projectile.Center, target.Center).Y * Main.rand.NextFloat(-10, 10), newColor: Color.Brown);
                target.AddBuff(ModContent.BuffType<RustyCut>(), 120);
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item1);
        }
    }

    public class RustyWaraxeR : ModItem
    {
        public override string Texture => TryGetTextureFromOther<RustyWaraxe>();

        public override void SetDefaults()
        {
            Item.knockBack = 10f;
            Item.width = Item.height = 54;
            Item.crit = 11;
            Item.damage = 16;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.channel = true;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Green;
            Item.shootSpeed = 1f;
            Item.shoot = ProjectileType<RustyWaraxeSwing>();
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (CanUseItem(player))
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0f);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<GalactiteOre>(50)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class RustyWaraxeSwing : SwordProjectile
    {
        public override string Texture => TryGetTextureFromOther<RustyWaraxe>();

        public override void SafeSetDefaults()
        {
            maxAttackType = 1; //up, down?
            shaderType = 0;
            trailLength = 12;
            disFromPlayer = 5;
            Projectile.scale *= 1.12f;
            Projectile.width = Projectile.height = 54;
            Projectile.extraUpdates = 1;
        }

        public override bool UseGlowmask => true;

        #region drawing stuff

        public override string TrailShapeTex() => TrailTexturePath + "TrailShape_Sharp";

        public override string TrailShapeTex2() => TrailTexturePath + "TrailShape_Sharp";

        public override string TrailColorTex() => TrailTexturePath + "Colors/White";

        public override string TrailColorTex2() => TrailTexturePath + "Colors/White";

        public override float TrailAlpha(float factor) => base.TrailAlpha(factor);

        public override BlendState TrailBlendState() => BlendState.Additive;

        public static SoundStyle SwingSound
        {
            get => SoundID.DD2_MonkStaffSwing with { Pitch = -0.25f, PitchVariance = 0.25f, MaxInstances = 0, Volume = 0.95f };
        }

        public override void DrawSelf(SpriteBatch spriteBatch, Color lightColor, Vector4 diagonal = default, Vector2 drawScale = default, Texture2D glowTexture = null)
        {
            if (drawScale == default)
                drawScale = new Vector2(-0.01f, 1.15f);

            diagonal = new Vector4(0, 1, 1, 0);

            Vector2 drawCenter = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawVertexByTwoLine(Request<Texture2D>(Texture).Value, lightColor, diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void DrawTrail(Color color)
        {
            //faded
            DrawTrailSection(TrailColorTex2(), TrailShapeTex(), 0.3f, 0.4f, 0.87f);

            //blurry + glowy sigmaness
            VFXManager.DrawCache.Add(() =>
            {
                DrawTrailSection(TrailColorTex(), TrailShapeTex(), 0.3f, 0.7f, 0.867f);
            });

            DrawTrailSection(TrailColorTex2(), TrailShapeTex(), 1f, 0.86f, 0.87f);
        }

        #endregion

        public override void End()
        {
            Projectile.Kill();
            Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = false;
        }

        bool b = false;
        float oldRot = 0f;
        public override void Attack()
        {
            void SwingEffects()
            {
                AttackSound(SwingSound);
                ResetProjectileDamage();
                ApplyScreenshake(Projectile.Center, 1f, 2f, 1000f, 4);
            }

            maxAttackType = 1;

            useTrail = true;
            float t = 1f;

            if(attackType == -1)
            {
                if (Player.controlUseTile)
                {

                }

                else
                {
                    EnactSwing_Simple(90f, 0f, 60f, 0.19f * ((timer > 45) ? 1.5f : 1f), 1f, SwingEffects, null, -1, true);

                    if (timer > 60f)
                    {
                        ResetProjectileDamage();
                        NextAttackType();
                    }
                }
            }

            if (attackType == 0)
            {
                float max = 65f;
                float max2 = 80f;
                float end = 103f;

                float t2 = (timer < max2 + 10 || timer > end - 10) ? 1f : 1.25f;

                if (timer < max)
                {
                    PrepareSwing(88f, -PiOver2 - Player.direction * 0.85f, t, true, true, -12f);
                }

                if (timer > max && timer <= max2)
                {
                    PrepareSwing(88f, -PiOver2 - Player.direction * 0.75f, t, true, true, -12f);
                }

                //EnactSwing(90f, max2, end, MiscArray[1] - 0.1f, 0.19f * t2, 1000f, t, [SwingEffects, null], -1, -12f);
                EnactSwing_Simple(90f, max2, end, 0.19f * t2, 1f, SwingEffects, null, -1, true);

                if (timer > end)
                    isAttacking = false;

                if (timer > end + 1)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (attackType == 1)
            {
                float max = 65f;
                float max2 = 80f;
                float end = 104f;

                float t2 = (timer < max2 + 10 || timer > end - 10) ? 1f : 1.25f;

                if (timer < max)
                {
                    PrepareSwing(88f, PiOver2 + Player.direction * 0.85f, t, true, true, -12f);
                }

                if (timer > max && timer <= max2)
                {
                    PrepareSwing(88f, PiOver2 + Player.direction * 0.75f, t, true, true, -12f);
                }

                //EnactSwing(90f, max2, end, MiscArray[1] + 0.23f, -0.18f * t2, 1000f, t, [SwingEffects, null], -1, -12f);
                EnactSwing_Simple(90f, max2, end, -0.18f * t2, 1f, SwingEffects, null, -1, true);

                if (timer > end)
                    isAttacking = false;

                if (timer > end)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (!isAttacking)
                useTrail = false;

            if (isAttacking && Main.rand.NextBool())
            {
                Vector2 pos2 = Projectile.Center + mainVec * Main.rand.NextFloat(0.76f, 0.9f);
                Vector2 vel2 = mainVec / 100f;

                CreateDust(DustID.Iron, vel2.RotatedBy(PiOver2 * -Sign(mainVec.Y)), pos2, default, Main.rand.NextFloat(0.897f, 1.13729f), 0, false, true);
            }
        }

        public override void SafeOnHitNPC(NPC target, NPC.HitInfo info, int dmgDone)
        {
            if (info.Crit)
            {
                SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);

                for (int i = 0; i < 40; i++)
                    Dust.NewDust(target.position, target.width, target.height, DustID.Blood, Helper.FromAToB(Projectile.Center, target.Center).X * Main.rand.NextFloat(-10, 10), Helper.FromAToB(Projectile.Center, target.Center).Y * Main.rand.NextFloat(-10, 10), newColor: Color.Brown);

                target.AddBuff(BuffType<RustyCut>(), 240);
            }

            ApplyScreenshake(target.Center, (attackType == 3) ? 20f : 8f, 2f, 500f, 10);
        }

        public override void SafeModifyHitNPC(NPC n, ref NPC.HitModifiers hitMods)
        {
            Strike = 1;
            hitMods.DefenseEffectiveness *= 0.25f;
        }

        public static int Strike = 0;

        public override void Load()
        {
            On_CombatText.NewText_Rectangle_Color_string_bool_bool += CombatText_NewText_Rectangle_Color_string_bool_bool;
        }

        private int CombatText_NewText_Rectangle_Color_string_bool_bool(On_CombatText.orig_NewText_Rectangle_Color_string_bool_bool orig, Rectangle location, Color color, string text, bool dramatic, bool dot)
        {
            if (Strike > 0)
            {
                color = Color.Lerp(Color.Gray, Color.Crimson, 0.215f);
                Strike--;
            }

            return orig(location, color, text, dramatic, dot);
        }

        public override void Unload()
        {
            On_CombatText.NewText_Rectangle_Color_string_bool_bool -= CombatText_NewText_Rectangle_Color_string_bool_bool;
        }
    }
}
