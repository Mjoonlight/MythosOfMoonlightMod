﻿using System;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ID;
using MythosOfMoonlight.Projectiles;
using Terraria.GameContent;
using Terraria.Audio;
using MythosOfMoonlight.Dusts;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Linq;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;
using MythosOfMoonlight.Common.Crossmod;
using MythosOfMoonlight.Items.Materials;
using Terraria.Utilities;
using MythosOfMoonlight.Common.Base;
using MythosOfMoonlight.Common.Systems;
using MythosOfMoonlight.Items.PurpleComet.IridicSet;
using System.Collections.Generic;

namespace MythosOfMoonlight.Items.PurpleComet.Galactite
{
    public class Estrella : ModItem
    {
        public override void SetDefaults()
        {
            Item.knockBack = 10f;
            Item.width = Item.height = 66;
            Item.crit = 5;
            Item.damage = 38;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.channel = true;
            //Item.reuseDelay = 45;
            Item.DamageType = DamageClass.Melee;
            //Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.LightRed;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<EstrellaP>();
        }
        int dir = 1;
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Helper.GetTex(Texture + "_Glow");
            spriteBatch.Reload(BlendState.Additive);
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
            spriteBatch.Reload(BlendState.AlphaBlend);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            dir = -dir;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, dir);
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

    public class EstrellaP : HeldSword
    {
        public override string Texture => "MythosOfMoonlight/Items/PurpleComet/Galactite/Estrella";
        public override string GlowTexture => "MythosOfMoonlight/Items/PurpleComet/Galactite/Estrella_Glow";

        public override void SetStaticDefaults()
        {
            Projectile.AddElement(CrossModHelper.Celestial);
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetExtraDefaults()
        {
            swingTime = 250;
            Projectile.Size = new(66);
            glowAlpha = 1f;
            BlendState _blendState = new BlendState();
            _blendState.AlphaSourceBlend = Blend.SourceAlpha;
            _blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

            _blendState.ColorSourceBlend = Blend.SourceAlpha;
            _blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
            glowBlend = _blendState;
            Projectile.extraUpdates = 4;
        }

        //what if i just
        //used the other sword thingy
        //but kept the starry dust n everything

        public override float Ease(float x)
        {
            return (float)(x == 0
  ? 0
  : x == 1
  ? 1
  : x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2
  : (2 - Math.Pow(2, -20 * x + 10)) / 2);
        }

        //what is this mess :skull:
        public override void ExtraAI()
        {
            Player player = Main.player[Projectile.owner];
            float rot = Projectile.rotation - MathHelper.PiOver4;
            Vector2 start = player.Center;
            Vector2 end = player.Center + rot.ToRotationVector2() * (Projectile.height + holdOffset * 0.25f);
            Vector2 offset = (Projectile.Size / 2) + ((Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * holdOffset * 0.25f);

            if (Projectile.timeLeft == 200)
                SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/estrellaOld") { PitchVariance = 0.3f, MaxInstances = 3 }, Projectile.Center);

            if (Projectile.ai[2].CloseTo(0.5f, 0.35f))
            {
                if (Projectile.timeLeft % 4 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 pos = Vector2.Lerp(start, end, Main.rand.NextFloat());
                        Dust.NewDustPerfect(pos, ModContent.DustType<PurpurineDust>(), Helper.FromAToB(pos, player.Center + Helper.FromAToB(player.Center, pos, false).RotatedBy(-Projectile.ai[1] * 0.5f)) * 5).noGravity = true;
                    }
                }

                for (float i = 0.1f; i < 4; i += 0.1f)
                {
                    Vector2 pos = Vector2.Lerp(start, end, i / 4);
                    Dust.NewDustPerfect(pos, ModContent.DustType<Starry>(), Helper.FromAToB(pos, player.Center + Helper.FromAToB(player.Center, pos, false).RotatedBy(-Projectile.ai[1] * 0.5f)) * 5, newColor: Color.Lerp(Color.Gray * 0.5f, Color.White, i / 4), Scale: (i / 3f) * 0.11f).noGravity = true;
                }
            }

            if (Projectile.timeLeft <= 50)
            {
                if (player.active && player.channel && !player.dead && !player.CCed && !player.noItems)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center);
                        Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), player.Center, dir, Projectile.type, Projectile.damage, Projectile.knockBack, player.whoAmI, 0, (-Projectile.ai[1]));
                        proj.rotation = Projectile.rotation;
                        proj.Center = Projectile.Center;
                        proj.timeLeft = 225;
                        Projectile.active = false;
                    }
                }
            }
        }

        public override void PreExtraDraw(float progress)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D tex = Helper.GetTex(Texture + "_Glow2"); //not used 
            Main.spriteBatch.Reload(BlendState.Additive);

            float s = 1;
            if (Projectile.oldPos.Length > 2)
            {
                Texture2D tex2 = Helper.GetTex("MythosOfMoonlight/Assets/Textures/Extra/Extra_209");
                Texture2D tex3 = Helper.GetTex("MythosOfMoonlight/Assets/Textures/Extra/seamlessNoise");

                VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[(Projectile.oldPos.Length - 1) * 6];

                if (Projectile.ai[2].CloseTo(0.5f, 0.3f))
                {
                    for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
                    {
                        if (Projectile.oldPos[i] != Vector2.Zero && Projectile.oldPos[i + 1] != Vector2.Zero)
                        {
                            Vector2 start = Projectile.oldPos[i];
                            Vector2 end = Projectile.oldPos[i + 1];

                            float num = Vector2.Distance(Projectile.oldPos[i], Projectile.oldPos[i + 1]);
                            Vector2 vector = (end - start) / num;

                            Color color = Color.Plum * s;
                            float off = 18.5f;
                           
                            Vector2 offset = (Projectile.Size / 2) + ((Projectile.rotation - PiOver4).ToRotationVector2() * off);
                            Vector2 pos1 = Projectile.oldPos[i] + offset - Main.screenPosition;
                            Vector2 pos2 = Projectile.oldPos[i + 1] + offset - Main.screenPosition;
                            Vector2 dir1 = Helper.GetRotation(Projectile.oldPos.ToList(), i) * off * s;
                            Vector2 dir2 = Helper.GetRotation(Projectile.oldPos.ToList(), i + 1) * off * (s + i / (float)Projectile.oldPos.Length * 0.03f);
                            
                            Vector2 v1 = pos1 + dir1;
                            Vector2 v2 = pos1 - dir1;
                            Vector2 v3 = pos2 + dir2;
                            Vector2 v4 = pos2 - dir2;

                            float p1 = i / (float)Projectile.oldPos.Length;
                            float p2 = (i + 1) / (float)Projectile.oldPos.Length;

                            vertices[i * 6] = Helper.AsVertex(v1, color, new Vector2(p1, Projectile.ai[1] != 1 ? 1 : 0));
                            vertices[i * 6 + 1] = Helper.AsVertex(v3, color, new Vector2(p2, Projectile.ai[1] != 1 ? 1 : 0));
                            vertices[i * 6 + 2] = Helper.AsVertex(v4, color, new Vector2(p2, Projectile.ai[1] == 1 ? 1 : 0));

                            vertices[i * 6 + 3] = Helper.AsVertex(v4, color, new Vector2(p2, Projectile.ai[1] == 1 ? 1 : 0));
                            vertices[i * 6 + 4] = Helper.AsVertex(v2, color, new Vector2(p1, Projectile.ai[1] == 1 ? 1 : 0));
                            vertices[i * 6 + 5] = Helper.AsVertex(v1, color, new Vector2(p1, Projectile.ai[1] != 1 ? 1 : 0));
                        }
                    }
                }

                Main.spriteBatch.SaveCurrent();
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
                Helper.DrawTexturedPrimitives(vertices, PrimitiveType.TriangleStrip, tex2);
                Main.spriteBatch.ApplySaved();
            }

            Main.spriteBatch.Reload(BlendState.AlphaBlend);
        }
        public override void OnSpawn(IEntitySource source)
        {
            //SoundEngine.PlaySound(SoundID.Item1);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 25; i++)
                Helper.SpawnDust(Projectile.Center, Projectile.Size, ModContent.DustType<PurpurineDust>(), Projectile.velocity);
            if (Projectile.ai[0] < 3)
            {
                Projectile.ai[0]++;
                for (int i = 0; i < 25; i++)
                {
                    Dust.NewDustPerfect(target.Center - Vector2.UnitY * 600, ModContent.DustType<Starry2>(), new Vector2(Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(4, 8)), newColor: Color.White).scale = Main.rand.NextFloat(0.07f, 0.16f);
                }
                for (int i = 0; i < 10; i++)
                    Dust.NewDustPerfect(target.Center, ModContent.DustType<PurpurineDust>(), Main.rand.NextVector2Circular(5, 5), newColor: Color.White).scale = Main.rand.NextFloat(0.05f, 0.16f);



                SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/estrellaImpact") { PitchVariance = 0.3f, MaxInstances = 3 }, Projectile.Center);
                //Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + Main.rand.NextVector2Circular(target.width / 2, target.height / 2), Helper.FromAToB(Projectile.Center, target.Center), ModContent.ProjectileType<EstrellaPImpact>(), 0, 0, Projectile.owner, target.whoAmI);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center - Vector2.UnitY * 700, Helper.FromAToB(target.Center - Vector2.UnitY * 500, target.Center) * Main.rand.NextFloat(4, 8f), ModContent.ProjectileType<EstrellaP2>(), Projectile.damage, 0, Projectile.owner, target.whoAmI);
            }
        }
        public override bool? CanDamage()
        {
            return (Projectile.ai[2].CloseTo(0.5f, 0.35f));
        }
    }

    public class EstrellaP2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            Projectile.AddElement(CrossModHelper.Celestial);
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 64;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 500;
            Projectile.penetrate = 3;
            Projectile.localNPCHitCooldown = 500;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide = false;

            ProjectileID.Sets.TrailCacheLength[Type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.NewProjectile(null, Projectile.Center + Helper.FromAToB(Projectile.Center, target.Center) * 20, Vector2.Zero, ModContent.ProjectileType<EstrellaPImpact>(), 0, 0);
            Projectile.ai[2] = 1;
            for (int i = 0; i < 7; i++)
                Helper.SpawnDust(Projectile.Center, Projectile.Size, ModContent.DustType<PurpurineDust>(), Projectile.velocity * 0.3f);
            for (int i = 0; i < 7; i++)
                Helper.SpawnDust(Projectile.Center, Projectile.Size, ModContent.DustType<PurpurineDust>(), Projectile.velocity.Length() * 0.3f * Main.rand.NextVector2Unit());
            for (int i = 0; i < 7; i++)
                Helper.SpawnDust(Projectile.Center, Projectile.Size, 58, Projectile.velocity.Length() * 0.3f * Main.rand.NextVector2Unit(), dustModification: new Action<Dust>((d) => { d.color = Color.Violet; }));

            for (int i = 0; i < 23; i++)
            {
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(2f, 2f);
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                for (int j = 0; j < 2; j++)
                    CreateDust(DustType<PurpurineDust>(), vel * Main.rand.NextFloat(4f, 9f), pos, Color.Violet, Main.rand.NextFloat(1.2f, 1.92f));
            }
        }

        public override void AI()
        {
            Helper.SpawnDust(Projectile.position, Projectile.Size, ModContent.DustType<PurpurineDust>(), Projectile.velocity * 0.5f);
           
            if (Projectile.frameCounter++ % 3 == 0)
            {
                if (Projectile.frame < 3)
                    Projectile.frame++;
                else
                    Projectile.frame = 0;
            }

            if (Projectile.timeLeft > 475 && Projectile.ai[2] == 0)
                Projectile.velocity = Projectile.velocity.Length() * Helper.FromAToB(Projectile.Center, Main.npc[(int)Projectile.ai[0]].Center + Main.npc[(int)Projectile.ai[0]].velocity);

            if (Projectile.velocity.Length() < 20f)
            {
                Projectile.velocity *= 1.15f;
            }

            Projectile.rotation += ToRadians(3);
           
            if (Projectile.timeLeft < 20)
                alpha -= 0.05f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = Projectile.Center + new Vector2(Main.rand.NextFloat(-5f, 6f), 0f);
                Vector2 vel = -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.25f);

                var d = CreateDust(DustType<StarryStretchyGlow>(), vel, pos, Color.White, Main.rand.NextFloat(2f, 4.2f));
                d.customData = new Vector2(1f, Main.rand.NextFloat(0.3f, 0.6f));
            }

            if (Projectile.Center.Y > Main.player[Projectile.owner].Center.Y)
                Projectile.tileCollide = true;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 10;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.NewProjectile(null, Projectile.Center + Helper.FromAToB(Projectile.Center, Projectile.Center + new Vector2(0f, -15f)) * 20, Vector2.Zero, ProjectileType<EstrellaPImpact>(), 0, 0);
            Projectile.ai[2] = 1;
            for (int i = 0; i < 7; i++)
                Helper.SpawnDust(Projectile.Center, Projectile.Size, ModContent.DustType<PurpurineDust>(), Projectile.velocity * 0.3f);
            for (int i = 0; i < 7; i++)
                Helper.SpawnDust(Projectile.Center, Projectile.Size, ModContent.DustType<PurpurineDust>(), Projectile.velocity.Length() * 0.3f * Main.rand.NextVector2Unit());
            for (int i = 0; i < 7; i++)
                Helper.SpawnDust(Projectile.Center, Projectile.Size, 58, Projectile.velocity.Length() * 0.3f * Main.rand.NextVector2Unit(), dustModification: new Action<Dust>((d) => { d.color = Color.Violet; }));

            for (int i = 0; i < 23; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(2f, 2f);
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                for (int j = 0; j < 2; j++)
                    CreateDust(DustType<PurpurineDust>(), vel * Main.rand.NextFloat(4f, 9f), pos, Color.Violet, Main.rand.NextFloat(1.2f, 1.92f));
            }

            return base.OnTileCollide(oldVelocity);
        }

        public static int Strike = 0;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Strike = 1;

            modifiers.DefenseEffectiveness *= 0.75f; //ignore defense (25%)??
        }

        float alpha = 1, animationOffset;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Helper.GetTex(Texture + "Star");
            Texture2D tex3 = TextureAssets.Projectile[Type].Value;

            animationOffset -= 0.05f;

            if (animationOffset <= 0)
                animationOffset = 1;

            animationOffset = Clamp(animationOffset, float.Epsilon, 1 - float.Epsilon);

            List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>(ProjectileID.Sets.TrailCacheLength[Type]);
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || i == 0) continue;
                float mult = 1f - (1f / Projectile.oldPos.Length) * i;
                mult *= mult;

                float __off = animationOffset;
                if (__off > 1) __off = -__off + 1;
                float _off = __off + (float)i / Projectile.oldPos.Length;

                if (mult > 0)
                {
                    vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + (Projectile.velocity.ToRotation()).ToRotationVector2() * 6 - Main.screenPosition + Projectile.Size / 2 + new Vector2(38f * mult, 30f).RotatedBy(Helper.FromAToB(Projectile.oldPos[i], Projectile.oldPos[i + 1]).ToRotation() + MathHelper.PiOver2), Color.Orchid * mult * 1f, new Vector2(_off, 0)));
                    vertices.Add(Helper.AsVertex(Projectile.oldPos[i] + (Projectile.velocity.ToRotation()).ToRotationVector2() * 6 - Main.screenPosition + Projectile.Size / 2 + new Vector2(38f * mult, -30f).RotatedBy(Helper.FromAToB(Projectile.oldPos[i], Projectile.oldPos[i + 1]).ToRotation() - MathHelper.PiOver2), Color.Orchid * mult * 1f, new Vector2(_off, 1)));
                }
            }

            Main.spriteBatch.SaveCurrent();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            if (vertices.Count > 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Helper.GetExtraTex("Extra/trail_01"), false);
                    Helper.DrawTexturedPrimitives(vertices.ToArray(), PrimitiveType.TriangleStrip, Helper.GetExtraTex("Extra/Ex1_backup"), false);
                }
            }

            Main.spriteBatch.ApplySaved();

            Main.spriteBatch.Draw(tex3, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * 66, 40, 66), Color.White * alpha, Projectile.velocity.ToRotation() - MathHelper.PiOver2, Projectile.Size / 2, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0, 12) - Main.screenPosition, null, Color.White * alpha * 0.75f, Projectile.rotation - MathHelper.PiOver2, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * alpha;
        }

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

    public class EstrellaPSlice : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Projectile.AddElement(CrossModHelper.Celestial);
        }
        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 500;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }
        public override bool? CanDamage() => false;
        public override bool ShouldUpdatePosition() => false;
        public override string Texture => "MythosOfMoonlight/Assets/Textures/Extra/blank";
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Helper.GetTex("MythosOfMoonlight/Assets/Textures/Extra/slash");
            float alpha = MathHelper.Lerp(1, 0, Projectile.ai[0]);
            for (int i = 0; i < 2; i++)
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Black * 0.5f * alpha, Projectile.rotation, tex.Size() / 2, new Vector2(Projectile.ai[0], 1 + alpha * 0.1f) * 0.35f * 2, SpriteEffects.None, 0);
            Main.spriteBatch.Reload(BlendState.Additive);
            for (int i = 0; i < 2; i++)
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.DarkViolet * alpha, Projectile.rotation, tex.Size() / 2, new Vector2(Projectile.ai[0], 1 + alpha * 0.1f) * 0.45f * 2, SpriteEffects.None, 0);
            Main.spriteBatch.Reload(BlendState.AlphaBlend);
            return false;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.ai[0] += 0.04f;
            if (Projectile.ai[0] > 1)
                Projectile.Kill();
        }
    }

    public class EstrellaPImpact : ModProjectile
    {
        public override string Texture => "MythosOfMoonlight/Assets/Textures/Extra/blank";
        public override void SetStaticDefaults()
        {
            Projectile.AddElement(CrossModHelper.Celestial);
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 19;
            Projectile.scale = 0;
            Projectile.tileCollide = false;
        }
        public override bool? CanDamage()
        {
            return false;
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        int seed;
        public override void OnSpawn(IEntitySource source)
        {
            seed = Main.rand.Next(int.MaxValue);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Helper.GetTex("MythosOfMoonlight/Assets/Textures/Extra/cone4");
            Texture2D tex2 = Helper.GetTex("MythosOfMoonlight/Assets/Textures/Extra/star_05");
            Main.spriteBatch.Reload(BlendState.Additive);
            UnifiedRandom rand = new UnifiedRandom(seed);
            float max = 40;
            if (lightColor == Color.Transparent)
            {

                float alpha = MathHelper.Lerp(0.5f, 0, Projectile.ai[1]) * 2;
                for (float i = 0; i < max; i++)
                {
                    float angle = Helper.CircleDividedEqually(i, max);
                    float scale = rand.NextFloat(0.2f, 1f);
                    Vector2 offset = new Vector2(Main.rand.NextFloat(50) * Projectile.ai[1] * scale, 0).RotatedBy(angle);
                    for (float j = 0; j < 2; j++)
                        Main.spriteBatch.Draw(tex, Projectile.Center + offset - Main.screenPosition, null, Color.DarkViolet * alpha * 0.5f, angle, new Vector2(0, tex.Height / 2), new Vector2(Projectile.ai[1], alpha) * scale, SpriteEffects.None, 0);
                }

                Main.spriteBatch.Draw(tex2, Projectile.Center - Main.screenLastPosition, null, Color.White * Projectile.scale * 2, Main.GameUpdateCount * -0.025f, tex2.Size() / 2, Projectile.scale * 0.5f, SpriteEffects.None, 0);
            }
            Main.spriteBatch.Reload(BlendState.AlphaBlend);
            return false;
        }
        public override void AI()
        {
            float progress = Utils.GetLerpValue(0, 20, Projectile.timeLeft);
            Projectile.scale = MathHelper.Clamp((float)Math.Sin(progress * MathHelper.Pi) * 0.5f, 0, 0.5f);
            Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 1, 0.1f);
        }
    }

    public class EstrellaR : ModItem
    {
        public override string Texture => TryGetTextureFromOther<Estrella>();

        public override void SetDefaults()
        {
            Item.knockBack = 10f;
            Item.width = Item.height = 66;
            Item.crit = 5;
            Item.damage = 38;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.channel = true;
            //Item.reuseDelay = 45;
            Item.DamageType = DamageClass.Melee;
            //Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.LightRed;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<EstrellaProj2>();
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Helper.GetTex(Texture + "_Glow");
            spriteBatch.Reload(BlendState.Additive);
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
            spriteBatch.Reload(BlendState.AlphaBlend);
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

    public class EstrellaProj2 : SwordProjectile
    {
        public override string Texture => TryGetTextureFromOther<Estrella>();

        public override void SafeSetDefaults()
        {
            maxAttackType = 1; //up, down?
            shaderType = 0;
            trailLength = 15;
            disFromPlayer = 5;
            Projectile.scale *= 1.12f;

            Projectile.extraUpdates = 1;
        }

        public override bool UseGlowmask => true;

        #region drawing stuff

        public override string TrailShapeTex() => TrailTexturePath + "TrailShape_Sharp";

        public override string TrailShapeTex2() => TrailTexturePath + "TrailShape_Sharp";

        public override string TrailColorTex() => TrailTexturePath + "Colors/MediumPurple";

        public override string TrailColorTex2() => TrailTexturePath + "Colors/DarkPurple";

        public override float TrailAlpha(float factor) => base.TrailAlpha(factor) * 1.02f;

        public override BlendState TrailBlendState() => BlendState.Additive;

        public static SoundStyle SwingSound
        {
            get => new SoundStyle("MythosOfMoonlight/Assets/Sounds/estrella") with { Pitch = -0.25f, PitchVariance = 0.25f, MaxInstances = 0, Volume = 0.15f };
        }

        public static SoundStyle SwingSound2
        {
            get => new SoundStyle("MythosOfMoonlight/Assets/Sounds/estrellaOld") with { PitchVariance = 0.25f, MaxInstances = 0 };
        }

        public override void DrawSelf(SpriteBatch spriteBatch, Color lightColor, Vector4 diagonal = default, Vector2 drawScale = default, Texture2D glowTexture = null)
        {
            if (drawScale == default)
                drawScale = new Vector2(-0.01f, 1.15f);

            diagonal = new Vector4(0, 1, 1, 0);

            Vector2 drawCenter = Projectile.Center - Main.screenPosition;

            BlendState _blendState = new BlendState();
            _blendState.AlphaSourceBlend = Blend.SourceAlpha;
            _blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

            _blendState.ColorSourceBlend = Blend.SourceAlpha;
            _blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, _blendState, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
           
            DrawVertexByTwoLine(Request<Texture2D>(Texture).Value, lightColor, diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);
            DrawVertexByTwoLine(Request<Texture2D>(Texture + "_Glow").Value, Color.White, diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void DrawTrail(Color color)
        {
            //inner
            DrawTrailSection_Faded(TrailColorTex(), TrailShapeTex(), 0.99f, 0.2f, 0.99f);

            // outer
            DrawTrailSection_Faded(TrailColorTex(), TrailShapeTex(), 1.2f, 0.68f, 1.03f);

            //faded
            DrawTrailSection(TrailColorTex2(), TrailShapeTex(), 0.5f, 0.6f, 1f);

            //guh
            MythosOfMoonlight.StarryDrawCache.Add(() =>
            {
                DrawTrailSection(TrailColorTex(), TrailTexturePath + "TrailShape_White", 3.5f, 0.12f, 0.98f);
            });

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

        bool b = false;

        public override void Attack()
        {
            //Projectile.Center = Player.Center + Utils.SafeNormalize(mainVec, Vector2.One) * disFromPlayer + ((Projectile.direction == -1) ? new Vector2(3f, -3f) : new Vector2(-9f, 2f));

            void SwingEffects()
            {
                AttackSound(SwingSound2);
                AttackSound(SwingSound);
                ResetProjectileDamage();
                ApplyScreenshake(Projectile.Center, 1f, 2f, 1000f, 4);
            }

            void SwingEffectsHeavy()
            {
                AttackSound(SwingSound2);
                AttackSound(SwingSound);
                ResetProjectileDamage();
                ModifyProjectileDamage(0.4f);
                ApplyScreenshake(Projectile.Center, 6f, 2f, 1000f, 4);
            }

            maxAttackType = 3;

            useTrail = true;
            float t = 1f;

            /*

            if (attackType == 0)
            {
                float max = 30f;
                float max2 = 50f;
                float end = 75f;

                if (timer < max)
                {
                    PrepareSwing(78f, PiOver2 - Player.direction * -0.85f, t, true, true, -12f);
                }

                if (timer > max && timer < max2)
                {
                    PrepareSwing(79f, PiOver2 - Player.direction * -0.67f, t, true, true, -12f);
                }

                EnactSwing(82f, max2, end, MiscArray[1] + 0.09f, -0.21f, 1400f, t, [SwingEffects, null], -1, -12f);

                if (timer > end)
                    isAttacking = false;

                if (timer > end)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (attackType == 1)
            {
                float max = 30f;
                float max2 = 50f;
                float end = 81f;

                float t2 = (timer < max2 + 10 || timer > end - 18) ? 1f : 1.15f;

                if (timer < max)
                {
                    PrepareSwing(78f, PiOver2 + Player.direction * 0.85f, t, true, true, -12f);
                }

                if (timer > max && timer < max2)
                {
                    PrepareSwing(79f, PiOver2 + Player.direction * 0.67f, t, true, true, -12f);
                }

                EnactSwing(80f, max2, end, MiscArray[1] + 0.09f, 0.223f * Lerp(t2, 1f, 0.2f), 1400f, t, [SwingEffects, null], -1, -12f);

                if (timer > end)
                    isAttacking = false;

                if (timer > end)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            */

            //combo:

            if (attackType == 0)
            {
                float max = 30f;
                float max2 = 60f;
                float end = 85f;

                float t2 = (timer < end - 10 && timer > max2 + 10) ? 1.2f : 0.925f;

                if (timer < max)
                {
                    PrepareSwing(88f, PiOver2 - Player.direction * -0.85f, t, true, true, -12f, b == false ? 1f : -1f);
                }

                if (timer > max && timer < max2)
                {
                    PrepareSwing(89f, PiOver2 - Player.direction * -0.72f, t, true, true, -12f);
                    b = true;
                }

                EnactSwing(92f, max2, end, MiscArray[1] + 0.09f, -0.21f * t2, 1400f, t, [SwingEffects, null], -1, -12f);

                if (timer > end)
                    isAttacking = false;

                if (timer > end)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (attackType == 1)
            {
                float max = 50f;
                float max2 = 70f;
                float end = 105f;

                float t2 = (timer < max2 + 10 || timer > end - 10) ? 1f : 1.25f;

                if (timer < max)
                {
                    PrepareSwing(88f, PiOver2 + Player.direction * 0.85f, t, true, true, -12f);
                }

                if (timer > max && timer < max2)
                {
                    PrepareSwing(89f, PiOver2 + Player.direction * 0.67f, t, true, true, -12f);
                }

                EnactSwing(92f, max2, end, MiscArray[1] + 0.23f, 0.21f, 400f, t, [SwingEffects, null], -1, -13.2f);

                if (timer > end)
                    isAttacking = false;

                if (timer > end)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (attackType == 2)
            {
                float max = 40f;
                float max2 = 50f;
                float end = 108f;

                float t2 = (timer < max2 + 10 || timer > end - 15) ? 1f : 1.155f;

                if (timer < max)
                {
                    PrepareSwing(88f, PiOver2 - Player.direction * -0.95f, t, true, true, -12f);
                }

                if (timer > max && timer < max2)
                {
                    PrepareSwing(89f, PiOver2 - Player.direction * -0.67f, t, true, true, -12f);
                }

                EnactSwing(92f, max2, end, MiscArray[1] + 0.09f, 0.21f * t2, 1400f, t, [SwingEffects, SwingEffects], 76f, -15.2f);

                if (timer > end)
                    isAttacking = false;

                if (timer > end)
                {
                    ResetProjectileDamage();
                    NextAttackType();
                }
            }

            if (attackType == 3)
            {
                float max = 60f;
                float max2 = 80f;
                float end = 105f;

                float t2 = (timer < max2 + 10 || timer > end - 10) ? 1f : 1.125f;

                if (timer < max)
                {
                    PrepareSwing(90f, PiOver2 + Player.direction * 0.5f, t, true, true, -12f);
                }

                if (timer > max && timer < max2)
                {
                    PrepareSwing(97f, PiOver2 + Player.direction * 0.6f, t, true, true, -12f);
                }

                EnactSwing(108f, max2, end, MiscArray[1] + 0.23f, 0.27f, 400f, t, [SwingEffectsHeavy, null], -1, -13.2f);

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

            if (isAttacking)
            {
                Vector2 pos = Projectile.Center + (mainVec / 1.1f) * Main.rand.NextFloat(0.8f, 1.01f);
                Vector2 vel = (mainVec / 100f) + Main.rand.NextVector2Circular(1f, 1f) + Player.velocity;

                //CreateDust(DustType<Starry2>(), vel, pos, Color.White, Main.rand.NextFloat(0.08f, 0.117f));
            }
        }

        public override void SafeOnHitNPC(NPC n, NPC.HitInfo info, int dmgDone)
        {
            for (int i = 0; i < Main.rand.Next(4, 10) + 5; i++)
            {
                Vector2 pos = n.Center + Main.rand.NextVector2Circular(n.width / 2f, n.height / 2f);
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) * (info.Crit ? 1.45f : 0.835f);

                CreateDust(DustID.GemAmethyst, vel, pos, default, Main.rand.NextFloat(0.34f, 1f));

                CreateDust(DustType<PurpurineDust>(), vel * Main.rand.NextFloat(1.5f, 4f), pos, default, Main.rand.NextFloat(0.64f, 1f));
            }

            for (int i = 0; i < 8; i++) //this could be good or bad
            {
                Vector2 pos = n.Center + Main.rand.NextVector2Circular(n.width / 2f, n.height / 2f);
                Vector2 vel = new Vector2(mainVec.X / 100f * -Projectile.direction, mainVec.Y / 100f * -Projectile.direction).RotatedBy(Projectile.rotation) * (info.Crit ? 15f : 8.35f);

                //var d = CreateDust(DustType<StarryStretchyGlow>(), (vel * Main.rand.NextFloat(1.5f, 4f)).RotatedByRandom(Pi * 0.3f), pos, Color.White, Main.rand.NextFloat(1.9f, 3f));
                //d.customData = new Vector2(1f, Main.rand.NextFloat(0.4f, 0.6f));
            }

            if (!HitNPC && attackType == 3)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angleD = Main.rand.NextFloat(TwoPi);
                    Vector2 posD = n.Center - angleD.ToRotationVector2() * Main.rand.NextFloat(10f, 15f);
                    Vector2 velD = angleD.ToRotationVector2() * Main.rand.NextFloat(14f, 18f);
                    Vector2 velD2 = (angleD + PiOver2).ToRotationVector2() * Main.rand.NextFloat(14f, 18f);

                    Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), posD, i == 0 ? velD : velD2, ProjectileType<EstrellaSlashProj>(), (Projectile.damage / 2), 0f, Projectile.owner, 0f, -0.2f, 0f);
                }

                HitNPC = true;
            }

            if (Player.ownedProjectileCounts[ProjectileType<EstrellaP2>()] < 3 && n.type != NPCID.TargetDummy && !n.CountsAsACritter)
            {

                SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/estrellaImpact") { PitchVariance = 0.3f, MaxInstances = 3 }, Projectile.Center);
                SpawnProjectle(Player, ProjectileType<EstrellaP2>(), n.Center - Vector2.UnitY * 700, Helper.FromAToB(n.Center - Vector2.UnitY * 500, n.Center) * Main.rand.NextFloat(4, 8f), Projectile.damage * 0.6f, 7f, Player.ownedProjectileCounts[ProjectileType<EstrellaP2>()] < 3, n.whoAmI);
            }
            ApplyScreenshake(n.Center, (attackType == 3) ? 20f : 8f, 2f, 500f, 10);
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

    class EstrellaSlashProj : ModProjectile
    {
        private float yscale = 0.6f;

        public override string Texture => TryGetTextureFromOther<Estrella>();

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 16;
            Projectile.tileCollide = false;
            Projectile.scale = 0.238f;
            Projectile.alpha = 30;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Utils.ToRotation(Projectile.velocity) + (float)Math.PI / 2f;

            Projectile.scale = 0.4f;

            if (Projectile.timeLeft > 8)
                yscale += 0.6f;

            else
            {
                yscale -= 1f;
                Projectile.alpha += 10;
            }

            Projectile.velocity *= 0.7f;

            //needs tweaking: make dust stick closer together!!!!

            Dust d = Dust.NewDustDirect(Projectile.Center + Projectile.velocity * Main.rand.NextFloatDirection(), 0, 0, DustType<Starry2>());
            d.velocity = Projectile.velocity * -1f * 0.43f * 1.2f;
           // d.velocity = d.velocity.RotatedBy(PiOver2);
            d.color = Color.White;
            d.noGravity = true;
            d.scale = 0.2f * Projectile.scale;

            Dust d2 = Dust.NewDustDirect(Projectile.Center + Projectile.velocity * Main.rand.NextFloatDirection(), 0, 0, DustType<Starry2>());
            d2.velocity = Projectile.velocity * 1f * 0.43f * 1.2f;
           // d.velocity = d.velocity.RotatedBy(PiOver2);
            d2.color = Color.White;
            d2.noGravity = true;
            d2.scale = 0.2f * Projectile.scale;

            Dust d3 = Dust.NewDustDirect(Projectile.Center + Projectile.velocity * Main.rand.NextFloatDirection(), 0, 0, DustType<Starry2>());
            d3.velocity = Projectile.velocity * Main.rand.NextFloatDirection() * 0.3f;
           // d.velocity = d.velocity.RotatedBy(PiOver2);
            d3.color = Color.White;
            d3.noGravity = true;
            d3.scale = 0.226f * Projectile.scale;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Strike = 1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.8f);
        }

        //messy as HELL oh my GYATT
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Extra[98].Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            //outer, darker part
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Violet /*with { A = 0 }*/, Projectile.rotation, Utils.Size(tex) / 2f, new Vector2(Projectile.scale * (Projectile.ai[0] == 1 ? 1.12f : 1.1f), yscale * (Projectile.ai[0] == 1 ? 1.12f : 1f)), 0, 0f);

            //middle, brighter part
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Purple, Color.White, 0.7f) /*with { A = 0 } */, Projectile.rotation, Utils.Size(tex) / 2f, new Vector2(Projectile.scale * 0.75f * (Projectile.ai[0] == 1 ? 1.1f : 1.05f), yscale * 0.75f * (Projectile.ai[0] == 1 ? 1.1f : 1f)), 0, 0f);

            //inner, white part
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White /*with { A = 0 } */, Projectile.rotation, Utils.Size(tex) / 2f, new Vector2(Projectile.scale * 0.45f * (Projectile.ai[0] == 1 ? 1.12f : 1.05f), yscale * 0.45f * (Projectile.ai[0] == 1 ? 1.12f : 1f)), 0, 0f);

            VFXManager.DrawCache.Add(() =>
            {
                //middle, brighter part
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Purple, Color.White, 0.7f) /*with { A = 0 } */, Projectile.rotation, Utils.Size(tex) / 2f, new Vector2(Projectile.scale * 0.75f * (Projectile.ai[0] == 1 ? 1.1f : 1.05f), yscale * 0.75f * (Projectile.ai[0] == 1 ? 1.1f : 1f)), 0, 0f);

                //inner, white part
                //Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White /*with { A = 0 } */, Projectile.rotation, Utils.Size(tex) / 2f, new Vector2(Projectile.scale * 0.45f * (Projectile.ai[0] == 1 ? 1.12f : 1.05f), yscale * 0.45f * (Projectile.ai[0] == 1 ? 1.12f : 1f)), 0, 0f);
            });

            MythosOfMoonlight.StarryDrawCache.Add(() =>
            {
                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Purple, Color.White, 0.7f) /*with { A = 0 } */, Projectile.rotation, Utils.Size(tex) / 2f, new Vector2(Projectile.scale * 0.75f * (Projectile.ai[0] == 1 ? 1.1f : 1.05f), yscale * 0.75f * (Projectile.ai[0] == 1 ? 1.1f : 1f)), 0, 0f);
            });

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 vec = NormalizeBetter(Projectile.velocity) * (float)Projectile.height * yscale;
            return Collision.CheckAABBvLineCollision(Utils.TopLeft(targetHitbox), Utils.Size(targetHitbox), Projectile.Center - vec, Projectile.Center + vec, (float)Projectile.width * Projectile.scale * 2f, ref point);
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
}