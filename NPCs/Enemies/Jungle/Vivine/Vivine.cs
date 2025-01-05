using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MythosOfMoonlight.Dusts;
using Terraria.GameContent;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using MythosOfMoonlight.Items.Jungle;
using MythosOfMoonlight.Common.Crossmod;

namespace MythosOfMoonlight.NPCs.Enemies.Jungle.Vivine
{
    public class Vivine : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 10;
            NPC.AddElement(CrossModHelper.Nature);
            NPC.AddNPCElementList("Humanoid");
            NPC.AddNPCElementList("Plantlike");
        }
        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 58;
            NPC.lifeMax = 110;
            NPC.defense = 5;
            NPC.damage = 15;
            NPC.knockBackResist = 0.8f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PlantGun>(), 50));
            npcLoot.Add(ItemDropRule.Common(ItemID.Vine, 3));
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundJungle,
                new FlavorTextBestiaryInfoElement("A type of tripod carnivorous flower native to the jungle's moist caverns. Spits a pink corrosive substance at prey, approaching only to slurp up the pre-digested slurry.")
            });
        }
        public float AIState
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        public float AITimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        public float AITimer2
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        public float AITimer3
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }
        const int Idle = 0, Move = 1, Spit = 2;
        public override void FindFrame(int frameHeight)
        {
            if (!NPC.IsABestiaryIconDummy)
            {
                if (NPC.velocity.Y > .1f || NPC.velocity.Y < -.1f)
                {
                    NPC.frameCounter = 1;
                    NPC.frame.Y = 4 * NPC.height;
                }
                if (AIState == Idle)
                    NPC.frame.Y = 0;
                else if (AIState == Move)
                {
                    NPC.frameCounter++;
                    if (NPC.frameCounter % 5 == 0)
                    {
                        if (NPC.frame.Y < 4 * NPC.height)
                            NPC.frame.Y += NPC.height;
                        else
                            NPC.frame.Y = 1 * NPC.height;
                    }
                }
                else if (AIState == Spit)
                {
                    if (NPC.frame.Y < 5 * NPC.height)
                        NPC.frame.Y = 5 * NPC.height;
                    NPC.frameCounter++;
                    if (NPC.frameCounter % 5 == 0)
                    {
                        if (NPC.frame.Y < 9 * NPC.height)
                            NPC.frame.Y += NPC.height;
                        else
                        {
                            AITimer = 0;
                            AIState = Idle;
                        }
                    }
                }
            }
            else
            {
                NPC.frameCounter++;
                if (NPC.frameCounter % 5 == 0)
                {
                    if (NPC.frame.Y < 4 * NPC.height)
                        NPC.frame.Y += NPC.height;
                    else
                        NPC.frame.Y = 1 * NPC.height;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            Helper.SpawnDust(NPC.position, NPC.Size, DustID.Grass, new Vector2(2 * hit.HitDirection, -1.5f), 4);
            Helper.SpawnDust(NPC.position, NPC.Size, ModContent.DustType<JunglePinkDust>(), Vector2.One * hit.HitDirection * 2, 4);
            if (NPC.life <= 0)
            {
                Helper.SpawnDust(NPC.position, NPC.Size, DustID.Grass, new Vector2(2 * hit.HitDirection, -1.5f), 8);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/Vivine", 2, 1, Vector2.One * hit.HitDirection * 2);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/Vivine", 1, 2, Vector2.One * hit.HitDirection * 2);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/Vivine", 3, 3, Vector2.One * hit.HitDirection * 2);
                Helper.SpawnDust(NPC.position, NPC.Size, ModContent.DustType<JunglePinkDust>(), Vector2.One * hit.HitDirection * 2, 8);
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return SpawnCondition.UndergroundJungle.Chance * 0.2f;
        }
        public override bool? CanFallThroughPlatforms()
        {
            Player player = Main.player[NPC.target];
            return player.Center.Y > NPC.Center.Y;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //3hi31mg
            var clr = new Color(255, 255, 255, 100); // full white
            var drawPos = NPC.Center - screenPos;
            var texture = ModContent.Request<Texture2D>(NPC.ModNPC.Texture + "_Glow").Value;
            var origTexture = TextureAssets.Npc[NPC.type].Value;
            var frame = new Rectangle(0, NPC.frame.Y, NPC.width, NPC.height);
            var orig = frame.Size() / 2f - new Vector2(0, 3);
            Main.spriteBatch.Draw(origTexture, drawPos, frame, drawColor, NPC.rotation, orig, NPC.scale, NPC.direction < 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            Main.spriteBatch.Draw(texture, drawPos, frame, clr, NPC.rotation, orig, NPC.scale, NPC.direction < 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
        public override void AI()
        {
            Lighting.AddLight(NPC.Center, new Vector3(.19f, .08f, .11f));
            NPC.TargetClosest();
            Player player = Main.player[NPC.target];
            NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            if (AIState == Idle)
            {
                NPC.velocity.X *= .9f;
                NPC.knockBackResist = 0.8f;
                AITimer++;
                if (AITimer >= 50)
                {
                    AITimer = 0;
                    AIState = Move;
                    NPC.frame.Y = NPC.height * 2;
                }
            }
            else if (AIState == Move)
            {
                var Distance = Vector2.Distance(player.Center, NPC.Center);
                NPC.knockBackResist = 0.8f;
                AITimer++;
                NPC.GetGlobalNPC<FighterGlobalAI>().FighterAI(NPC, 6, 1, true, -1, 0/*, 1, 0*/);
                if (AITimer >= 200 && Distance < 500)
                {
                    AITimer = 0;
                    AIState = Spit;
                    NPC.frame.Y = NPC.height * 5;
                    NPC.velocity = Vector2.Zero;
                }
            }
            else if (AIState == Spit)
            {
                NPC.velocity.X *= 0;
                NPC.knockBackResist = 0f;
                if (NPC.frame.Y == 9 * NPC.height && AITimer == 0)
                {
                    AITimer = 1;
                    {
                        SoundEngine.PlaySound(SoundID.Item17);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - new Vector2(1, 19), Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center) * 10f, ModContent.ProjectileType<VivineSpit>(), 15, 0);
                    }
                }
            }
        }
    }
    public class VivineSpit : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Projectile.AddElement(CrossModHelper.Poison);
        }
        public override void SetDefaults()
        {
            Projectile.Size = Vector2.One * 10;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 1;
            Projectile.timeLeft = 300;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Vector3(.19f, .08f, .11f) * .5f);
            Helper.SpawnDust(Projectile.position, Projectile.Size, ModContent.DustType<JunglePinkDust>(), Vector2.Zero, 1, dust => dust.noGravity = true);
        }
        public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 100);
        public override void Kill(int timeleft)
        {
            Helper.SpawnDust(Projectile.position, Projectile.Size, ModContent.DustType<JunglePinkDust>(), -Projectile.velocity * .5f, 20, dust => dust.scale = 1.5f);
            SoundEngine.PlaySound(SoundID.NPCHit18);
        }
    }
}