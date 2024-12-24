using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Common.Systems;
using MythosOfMoonlight.Common.Base;
using MythosOfMoonlight.Items.PurpleComet.Galactite;
using System.Threading;

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

    public class StarBatonP : SwordProjectile
    {
        public override string Texture => TryGetTextureFromOther<StarBaton>();

        public override void SafeSetDefaults()
        {
            maxAttackType = 1; //up, down?
            shaderType = 0;
            trailLength = 21;
            disFromPlayer = 5;
            Projectile.scale *= 1.12f;

            Projectile.extraUpdates = 1;
        }

        public override bool UseGlowmask => true;

        #region drawing stuff

        public override string TrailShapeTex() => TrailTexturePath + "TrailShape_Sharp";

        public override string TrailShapeTex2() => TrailTexturePath + "TrailShape_Sharp";

        public override string TrailColorTex() => TrailTexturePath + "Colors/Yellow";

        public override string TrailColorTex2() => TrailTexturePath + "Colors/PhotonicLight";

        public override float TrailAlpha(float factor) => base.TrailAlpha(factor) * 1.02f;

        public override BlendState TrailBlendState() => BlendState.Additive;

        public static SoundStyle SwingSound
        {
            get => SoundID.DD2_MonkStaffSwing with { Pitch = -0.25f, PitchVariance = 0.25f, MaxInstances = 0, Volume = 0.75f };
        }


        public override void DrawSelf(SpriteBatch spriteBatch, Color lightColor, Vector4 diagonal = default, Vector2 drawScale = default, Texture2D glowTexture = null)
        {
            if (drawScale == default)
                drawScale = new Vector2(-1.2f, 1.15f);

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

            DrawVertexByTwoLine(Request<Texture2D>(Texture + "_Glow").Value, Color.Lerp(Color.Transparent, Color.LightYellow, MiscArray[10] / 200f), diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);

            VFXManager.DrawCache.Add(() =>
            {
                DrawVertexByTwoLine(Request<Texture2D>(Texture + "_Glow").Value, Color.Lerp(Color.Transparent, Color.LightYellow, MiscArray[10] / 200f), diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);
            });
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void DrawTrail(Color color)
        {
            //////////front////////

            //inner
            DrawTrailSection_Faded(TrailColorTex(), TrailShapeTex(), 0.99f, 0.2f, 0.92f);

            // outer
            DrawTrailSection_Faded(TrailColorTex(), TrailShapeTex(), 1.2f, 0.68f, 0.91f);

            //faded
            DrawTrailSection(TrailColorTex2(), TrailShapeTex(), 0.5f, 0.6f, 0.9f);

            //blurry + glowy sigmaness
            VFXManager.DrawCache.Add(() =>
            {
                DrawTrailSection(TrailColorTex(), TrailShapeTex(), 5f, 0.87f, 0.92f);
            });

            /////////back//////

            //inner
            DrawTrailSection_Faded(TrailColorTex(), TrailShapeTex(), 0.99f, -0.2f, -0.8f);

            // outer
            DrawTrailSection_Faded(TrailColorTex(), TrailShapeTex(), 1.2f, -0.68f, -0.89f);

            //faded
            DrawTrailSection(TrailColorTex2(), TrailShapeTex(), 0.5f, -0.6f, -0.87f);

            //blurry + glowy sigmaness
            VFXManager.DrawCache.Add(() =>
            {
                DrawTrailSection(TrailColorTex(), TrailShapeTex(), 5f, -0.74f, -0.84f);
            });
        }

        #endregion

        public override void End()
        {
            Projectile.Kill();
            Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = false;
        }

        public override void AI()
        {
            if (Player.dead || !Player.active || Player.CCed || Player.noItems)
                End();

            Player.heldProj = Projectile.whoAmI;

            Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = true;

            //Miscarray[6]: for checking if the thing is being spun or thrown
            //7 is for time management while thrown

            if (MiscArray[6] == 0)
            {
                Projectile.Center = Player.Center + Utils.SafeNormalize(mainVec, Vector2.One) * disFromPlayer;
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, mainVec.ToRotation() - 1.57f);
            }

            isAttacking = false;

            Projectile.ownerHitCheck = !CanIgnoreTiles;
            Projectile.timeLeft++;

            timer++;

            useTrail = true;

            if (attackType == 0)
            {
                if (timer <= 40f)
                    PrepareSwing(60f, PiOver2 + Player.direction * 0.5f, 1f, true, true, -12f, 1f);
     
                if (timer > 40)
                {
                    if (Player.controlUseItem && MiscArray[6] == 0)
                    {
                        if (MiscArray[6] == 0) //double check!!
                        {
                            if (++MiscArray[8] >= 40)
                            {
                                AttackSound(SwingSound);
                                //ApplyScreenshake(Projectile.Center, .4f, 2f, 250f, 4);
                                ResetProjectileDamage();
                                //ModifyProjectileDamage(.15f);
                                MiscArray[8] = 0;
                            }

                            isAttacking = true;

                            Projectile.rotation += 0.12f * Projectile.spriteDirection;
                            mainVec = Projectile.rotation.ToRotationVector2() * 50f;

                            Player.direction = Projectile.direction;
                        }

                        MiscArray[10]++;

                        if (MiscArray[10] == 200f)
                        {

                        }
                    }

                    if (!Player.controlUseItem || MiscArray[6] == 1) //fly out and return now
                    {
                        if (MiscArray[6] == 0)
                        {
                            Vector2 tMouse = NormalizeBetter(Projectile.DirectionTo(Main.MouseWorld)) * 10f;

                            MiscArray[3] = tMouse.X;
                            MiscArray[4] = tMouse.Y;
                        }

                        MiscArray[6] = 1;

                        if (MiscArray[10] < 200f)
                            End();

                        if (++MiscArray[8] >= 30)
                        {
                            AttackSound(SwingSound);
                            ResetProjectileDamage();
                            ModifyProjectileDamage(.15f);
                            MiscArray[8] = 0;
                        }

                        Projectile.tileCollide = true;
                        Projectile.ignoreWater = false;
                        isAttacking = true;

                        CanIgnoreTiles = true;

                        Player player = Main.player[Projectile.owner];
                        Projectile.Center += Projectile.velocity;
                        useTrail = true;

                        Projectile.rotation += 0.16f * Projectile.spriteDirection;
                        mainVec = Projectile.rotation.ToRotationVector2() * 50f;

                        if (Projectile.Distance(Player.Center) > 2000f)
                            End();

                        if (++MiscArray[7] >= 130)
                        {
                            if (MiscArray[11] == 0f)
                            {
                                Vector2 toPlayer = NormalizeBetter(Projectile.DirectionTo(Player.Center)) * 11f;
                                MiscArray[3] = toPlayer.X;
                                MiscArray[4] = toPlayer.Y;

                            }

                            if (Projectile.Center.Distance(Player.Center) < 10f)
                            {
                                if (Player.controlUseItem)
                                {
                                    MiscArray.Clear();
                                    timer = 0;
                                }

                                else End();
                            }
                        }

                        LockPlayerDir(Player);

                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, new Vector2(MiscArray[3], MiscArray[4]), 0.1f);
                    }
                }
            }

            if (isAttacking)
                Player.direction = Projectile.spriteDirection;

            if (useTrail)
            {
                trailVecs.Enqueue(mainVec);
                if (trailVecs.Count > trailLength)
                    trailVecs.Dequeue();
            }

            else
            {
                trailVecs.Clear();
            }

            Projectile.friendly = isAttacking;

            if (isAttacking)
            {
                Vector2 pos1 = Projectile.Center + (mainVec * Main.rand.NextFloat(0.9f, 1.1f)); //front

                Vector2 pos2 = Projectile.Center - (mainVec * Main.rand.NextFloat(0.9f, 1.1f)); //back

                Vector2 vel = Player.velocity + mainVec / 200f;

                CreateDust(DustID.YellowStarDust, vel, pos1, default);
                CreateDust(DustID.YellowStarDust, vel, pos2, default);
            }
        }

        //account for the fact that the "center" has been shifted up a lot, and there are now 2 "blades" that can deal damage!!!!!!
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (isAttacking && Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), ProjCenter_WithoutGravDir + MainVec_WithoutGravDir * Projectile.scale * 0.11f, ProjCenter_WithoutGravDir + MainVec_WithoutGravDir * Projectile.scale))
                return true;

            if (isAttacking && Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), ProjCenter_WithoutGravDir - MainVec_WithoutGravDir * Projectile.scale * 0.11f, ProjCenter_WithoutGravDir - MainVec_WithoutGravDir * Projectile.scale))
                return true;

            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = height = 10;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            MiscArray[7] = 130f;
            return false;
        }

        public override void SafeOnHitNPC(NPC n, NPC.HitInfo info, int dmgDone)
        {
            for (int i = 0; i < Main.rand.Next(4, 10) + 5; i++)
            {
                Vector2 pos = n.Center + Main.rand.NextVector2Circular(n.width / 2f, n.height / 2f);
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) * (info.Crit ? 1.45f : 0.835f);

                CreateDust(DustID.YellowStarDust, vel, pos, default, Main.rand.NextFloat(0.34f, 1f));

                CreateDust(DustID.Enchanted_Gold, vel * Main.rand.NextFloat(1.5f, 4f), pos, default, Main.rand.NextFloat(0.64f, 1f));
            }

            ApplyScreenshake(n.Center,  1.8f, 2f, 500f, 4);
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
                color = Color.Lerp(Color.LightYellow, Color.Gold, 0.23f);
                Strike--;
            }

            return orig(location, color, text, dramatic, dot);
        }

        public override void Unload()
        {
            On_CombatText.NewText_Rectangle_Color_string_bool_bool -= CombatText_NewText_Rectangle_Color_string_bool_bool;
        }
    }

    /*

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

        Vector2 storedMousePos = Vector2.Zero;
        float distance = 0f;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active || player.ghost)
            {
                Projectile.Kill();
                return;
            }

            if (Mode == 0f)
            {
                SpinTimer++;

                if (SpinTimer % 21 == 0)
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

                if (SpinTimer >= 200f && !player.controlUseItem)
                {
                    Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld);
                    distance = Clamp(distance, 100f, Projectile.Distance(Main.MouseWorld));
                    Mode = 1f;
                }

                if(SpinTimer == 200f)
                {
                    Main.NewText("ready");
                }

                if (Projectile.owner == Main.myPlayer && !player.controlUseItem && SpinTimer < 190f)
                {
                    Projectile.Kill();
                    return;
                }
            }

            else
            {
                Main.NewText("guh");

                Charge++;

                float max = 200f;

                Projectile.timeLeft = 10;
                Projectile.velocity = NormalizeBetter(Projectile.velocity) * distance * Sin(Pi / max * Charge); //once  > 100 it should return

                if (Charge >= 400f)
                    Projectile.Kill();
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

            Projectile.SimpleDrawProjectile(Request<Texture2D>(Texture).Value, lightColor, false, 1f, 0f);
            Projectile.SimpleDrawProjectile(Request<Texture2D>(Texture + "_Glow").Value, Color.LightYellow, true, 1f, 0f);

            VFXManager.DrawCache.Add(() =>
            {
                Projectile.SimpleDrawProjectile(Request<Texture2D>(Texture + "_Glow").Value, Color.Lerp(Color.Transparent, Color.LightYellow, SpinTimer / 600f), true, 1f, 0f);
            });

            return false;
        }
    }

    */
}
