using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using MythosOfMoonlight.Projectiles.IridicProjectiles;
using MythosOfMoonlight.Items.Materials;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Base;
using MythosOfMoonlight.Common.Systems;
using MythosOfMoonlight.Dusts;
using rail;

namespace MythosOfMoonlight.Items.PurpleComet.IridicSet
{
    /*

    public class PurpurineSaber : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Iridic Saber");
            Tooltip.SetDefault("Every full swing creates a short energy slice forward.\n" +
                "Is that a lightsaber?\n" +
                "Nope, Even better!"); 
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Melee;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = false;
            //Item.channel = true;
            Item.value = Item.buyPrice(0, 0, 0, 1);
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<SlashWave>();
            Item.shootSpeed = 8f;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Helper.GetTex(Texture + "_Glow");
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PurpurineQuartz>(), 25)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    */

    internal class PurpurineSaber : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = 10;
            Item.staff[Type] = true;
            Item.shoot = ProjectileType<PurpurineSaberProj>();
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.value = Item.sellPrice(0, 3, 70, 0);
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (CanUseItem(player))
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0f);

            return false;
        }

        public override void AddRecipes() { }
    }

    public class PurpurineSaberProj : SwordProjectile 
    {
        public override string Texture => TryGetTextureFromOther<PurpurineSaber>();

        public override void SafeSetDefaults()
        {
            maxAttackType = 4;
            shaderType = 0;
            trailLength = 100;
            disFromPlayer = 5;
            Projectile.scale *= 1.12f;

            Projectile.extraUpdates = 3;
        }

        public override bool UseGlowmask => true;

        #region drawing stuff

        public override string TrailShapeTex() => TrailTexturePath + "TrailShape_Wavy"; 

        public override string TrailShapeTex2() => TrailTexturePath + "TrailShape_Sharp";

        public override string TrailColorTex() => TrailTexturePath + "Colors/MediumPurple";

        public override string TrailColorTex2() => TrailTexturePath + "Colors/DarkPurple";

        public override float TrailAlpha(float factor) => base.TrailAlpha(factor) * 1.02f;

        public override BlendState TrailBlendState() => BlendState.Additive;

        public static SoundStyle SwingSound
        {
            get => SoundID.Item15 with { PitchVariance = 0.25f, MaxInstances = 0 };
        }

        public override void DrawSelf(SpriteBatch spriteBatch, Color lightColor, Vector4 diagonal = default, Vector2 drawScale = default, Texture2D glowTexture = null)
        {
            if (drawScale == default)
                drawScale = new Vector2(-0.12f, 1.15f);

            diagonal = new Vector4(0, 1, 1, 0);

            Vector2 drawCenter = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawVertexByTwoLine(Request<Texture2D>(Texture).Value, Color.Lerp(Color.Violet, Color.White, 0.95f), diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void DrawTrail(Color color)
        {
            //inner
            DrawTrailSection_Faded(TrailColorTex(), TrailShapeTex(), 0.99f, 0.2f, 0.99f);

            // outer
            DrawTrailSection(TrailColorTex(), TrailShapeTex(), 1.2f, 0.68f, 1.03f);

            //super bright outer
            DrawTrailSection(TrailTexturePath + "Colors/White", TrailShapeTex(), 2f, 0.87f, 1.02f);

            //faded
            DrawTrailSection(TrailColorTex2(), TrailShapeTex(), 0.5f, 0.6f, 1f);

            //blurry + glowy sigmaness
            VFXManager.DrawCache.Add(() =>
            {
                DrawTrailSection(TrailColorTex(), TrailShapeTex(), 5f, 0.94f, 0.98f);
            });
        }

        #endregion

        public override void End()
        {
            Projectile.Kill();
            Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = false;
        }

        public override void Attack()
        {
            void SwingEffects()
            {
                AttackSound(SwingSound with { PitchVariance = .13f, Pitch = -0.09f, Volume = 0.89f });
                ResetProjectileDamage();
            }

            void OnlyOnSecondSwing()
            {
                Vector2 velToMouse = NormalizeBetter(Main.MouseWorld - Projectile.Center) * 12f;

                Vector2 pos = Projectile.Center + velToMouse * 1.3f;

                Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, velToMouse, ProjectileType<SlashWave>(), (int)(Projectile.damage * 0.8f), 3f, Projectile.owner);

                //AttackSound(SoundID.Item60 with { Pitch = 0.1f, Volume = 0.2f });
                AttackSound(SoundID.Item45 with { Pitch = 0.05f, Volume = 0.15f });
                AttackSound(SoundID.Item130 with { Pitch = 0.05f, Volume = 0.25f });
            }

            maxAttackType = 1;

            useTrail = true;
            float t = 1f;

            if (attackType == 0)
            {
                float max = 13f;
                float end = 41f;

                if (timer < max)
                {
                    PrepareSwing(47f, PiOver2 - Player.direction * -0.85f, t, true, true, -12f);
                }

                EnactSwing(46f, max, end, MiscArray[1] + 0.09f, -0.21f, 1400f, t, [SwingEffects, null], -1, 4.2f);

                if (timer > end * t)
                    isAttacking = false;

                if (timer > end * t)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (attackType == 1)
            {
                float max = 14f;
                float end = 40f;

                if (timer < max)
                {
                    PrepareSwing(47f, -PiOver2 + Player.direction * -0.3f, t, true, true, -12f);
                }

                EnactSwing(49f, max, end, MiscArray[1] + 0.09f, 0.215f, 1410f, t, [SwingEffects, OnlyOnSecondSwing], max + 10, 4.1f);

                if (timer > end * t)
                    isAttacking = false;

                if (timer > end * t)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (!isAttacking)
                useTrail = false;

            if (isAttacking && Main.rand.NextBool(3))
            {
                Vector2 pos = Projectile.Center + (mainVec / 1.1f);
                Vector2 vel = (mainVec / 100f) + Main.rand.NextVector2Circular(1f, 1f);

                CreateDust(DustType<PurpurineDust>(), vel, pos, default, Main.rand.NextFloat(0.34f, 1f));
            }
        }

        public override void SafeOnHitNPC(NPC n, NPC.HitInfo info, int dmgDone)
        {
            for(int i = 0; i < Main.rand.Next(4, 10) + 5; i++)
            {
                Vector2 pos = n.Center + Main.rand.NextVector2Circular(n.width / 2f, n.height / 2f);
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) * (info.Crit ? 1.45f : 0.835f);

                CreateDust(DustID.GemAmethyst, vel, pos, default, Main.rand.NextFloat(0.34f, 1f));

                CreateDust(DustType<PurpurineDust>(), vel * Main.rand.NextFloat(1.5f, 4f), pos, default, Main.rand.NextFloat(0.34f, 1f));
            }
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

        //when you deal damage the color is changed to a new one!@!e32q342
        private int CombatText_NewText_Rectangle_Color_string_bool_bool(On_CombatText.orig_NewText_Rectangle_Color_string_bool_bool orig, Rectangle location, Color color, string text, bool dramatic, bool dot)
        {
            if (Strike > 0)
            {
                color = Color.Lerp(Color.Violet, Color.Purple, 0.3f);
                Strike--;
            }

            return orig(location, color, text, dramatic, dot);
        }

        public override void Unload()
        {
            On_CombatText.NewText_Rectangle_Color_string_bool_bool -= CombatText_NewText_Rectangle_Color_string_bool_bool;
        }
    }

    public class SlashWave : ModProjectile
    {
        public override string Texture => "MythosOfMoonlight/Assets/Textures/Trails/TrailShape_WavyRotated";

        public string Texture2 => "MythosOfMoonlight/Assets/Textures/Trails/TrailShape_SolidWavyRotated";

        public Color color = LBASMT(Color.Violet, Color.Lerp(Color.Violet, Color.Purple, 0.5f), 0.045f);

        private float alpha;

        private List<Vector2> vectors1 = [];
        private List<Vector2> vectors2 = [];

        public float randDir = 0f;

        private Vector2 proj_Back => Projectile.Center - Vector2.Normalize(Projectile.velocity.RotatedBy(Projectile.rotation)) * 5f;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 30;
            Projectile.scale = 0.09156f;

            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.DamageType = DamageClass.Melee;

            randDir = (Main.rand.NextBool() == true ? -1f : 1f);

            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        //sync the stuff that isnt synced by default :3

        public override void SendExtraAI(BinaryWriter writer)
        {
            Utils.WriteRGB(writer, color);
            writer.Write(Projectile.scale);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            color = Utils.ReadRGB(reader);
            Projectile.scale = reader.ReadSingle();
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 30)
            {
                int counts = 80;
                for (int i = (int)(-counts / 1.67f); i <= (counts / 1.67f); i++)
                {
                    Vector2 d = Vector2.Normalize(Projectile.velocity);
                    vectors1.Add(proj_Back - d * 15f * Projectile.scale + Utils.RotatedBy(d, -0.45f * i * (TwoPi * 0.96f * randDir) / (float)counts) * 120f * Projectile.scale - proj_Back);
                    vectors2.Add(proj_Back - d * 70f * Projectile.scale + Utils.RotatedBy(d, -0.45f * i * (TwoPi * 0.96f * randDir) / (float)counts) * 120f * Projectile.scale - proj_Back);
                }
            }

            if (Projectile.timeLeft > 20)
                alpha = Lerp(alpha, 1f, 0.1f);

            if (Projectile.timeLeft < 15)
            {
                if (alpha > 0f)
                    alpha -= 1f / 22f;

                else alpha = 0f;

                if (alpha < 0f)
                    alpha = 0f;
            }

            if (Projectile.timeLeft < 20)
                Projectile.velocity *= 0.9f;

            if (Projectile.timeLeft >= 25)
                Projectile.velocity *= 1.09998f;

            //Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.velocity.RotatedBy(Pi / 18f * randDir), 0.012f);

            Projectile.rotation = Lerp(Projectile.rotation, Projectile.rotation + (TwoPi * 0.39f), 0.2f);

            if (alpha > 0.4f)
            {
                for (int i = 0; i < vectors1.Count - 5; i += 5)
                {
                    Lighting.AddLight(proj_Back + Projectile.ai[0] * vectors1[i] * Utils.NextFloat(Main.rand, 0.4f, 1f), 2f * alpha * color.ToVector3());
                }
            }

            if (Projectile.timeLeft < 27)
            {
                for (int i = 0; i < (int)(Projectile.scale * 25f); i++)
                {
                    Vector2 pos = proj_Back + (vectors1[Main.rand.Next(vectors1.Count)] * Projectile.ai[0] * 0.96f) * Utils.NextFloat(Main.rand, 0.3f, 1.2f);
                    Vector2 vel = Projectile.velocity * -0.15f * Main.rand.NextFloat(0.35f, 0.6f);
                    CreateDust(DustType<PurpurineDust>(), vel * Main.rand.NextFloat(1.5f, 4f), pos, default, Main.rand.NextFloat(0.34f, 1f));
                }
            }

            Projectile.ai[0] = Lerp(Projectile.ai[0], 5f, 0.12f);

            if (Projectile.ai[1] == 1f)
            {
                alpha = Lerp(alpha, 0f, 0.2f);
                Projectile.velocity *= 0.9998f;

                if (alpha <= 0f)
                {
                    Projectile.Kill();
                }
            }

            if (stuff[1] == 1f)
            {
                Projectile.friendly = false;
                Projectile.ai[1] = 1f;
            }
        }

        /// <summary>
        /// 0: for hits
        /// 1: if hit 3 times set to 1, handles despawning the proj + no more damage!!!
        /// 2: unused for now
        /// </summary>
        float[] stuff = [0f, 0f, 0f];

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(0.95f * Projectile.damage);

            if (++stuff[0] >= 3)
                stuff[1] = 1f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = oldVelocity;
            Projectile.tileCollide = false;
            Projectile.ai[1] = 1f;
            Projectile.friendly = false; //no damag
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            for (int i = 0; i < vectors1.Count; i += 5)
            {
                if (Collision.CheckAABBvLineCollision(Utils.TopLeft(targetHitbox), Utils.Size(targetHitbox), proj_Back + 0.3f * Projectile.ai[0] * vectors1[i], proj_Back + Projectile.ai[0] * vectors1[i], 50f, ref point))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        { 
            void draweththeetrail()
            {
                for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type] + 1; i++)
                {
                    float t = (float)i / ProjectileID.Sets.TrailCacheLength[Projectile.type];

                    float y = t * 5f;

                    y /= 1f / (y + 1);

                    Draw(t * 0.65f, y * Projectile.velocity.Length() / 10f, (1.59f - t));
                }
            }

            //this is the slash wave from the saber

            draweththeetrail();

            VFXManager.DrawCache.Add(() =>
            {
                draweththeetrail();
               // Draw(1.2f, 1f, 1f);
            });

            MythosOfMoonlight.StarryDrawCache.Add(() =>
            {
                draweththeetrail();
                // Draw(1.2f, 1f, 1f);
            });

            Draw(1f, 1f, 1f);
            
            return false;
        }

        public void Draw(float opacity, float scaleMod, float sizeMod)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            List<VertexInfo> vertices = [];
            Color c = new Color();

            #region main

            //main wave
            for (int i = 0; i < vectors1.Count; i++)
            {
                float factor = i / (float)(vectors1.Count - 1);
                float a = 1.1f * alpha;
                float t = (float)Main.timeForVisualEffects * 0.045f;
                if (factor < 0.25)
                {
                    a = Lerp(0f, a, factor * 5.5f);
                }

                if (factor > 0.8f)
                {
                    a = Lerp(0f, a, (1f - factor) * 5.2f);
                }

                c = color;
                c.A = (byte)(c.A * a);

                vertices.Add(new VertexInfo(proj_Back + sizeMod * Projectile.ai[0] * vectors1[i] - Main.screenPosition, new Vector3(factor + t, 0f, 0f), c * opacity));
                vertices.Add(new VertexInfo(proj_Back + 0.5f * sizeMod * Projectile.ai[0] * vectors1[i] - Main.screenPosition, new Vector3(factor + t, 1f, 0f), c * opacity));
            }

            Main.graphics.GraphicsDevice.Textures[0] = Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad).Value;
            Main.graphics.GraphicsDevice.Textures[1] = Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad).Value;

            if (vertices.Count > 2)
            {
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
            }

            #endregion

            vertices.Clear();

            #region main2 - for trail

            Vector2 back3 = (Projectile.Center - Vector2.Normalize(Projectile.velocity) * 0.99f * scaleMod);

            //main wave
            for (int i = 0; i < vectors1.Count; i++)
            {
                float factor = i / (float)(vectors1.Count - 1);
                float a = 1.1f * alpha;

                if (factor < 0.25)
                {
                    a = Lerp(0f, a, factor * 5.5f);
                }

                if (factor > 0.8f)
                {
                    a = Lerp(0f, a, (1f - factor) * 5.2f);
                }

                c = color;
                c.A = (byte)(c.A * a);

                vertices.Add(new VertexInfo(back3 + sizeMod * Projectile.ai[0] * vectors1[i] - Main.screenPosition, new Vector3(factor, 0f, 0f), c * opacity));
                vertices.Add(new VertexInfo(back3 + 0.5f * sizeMod * Projectile.ai[0] * vectors1[i] - Main.screenPosition, new Vector3(factor, 1f, 0f), c * opacity));
            }

            Main.graphics.GraphicsDevice.Textures[0] = Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad).Value;
            Main.graphics.GraphicsDevice.Textures[1] = Request<Texture2D>(Texture, AssetRequestMode.AsyncLoad).Value;

            if (vertices.Count > 2)
            {
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
            }

            #endregion

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        }

        #region text color stuff

        public static int Strike = 0;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers mods)
        {
            Strike = 1;
        }

        public override void Load()
        {
            vectors1 ??= [];
            vectors2 ??= [];

            On_CombatText.NewText_Rectangle_Color_string_bool_bool += CombatText_NewText_Rectangle_Color_string_bool_bool;
        }

        //when you deal damage the color is changed to a new one!@!e32q342
        private int CombatText_NewText_Rectangle_Color_string_bool_bool(On_CombatText.orig_NewText_Rectangle_Color_string_bool_bool orig, Rectangle location, Color color, string text, bool dramatic, bool dot)
        {
            if (Strike > 0)
            {
                color = Color.Lerp(Color.Violet, Color.Purple, 0.3f);
                Strike--;
            }

            return orig(location, color, text, dramatic, dot);
        }

        public override void Unload()
        {
            vectors1?.Clear();
            vectors2?.Clear();

            On_CombatText.NewText_Rectangle_Color_string_bool_bool -= CombatText_NewText_Rectangle_Color_string_bool_bool;
        }

        #endregion
    }
}
