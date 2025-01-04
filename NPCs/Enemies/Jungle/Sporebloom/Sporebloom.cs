using log4net.Util;
using MythosOfMoonlight.Common.Crossmod;
using MythosOfMoonlight.Common.Graphics.MoMParticles;
using MythosOfMoonlight.Common.Graphics.MoMParticles.Types;
using MythosOfMoonlight.Common.Systems;
using MythosOfMoonlight.Common.Utilities;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Items.Jungle;
using MythosOfMoonlight.NPCs.Enemies.Jungle.Vivine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;

namespace MythosOfMoonlight.NPCs.Enemies.Jungle.Sporebloom
{
    internal class Sporebloom : ModNPC
    {
        #region misc

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 16;
            NPC.AddElement(CrossModHelper.Nature);
            NPC.AddNPCElementList("Plantlike");
        }

        public override void SetDefaults()
        {
            NPC.width = 52;
            NPC.height = 56;

            NPC.lifeMax = 240;
            NPC.defense = 20;
            NPC.damage = 5; //low contact dmg because the spores do the work

            NPC.knockBackResist = 0.7f;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.aiStyle = -1;

            NPC.noGravity = true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ItemType<PlantBow>(), 50, 25));
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(
            [
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundJungle,
                new FlavorTextBestiaryInfoElement("One of the many hazardous plants of the underground jungle. It floats with the help of its balloon-like bulb, filled with poisonous spores that it spreads when in the vicinity of other living things.")
            ]);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.UndergroundJungle.Chance * 0.13f;
        }

        //order of writing and reading is important!!!
        //when writing a 4 byte float, make sure to read the same thing!
        //ex: dont write a bool and then an int32 but read them in the opposite order

        public override void SendExtraAI(BinaryWriter writer)
        {
            for(int i = 0; i < Logic.Length; i++)
                writer.Write(Logic[i]);

            writer.WriteVector2(targetPosition);
            writer.Write(NPC.behindTiles);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < Logic.Length; i++)
                Logic[i] = reader.ReadSingle();

            targetPosition = reader.ReadVector2();
            NPC.behindTiles = reader.ReadBoolean();
        }

        #endregion

        /// <summary>
        /// 0: General timer
        /// <br>1:For checking whether or not to do the barf animation + spew spores</br>
        /// <br>2:Misc timer for particles</br>
        /// <br>3:Recovery phase frame checl</br>
        /// <br>4:Random velocity rotation for the idle phase</br>
        /// <br>5:For getting a desired position to wander to for the idle phase</br>
        /// <br>6:Handles letting the glow fade in and out</br>
        /// <br>7:Same here</br>
        /// <br>8:Frameheight check for the barf, to ensure it completes the animation</br>
        /// <br>9:Glow interpolant</br>
        /// <br>10: draw offset for the roots when healing</br>
        /// </summary>
        public float[] Logic = new float[11];

        Vector2 targetPosition = Vector2.Zero;

        public enum Behaviour
        {
            Idle, SporeBarf, FallingDownToRecover
        }

        public Behaviour CurrentBehaviour
        {
            get => (Behaviour)NPC.ai[2];
            set => NPC.ai[2] = (int)value;
        }

        Texture2D tex = null, texGlow = null, texGlow2 = null;

        public override void FindFrame(int frameHeight)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                switch (CurrentBehaviour)
                {
                    case Behaviour.Idle:
                        {
                            int start = 0;
                            int end = 3;

                            NPC.frameCounter += 0.5f;
                            if (NPC.frameCounter > 3f)
                            {
                                NPC.frameCounter = 0;
                                NPC.frame.Y += frameHeight;

                                if (NPC.frame.Y > end * frameHeight)
                                {
                                    NPC.frame.Y = start * frameHeight;
                                }
                            }
                        }
                        break;

                    case Behaviour.SporeBarf:
                        {
                            if (Logic[1] == 0f) //preparing 
                            {
                                int start = 4;
                                int end = 6;
                                float speed = 3f;
                                NPC.frameCounter += 0.5f;
                                if (NPC.frameCounter > speed)
                                {
                                    NPC.frameCounter = 0;
                                    NPC.frame.Y += frameHeight;

                                    if (NPC.frame.Y > end * frameHeight)
                                    {
                                        NPC.frame.Y = start * frameHeight;
                                        speed -= 0.035f;
                                    }
                                }
                            }

                            if (Logic[1] == 1f)//blegh
                            {
                                Logic[8] = frameHeight;

                                int start = 7;
                                int end = 13;

                                NPC.frameCounter += 0.5f;
                                if (NPC.frameCounter > 3f)
                                {
                                    NPC.frameCounter = 0;
                                    NPC.frame.Y += frameHeight;

                                    if (NPC.frame.Y > end * frameHeight)
                                    {
                                        NPC.frame.Y = end * frameHeight;
                                    }
                                }
                            }
                        }
                        break;

                    case Behaviour.FallingDownToRecover:
                        {
                            if (Logic[3] == 1f)
                            {
                                Logic[8] = frameHeight;

                                int start = 14;
                                int end = 15;

                                NPC.frameCounter += 0.5f;
                                if (NPC.frameCounter > 5f)
                                {
                                    NPC.frameCounter = 0;
                                    NPC.frame.Y += frameHeight;

                                    if (NPC.frame.Y > end * frameHeight)
                                    {
                                        NPC.frame.Y = end * frameHeight;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            else //for the bestiary
            {
                int start = 0;
                int end = 3;

                NPC.frameCounter += 0.5f;
                if (NPC.frameCounter > 3f)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > end * frameHeight)
                    {
                        NPC.frame.Y = start * frameHeight;
                    }
                }
            }
        }

        public override void AI()
        {
            CheckTarget(out Player player);
            Lighting.AddLight(NPC.Center, new Vector3(.19f, .08f, .11f));

            if (player is null || player.dead || !player.active)
            {
                CurrentBehaviour = Behaviour.Idle;
                NPC.netUpdate = true;
            }

            else
            {
                if (Logic[6] == 1f)
                    Logic[9] = Lerp(Logic[9], 0f, 0.12f); //glow fades out

                if (Main.rand.NextBool(34))
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2CircularEdge(55f, 55f);
                    Vector2 vel = Main.rand.NextVector2Circular(0.1f, 0.1f);

                    Color particleColor = Color.Lerp(Color.LimeGreen, Color.Green, Main.rand.NextFloat(1f));

                    ParticleHandler.SpawnParticle(
                    new GlowyBall(pos, vel.RotatedByRandom(Pi / 20f), particleColor, Color.Lerp(particleColor, Color.White, 0.12f), 35, 1f, 0f, 0f, Main.rand.NextFloat(0.52f, 1.05f), Vector2.One, 0.94f, 0.0015f)
                    {
                        BloomColor = particleColor,
                        DrawWithBloom = true,
                    });
                }

                NPC.rotation = Lerp(NPC.rotation, NPC.velocity.X * 0.11815f, 0.1f);

                if (CurrentBehaviour == Behaviour.Idle) //wander around aimlessly, do not use the player as a target for anything!!
                {
                    Logic[10] = Lerp(Logic[10], 0f, 0.13f);

                    if (Logic[5] == 0f)
                    {
                        targetPosition = NPC.Center + new Vector2(RandomInRange(-500f, 500f, 250f), RandomInRange(-600f, 600f, 300f));

                        for (int i = 0; i < 100; i++)
                        {
                           // if (Collision.SolidCollision(targetPosition, NPC.width + 10, NPC.height + 10))
                           // {
                           //     targetPosition.X += RandomInRange(-300f, 300f, 200f);
                            //    targetPosition.Y += RandomInRange(-300f, 300f, 100f);
                            //}

                            if (Collision.CanHitLine(NPC.Center, NPC.width + 10, NPC.height + 10, targetPosition, NPC.width + 10, NPC.height + 10))
                            {
                                targetPosition.X += RandomInRange(-300f, 300f, 200f);
                                targetPosition.Y += RandomInRange(-300f, 300f, 100f);

                                targetPosition *= -1.1f;
                            }

                            else break;
                        }

                        Logic[4] = Main.rand.NextFloat(TwoPi);
                        Logic[5] = 1f;

                        NPC.netUpdate = true;
                    }

                    Vector2 destination = targetPosition + NPC.velocity * 5f;

                    Vector2 velocity = NormalizeBetter(destination - NPC.Center).RotatedBy(Logic[4]);

                    NPC.velocity = Vector2.Lerp(NPC.velocity, velocity * 0.9f, 0.15f);

                    if (NPC.collideX)
                        Logic[5] = 0f;

                    if (NPC.collideY)
                        Logic[5] = 0f;

                    if (NPC.Center.X.CloseTo(targetPosition.X, NPC.width + 5f) || NPC.Center.Y.CloseTo(targetPosition.Y, NPC.height + 5f) || Collision.SolidCollision(destination, NPC.width + 10, NPC.height + 10))
                        Logic[5] = 0f;

                    if (++Logic[0] >= 600f)
                    {
                        if (player != null && player.active)
                        {
                            ResetAllValues([9]); //save the glow!
                            CurrentBehaviour = Behaviour.SporeBarf;
                            NPC.netUpdate = true;
                        }

                        Logic[0] = 0f;
                    }
                }

                //hawk tuah
                if (CurrentBehaviour == Behaviour.SporeBarf) 
                {
                    if (++Logic[0] >= 800f) //barf
                    {
                        NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
                        NPC.velocity *= 0.9f;
                        Logic[6] = 0f;
                        Logic[9] = Lerp(Logic[9], 1f, 0.041f);

                        if (Logic[2] == 65f && Main.getGoodWorld)
                        {
                            SoundEngine.PlaySound(new(Texture + "_hawkTuah"), NPC.Center);
                        }

                        if (++Logic[2] >= 90)
                        {
                            Logic[1] = 1f;

                            if (Logic[2] == 109f) //barf time!!!
                            {
                                for(int i = 0; i < 50; i++)
                                {
                                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(10f, 10f);
                                    Vector2 vel = -pos.DirectionTo(NPC.Center) * Main.rand.NextFloat(1.5f, 10f);

                                    Color particleColor = Color.Lerp(Color.LimeGreen, Color.HotPink, Main.rand.NextFloat(1f));

                                    ParticleHandler.SpawnParticle(
                                    new GlowyBall(pos, vel, particleColor, Color.Lerp(particleColor, Color.White, 0.2f), 35, 1f, -0.48f, Main.rand.NextFloat(0.52f, 1.05f), 0f, Vector2.One, 0.92f, 0.0025f)
                                    {
                                        BloomColor = particleColor,
                                        DrawWithBloom = true,
                                    });
                                }

                                for (int i = 0; i < 20; i++)
                                {
                                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                                    Vector2 vel = -pos.DirectionTo(NPC.Center) * Main.rand.NextFloat(1.5f, 10f);

                                    Color particleColor = Color.Lerp(Color.LimeGreen, Color.HotPink, Main.rand.NextFloat(1f));

                                    ParticleHandler.SpawnParticle(
                                    new GlowyBall(pos, vel, particleColor, Color.Lerp(particleColor, Color.White, 0.2f), 130, 0.7f, -0.1f, Main.rand.NextFloat(0.52f, 1.05f), -0.1f, Vector2.One, 0.9f, 0.0025f)
                                    {
                                        BloomColor = particleColor,
                                        DrawWithBloom = true,
                                    });
                                }

                                for (int i = 0; i < 8; i++)
                                {
                                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(3f, 3f);
                                    Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f) * Main.rand.NextFloat(0.5f, 1f);

                                    NPC.SpawnProjectile(pos, vel, ProjectileType<SporebloomSpore>(), NPC.damage, 2f);
                                }
                            }

                            if (Logic[2] >= 135f && NPC.frame.Y >= 13f * Logic[8])
                            {
                                ResetAllValues([9]);
                                Logic[6] = 0f;
                                CurrentBehaviour = Behaviour.FallingDownToRecover;
                                NPC.netUpdate = true;
                            }
                        }

                        else
                        {
                            float rand = 200f * (1.4f - (Logic[2] / 90f));

                            Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(rand, rand);
                            Vector2 vel = pos.DirectionTo(NPC.Center) * Main.rand.NextFloat(1.8f, 8f) * (1f - (Logic[2] / 90f));

                            Color particleColor = Color.Lerp(Color.LimeGreen, Color.HotPink, Main.rand.NextFloat(1f));

                            ParticleHandler.SpawnParticle(
                            new GlowyBall(pos, vel, particleColor, Color.Lerp(particleColor, Color.White, 0.2f), (int)(Main.rand.Next(12, 40) * (1.5f - (Logic[2] / 90f))), 0.58f, -0.48f, Main.rand.NextFloat(0.52f, 1.05f), 0f, Vector2.One, 0.92f, 0.0025f)
                            {
                                BloomColor = particleColor,
                                DrawWithBloom = true,
                            });
                        
                        }
                    }

                    else
                    {
                        Vector2 pointToMoveTo = player.Center - NPC.velocity * 20f;
                        Vector2 velocity = NormalizeBetter(pointToMoveTo - NPC.Center);

                        float speed = 1f;
                        speed = Lerp(speed, 4f, 0.18f);

                        NPC.velocity = Vector2.Lerp(NPC.velocity, velocity * speed, (speed / 90f) + 0.035f);

                        if (NPC.collideX)
                            NPC.velocity.X *= -1.2f;

                        if(NPC.collideY)
                            NPC.velocity.Y *= -1.2f;

                        if (NPC.Center.X.CloseTo(pointToMoveTo.X, 100f) || NPC.Center.Y.CloseTo(pointToMoveTo.Y, 100f))
                            speed *= 0.98f; //slow down

                        if (NPC.Center.X.CloseTo(pointToMoveTo.X, 10f) || NPC.Center.Y.CloseTo(pointToMoveTo.Y, 10f) && Logic[0] > 180) //close enough to barf + some time has passed
                        {
                            speed = 0f;
                            Logic[0] = 800f;
                        }
                    }
                }

                if (CurrentBehaviour == Behaviour.FallingDownToRecover) //float down, glow, and slightly absorb particles and boost regen but lower defense
                {
                    NPC.knockBackResist = 0f;
                    NPC.defense = 0;
                    NPC.damage = 0;
                    NPC.behindTiles = true;

                    if (Logic[0] <= 500f) //fall, then heal a bit.
                    {
                        if (!NPC.collideY)
                        {
                            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.UnitY * 1.2f, 0.051f);
                            NPC.velocity.X *= 0.95f;
                        }

                        if (NPC.collideY || Collision.SolidCollision(NPC.Center, 0, NPC.height))
                        {
                            Logic[0]++;
                            Logic[10] = Lerp(Logic[10], 13f, 0.13f);
                            if (Logic[0] % 45 == 0 && NPC.life < NPC.lifeMax)
                            {
                                for(int i = 0; i < 14; i++)
                                {
                                    Vector2 pos = NPC.Center + Main.rand.NextVector2CircularEdge(55f, 55f) + new Vector2(0f, 20f);
                                    Vector2 vel = pos.DirectionTo(NPC.Center) * Main.rand.NextFloat(3.6f, 5.2f);

                                    Color particleColor = Color.Lerp(Color.LimeGreen, Color.Green, Main.rand.NextFloat(1f));

                                    ParticleHandler.SpawnParticle(
                                    new GlowyBall(pos, vel.RotatedByRandom(Pi / 20f), particleColor, Color.Lerp(particleColor, Color.White, 0.12f), 38, 0.958f, -0.62f, Main.rand.NextFloat(0.52f, 1.05f), 0f, Vector2.One, 0.94f, 0.005f)
                                    {
                                        BloomColor = particleColor,
                                        DrawWithBloom = true,
                                    });
                                }

                                ParticleHandler.SpawnParticle(
                                new Shockwave(NPC.Center + new Vector2(0f, 20f), Vector2.Zero, Color.Lerp(Color.LimeGreen, Color.ForestGreen, Main.rand.NextFloat(1f)) * 0.87f, 0f, 0.25f, 0f, Vector2.One, 30)
                                {
                                    BloomColor = Color.LightGreen,
                                    DrawWithBloom = true,
                                });

                                NPC.SimpleHealNPC(NPC.lifeMax / 120);
                            }

                            if (NPC.life >= NPC.lifeMax && Logic[0] >= 200) //end early if full on hp
                            {
                                Logic[0] = 501f;
                            }
                        }
                    }

                    else //all good!
                    {
                        Logic[3] = 1f;
                        Logic[0]++;
                        if (NPC.frame.Y >= 15f * Logic[8] && Logic[0] > 520f)
                        {
                            ResetAllValues([9]); //save the glow, and let the glow fade out now!
                            CurrentBehaviour = Behaviour.Idle;
                            NPC.defense = NPC.defDefense;
                            NPC.damage = NPC.defDamage;
                            Logic[6] = 1f;
                            NPC.behindTiles = false;
                            NPC.netUpdate = true;
                        }
                    }
                }
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.dedServ)
                return;

            if (NPC.life <= 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                    Vector2 vel = (-pos.DirectionTo(NPC.Center) * Main.rand.NextFloat(1.5f, 10f)) + new Vector2(0f, Main.rand.NextFloat(-7f, 4f));

                    Color particleColor = Color.Lerp(Color.LimeGreen, Color.HotPink, Main.rand.NextFloat(1f));

                    ParticleHandler.SpawnParticle(
                    new GlowyBall(pos, vel, particleColor, Color.Lerp(particleColor, Color.White, 0.2f), 130, 0.8f, -0.1f, Main.rand.NextFloat(0.52f, 1.05f), -0.1f, Vector2.One, 0.9f, 0.0025f)
                    {
                        BloomColor = particleColor,
                        DrawWithBloom = true,
                    });
                }
            }

            else
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(8f, 8f);
                    Vector2 vel = -pos.DirectionTo(NPC.Center) * Main.rand.NextFloat(1.5f, 3f);

                    Color particleColor = Color.Lerp(Color.LimeGreen, Color.HotPink, Main.rand.NextFloat(1f));

                    ParticleHandler.SpawnParticle(
                    new GlowyBall(pos, vel, particleColor, Color.Lerp(particleColor, Color.White, 0.2f), 130, 0.8f, -0.1f, Main.rand.NextFloat(0.52f, 1.05f), -0.1f, Vector2.One, 0.9f, 0.0025f)
                    {
                        BloomColor = particleColor,
                        DrawWithBloom = true,
                    });
                }
            }
        }

        #region utils

        void ResetAllValues(int[] indexesToIgnore)
        {
            if (indexesToIgnore.Length > 0)
            {
                for (int i = 0; i < Logic.Length; i++)
                {
                    for (int j = 0; j < indexesToIgnore.Length; j++)
                    {
                        if (i != indexesToIgnore[j])
                            Logic[i] = 0;
                    }
                }
            }

            else
            {
                for (int i = 0; i < Logic.Length; i++)
                {
                    Logic[i] = 0;
                }
            }

            targetPosition = Vector2.Zero;
        }

        //literally impossible to get an invalid player unless there are none

        bool InvalidTarget => NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active;

        void CheckTarget(out Player target, bool face = true)
        {
            if (InvalidTarget)
            {
                NPC.TargetClosest();
            }

            target = GetTarget(face);

            if (target is null || InvalidTarget)
            {
                NPC.TargetClosest();
                target = GetTarget(face);
            }
        }

        public Player GetTarget(bool faceTheMan = true)
        {
            NPC.TargetClosest(faceTheMan);

            if (NPC.HasValidTarget)
                return Main.player[NPC.target];

            else
            {
                NPC.TargetClosest(faceTheMan);

                if (!NPC.HasValidTarget)
                    return null;

                else return Main.player[NPC.target];
            }
        }

        #endregion

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            tex ??= Request<Texture2D>($"{Texture}").Value;
            texGlow ??= Request<Texture2D>($"{Texture}" + "_Glow").Value;
            texGlow2 ??= Request<Texture2D>($"{Texture}" + "_Glow2").Value;

            Vector2 offsetRoots = new Vector2(0f, Logic[10]);

            NPC.SimpleDrawNPC(tex, offsetRoots, screenPos, drawColor, false, 1f);
            NPC.SimpleDrawNPC(texGlow2, offsetRoots, screenPos, Color.White, true, 1f);
            NPC.SimpleDrawNPC(texGlow, offsetRoots, screenPos, Color.White * Logic[9], true, 1f);

            return false;
        }
    }

    class SporebloomSpore : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<Sporebloom>() + "_Spore";

        Texture2D tex = null;
        bool lerpToZero = false;
        float opacity = 0f;
        float scale = 0f;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 700;
            Main.projFrames[Type] = 3;
            Projectile.scale = 1f;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0f)
            {
                Projectile.frame = Main.rand.Next(2);
                Projectile.ai[0] = 1f;
                Projectile.netUpdate = true;
                Projectile.alpha = 50;
            }

            else
            {
                //Main.NewText(Projectile.alpha);

                if (Projectile.timeLeft < 80)
                    Projectile.alpha += 2;

                Projectile.alpha += 1;

                if (Projectile.alpha >= 255)
                    Projectile.Kill();

                Projectile.scale += 0.0054f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, 0.001f);
                Projectile.rotation += 0.032f * Projectile.velocity.X;

                if (Main.rand.NextBool(6))
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2CircularEdge(35f, 35f);
                    Vector2 vel = Main.rand.NextVector2Circular(0.1f, 0.1f);

                    Color particleColor = Color.Lerp(Color.LightPink, Color.Pink, Main.rand.NextFloat(1f));

                    ParticleHandler.SpawnParticle(
                    new GlowyBall(pos, vel.RotatedByRandom(Pi / 20f), particleColor, Color.Lerp(particleColor, Color.White, 0.12f), 35, 1f, 0f, 0f, Main.rand.NextFloat(0.52f, 1.05f), Vector2.One, 0.94f, 0.0015f)
                    {
                        BloomColor = particleColor,
                        DrawWithBloom = true,
                    });
                }

                if (Projectile.ai[2] == 1f)
                {
                    Projectile.velocity.Y += 0.023f;

                    foreach (NPC n in Main.ActiveNPCs)
                    {
                        if (!n.friendly && !n.CountsAsACritter && n.Hitbox.Intersects(Projectile.Hitbox))
                        {
                            n.AddBuff(BuffID.Poisoned, 2);
                        }
                    }
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            
        }



        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            lerpToZero = true;
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate((int)(10f * Projectile.scale), (int)(10f * Projectile.scale));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            tex ??= Request<Texture2D>(Texture).Value;

            Main.spriteBatch.SaveCurrent();

            Main.spriteBatch.Reload(BlendState.Additive);

            VFXManager.DrawCache.Add(() =>
            {
                Projectile.SimpleDrawProjectile(tex, Color.HotPink * Projectile.Opacity * 0.8f, true, Projectile.scale * 1.1f);
            });

            Projectile.SimpleDrawProjectile(tex, Color.Pink * Projectile.Opacity, true, Projectile.scale);

            VFXManager.DrawCache.Add(() =>
            {
                Projectile.SimpleDrawProjectile(tex, Color.LightPink * Projectile.Opacity * 0.98f, true, Projectile.scale * 0.35f);
            });

            Main.spriteBatch.ApplySaved();

            return false;
        }
    }
}
