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
using Terraria.WorldBuilding;

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

        float dmgBoostWhenOut = 0.2f;

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
                    PrepareSwing(36f, PiOver2 + Player.direction * 0.5f, 1f, true, true, -12f, 1f);
     
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
                                ModifyProjectileDamage(dmgBoostWhenOut / 3f);
                                MiscArray[8] = 0;
                            }

                            isAttacking = true;

                            Projectile.rotation += 0.12f * Projectile.spriteDirection;
                            mainVec = Projectile.rotation.ToRotationVector2() * 30f;

                            Player.direction = Projectile.direction;
                        }

                        MiscArray[10]++;

                        if (MiscArray[10] == 200f)
                        {
                            for (int i = 0; i < 40; i++)
                            {
                                float rotation = TwoPi * i / 40;

                                Vector2 vel = rotation.ToRotationVector2() * ((i % 3 == 0) ? 0.5f : 1.5f);

                                Vector2 pos = Player.Center + vel * 15f;

                                CreateDust(DustID.YellowStarDust, (vel * 2.5f).RotatedBy(Sin(vel.X)), pos, default);
                            }

                            SoundEngine.PlaySound(SoundID.NPCHit5 with { Pitch = -0.23f, Volume = 0.6f }, Player.Center);
                        }
                    }

                    //todo: make it decelerate and then return rather than instantly return!!!

                    if (!Player.controlUseItem || MiscArray[6] == 1) //fly out and return now
                    {
                        if (MiscArray[6] == 0)
                        {
                            Vector2 tMouse = NormalizeBetter(Projectile.DirectionTo(Main.MouseWorld)) * 7f;

                            MiscArray[3] = tMouse.X;
                            MiscArray[4] = tMouse.Y;
                        }

                        MiscArray[6] = 1;

                        if (MiscArray[10] < 200f) //fixed!!!!!
                            End();

                        if (++MiscArray[8] >= 30)
                        {
                            AttackSound(SwingSound);
                            ResetProjectileDamage();
                            ModifyProjectileDamage(dmgBoostWhenOut, 1f);
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
                        mainVec = Projectile.rotation.ToRotationVector2() * 30f;

                        if (Projectile.Distance(Player.Center) > 2000f)
                            End();

                        if (MiscArray[7] >= 63f && MiscArray[7] <= 80f) //slow down before returning
                        {
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.12f);
                        }

                        if (++MiscArray[7] > 80) //return
                        {
                            if (MiscArray[11] == 0f)
                            {
                                Vector2 toPlayer = NormalizeBetter(Projectile.DirectionTo(Player.Center)) * 7.5f;
                                MiscArray[3] = toPlayer.X;
                                MiscArray[4] = toPlayer.Y;

                            }

                            if (Projectile.Center.Distance(Player.Center) < 10f)
                            {
                                if (Player.controlUseItem)
                                {
                                    MiscArray.Clear();
                                    timer = 30;
                                    Projectile.velocity = Vector2.Zero;
                                    dmgBoostWhenOut += 0.08f;

                                    for (int i = 0; i < 30; i++)
                                    {
                                        float rotation = TwoPi * i / 30;

                                        Vector2 vel = rotation.ToRotationVector2() * ((i % 3 == 0) ? 0.5f : 1.5f);

                                        Vector2 pos = Player.Center + vel * 15f;

                                        CreateDust(DustID.YellowStarDust, (vel * 2.5f).RotatedBy(Sin(vel.X)), pos, default);
                                    }
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

                //Color dustColor = Color.Lerp(Color.Blue, Color.LightYellow, MiscArray[10] / 200f);

                int dustType = ((MiscArray[10] / 200f) >= 1f) ? DustID.YellowStarDust : DustID.GemSapphire;

                CreateDust(dustType, vel, pos1, default);
                CreateDust(dustType, vel, pos2, default);
            }
        }

        //account for the fact that the "center" has been shifted up a lot, and there are now 2 parts that can deal damage!!!!!!
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
            width = height = 4;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (MiscArray[6] != 0f)
                MiscArray[7] = 130f;

            return false;
        }

        public override void SafeOnHitNPC(NPC n, NPC.HitInfo info, int dmgDone)
        {
            for (int i = 0; i < Main.rand.Next(4, 10) + 5; i++)
            {
                Vector2 pos = n.Center + Main.rand.NextVector2Circular(n.width / 2f, n.height / 2f);
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) * (info.Crit ? 1.45f : 0.835f);

                CreateDust(DustID.YellowStarDust, vel, pos, default, Main.rand.NextFloat(1f, 1.6f));

                CreateDust(DustID.Enchanted_Gold, vel * Main.rand.NextFloat(1.5f, 4f), pos, default, Main.rand.NextFloat(0.9f, 1f));
            }

            ApplyScreenshake(n.Center, 0.95f, 1f, 200f, 3);

            if (MiscArray[6] != 0f)
                MiscArray[7] = 130f;
        }

        public override void SafeModifyHitNPC(NPC n, ref NPC.HitModifiers hitMods)
        {
            Strike = 1;

            if (MiscArray[6] != 0f)
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
}
