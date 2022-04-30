﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MythosOfMoonlight.Dusts;
using Terraria.GameContent;
using Terraria.ModLoader.Utilities;

namespace MythosOfMoonlight.NPCs.Enemies.EntropicTotem
{
    public class EntropicTotem : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Entropic Totem");
            Main.npcFrameCount[NPC.type] = 5;

            NPCID.Sets.DebuffImmunitySets.Add(Type, new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused,
                    BuffID.Poisoned,
                    BuffID.OnFire,
                    BuffID.Venom
                }
            });

            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0) { Velocity = 1 };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }
        public override void SetDefaults()
        {
            NPC.width = 62;
            NPC.height = 70;
            NPC.aiStyle = -1;
            NPC.damage = 15;
            NPC.defense = 10;
            NPC.lifeMax = 260;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath43;
        }
        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.650f * bossLifeScale);
        }

        const float SPEED = 4.2f, MINIMUM_DISTANCE = 60f;
        int State
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        Vector2 direction;
        void IncreaseFrameCounter() => NPC.frameCounter++;
        void GetTarget() => NPC.TargetClosest();
        void MovementLogic() // self explanatory
        {
            var player = Main.player[NPC.target];
            var distance = NPC.DistanceSQ(player.position); // check distance between player and NPC
            if (distance >= MINIMUM_DISTANCE * MINIMUM_DISTANCE) // if horizontal distance exceeds the minimum distance, change NPC direction to aim at player
            {
                direction = -(NPC.position - player.position).SafeNormalize(Vector2.UnitX);
            }
            switch (State)
            {
                case 0: // when not firing the projectiles
                    NPC.velocity = Vector2.Lerp(NPC.velocity, direction * SPEED, 0.03f);
                    break;
                case 1: // while firing the projectiles
                    NPC.velocity = Vector2.Lerp(NPC.velocity, direction * SPEED / 2f, 0.03f);
                    break;
            }

        }
        void TiltSprite() => NPC.rotation = MathHelper.Clamp(NPC.velocity.X * .15f, MathHelper.ToRadians(-30), MathHelper.ToRadians(30));
        void StateTransitionManagement()
        {
            var maxTime = EntropicTotemProjectile.EntropicTotemProjectile.MAX_TIMELEFT;
            if (State == 0 && NPC.frameCounter > maxTime * 2) // fires every 2 Projectile lifespans
            {
                int etpType = ModContent.ProjectileType<EntropicTotemProjectile.EntropicTotemProjectile>();
                for (int i = 0; i < 4; i++)
                {
                    var proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitX.RotatedBy(i * MathHelper.PiOver2) * 3, etpType, NPC.damage, 2, 255);
                    Main.projectile[proj].ai[0] = 3f;
                    Main.projectile[proj].ai[1] = NPC.whoAmI;
                }
                State = 1;
            }
            else if (NPC.frameCounter > maxTime * 3)
            {
                State = 0;
                NPC.frameCounter = 0;
            }
        }
        int AnimationFrame
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        int TrueFrame => AnimationFrame / FRAME_RATE;
        const int FRAME_RATE = 5;
        public override void FindFrame(int frameHeight)
        {
            if (AnimationFrame++ % FRAME_RATE == 0) // every 5 frames, go to next step of animation
            {
                var trueFrame = AnimationFrame / FRAME_RATE;
                NPC.frame.Y = frameHeight * trueFrame;
                if (trueFrame > Main.npcFrameCount[NPC.type] - 2)
                {
                    AnimationFrame = 0;
                }
            }
        }
        public override void HitEffect(int hitDirection, double damage)
        {
            int dustAmount = 5;
            if (NPC.life <= 0)
            {
                if (Main.netMode == NetmodeID.Server)
                    return;

                Helper.SpawnGore(NPC, "MythosOfMoonlight/EntroTotem", 1, 1);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/EntroTotem", 8, 2);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/EntroTotem", 3, 3);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/EntroTotem", 6, 4);
                Helper.SpawnDust(NPC.position, NPC.Size, ModContent.DustType<EntropicTotemProjectileDust>(), new Vector2(-hitDirection * Math.Abs(NPC.oldVelocity.X), -1.5f), dustAmount);
                dustAmount = 10;

            }
            Helper.SpawnDust(NPC.position, NPC.Size, DustID.Stone, new Vector2(-hitDirection * Math.Abs(NPC.oldVelocity.X), -1.5f), dustAmount);

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //3hi31mg
            var clr = new Color(255, 255, 255, 255); // full white
            var drawPos = NPC.Center - screenPos;
            var texture = ModContent.Request<Texture2D>(NPC.ModNPC.Texture + "_Glow").Value;
            var origTexture = TextureAssets.Npc[NPC.type].Value;
            var frame = new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height);
            var orig = frame.Size() / 2f;
            Main.spriteBatch.Draw(origTexture, drawPos, frame, drawColor, NPC.rotation, orig, NPC.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, frame, clr, NPC.rotation, orig, NPC.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override void AI()
        {
            IncreaseFrameCounter();
            GetTarget();
            MovementLogic();
            TiltSprite();
            StateTransitionManagement();
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.Cavern.Chance * 0.04f;
        }

    }
}