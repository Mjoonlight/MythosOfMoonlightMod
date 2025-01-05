using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Items.Materials;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MythosOfMoonlight.Common.BiomeManager;
using MythosOfMoonlight.Common.Crossmod;
using MythosOfMoonlight.Items.PurpleComet.IridicSet;
using MythosOfMoonlight.Items.Weapons.Summon;

namespace MythosOfMoonlight.NPCs.Enemies.CometFlyby.StrandedMartian
{
    public class StrandedMartian : ModNPC
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Stranded Martian");
            Main.npcFrameCount[NPC.type] = 9;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0) { Velocity = 1f };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
            NPC.AddNPCElementList("Humanoid");
        }
        public override void SetDefaults()
        {
            NPC.width = 38;
            NPC.height = 44;
            NPC.damage = 15;
            NPC.lifeMax = 90;
            NPC.defense = 2;
            NPC.HitSound = SoundID.NPCHit39;
            NPC.DeathSound = SoundID.NPCDeath57;
            NPC.aiStyle = -1;
            NPC.netAlways = true;
            SpawnModBiomes = new int[]
            {
                ModContent.GetInstance<PurpleCometBiome>().Type
            };
        }
        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter++;
                NPC.frame = new Rectangle(0, (2 + ((int)(NPC.frameCounter / 6) % 7)) * 46, 38, 46);
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<PurpleCometBiome>().ModBiomeBestiaryInfoElement),
                new FlavorTextBestiaryInfoElement("A martian sent on a scouting mission, who collided with a rogue comet that was unforeseen in its ships course. Determined to survive, it adapted by creating a crystalline substance from the comet's stone known as Iridic quartz, which sees use widely in its arsenal, alongside an alloy of Martian steel and Iridic quartz, known as Iridium. It follows the comet on ground, secretively working on a means to communicate to its planet once more.")
            });
        }
        private enum NState
        {
            Wander,
            Shoot
        }
        private NState State
        {
            get { return (NState)(int)NPC.ai[0]; }
            set { NPC.ai[0] = (int)value; }
        }
        private void SwitchTo(NState state)
        {
            State = state;
        }
        public float JumpCD
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        public float Timer
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        public override void AI()
        {
            if (JumpCD > 60)
            {
                JumpCD = 0;
            }
            foreach (Player player in Main.player)
            {
                if (State == NState.Wander && Vector2.Distance(player.Center, NPC.Center) <= 300f)
                {
                    Timer = 0;
                    NPC.target = player.whoAmI;
                    SwitchTo(NState.Shoot);
                }
            }
            switch (State)
            {
                case NState.Wander:
                    Timer++;
                    NPC.spriteDirection = NPC.direction;
                    if (NPC.velocity.Y <= 0)
                        NPC.frame = new Rectangle(0, (2 + ((int)(Timer / 6) % 7)) * 46, 38, 46);
                    else
                        NPC.frame = new Rectangle(0, 46, 38, 46);

                    if (NPC.collideX && NPC.collideY)
                        NPC.velocity.Y -= 7f;

                    if (Timer > 20)
                        NPC.GetGlobalNPC<FighterGlobalAI>().FighterAI(NPC, 7, 1, true, -1, 0/*, 1, 0*/);
                    break;
                case NState.Shoot:
                    Timer++;
                    NPC.direction = (Main.player[NPC.target].Center.X >= NPC.Center.X) ? 1 : -1;
                    NPC.spriteDirection = NPC.direction;
                    NPC.frame = new Rectangle(0, 0, 38, 46);
                    if (Timer % 60 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item68, NPC.Center);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Normalize(Main.player[NPC.target].Center + new Vector2(0, -150) - NPC.Center) * 10f, ModContent.ProjectileType<CometEmberMini>(), Main.expertMode ? 6 : 8, .075f, NPC.target, Main.player[NPC.target].Center.X >= NPC.Center.X ? 1 : -1);
                    }
                    if (!Main.player[NPC.target].active || Main.player[NPC.target].dead || Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 400f || Timer > 120)
                    {
                        NPC.TargetClosest(true);
                        SwitchTo(NState.Wander);
                    }
                    NPC.velocity *= .9f;
                    break;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Color color = Color.White;
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY + 4);
            Texture2D texture = ModContent.Request<Texture2D>(NPC.ModNPC.Texture + "_Glow").Value;
            Texture2D origTexture = TextureAssets.Npc[NPC.type].Value;
            Rectangle frame = new(0, NPC.frame.Y, NPC.width, NPC.height);
            Vector2 orig = frame.Size() / 2f;
            Main.spriteBatch.Draw(origTexture, drawPos, frame, drawColor, NPC.rotation, orig, NPC.scale, NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            Main.spriteBatch.Draw(texture, drawPos, frame, color, NPC.rotation, orig, NPC.scale, NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return base.SpawnChance(spawnInfo);
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (State == NState.Shoot)
            {
                Timer = 0;
                NPC.target = player.whoAmI;
                SwitchTo(NState.Wander);
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PurpurineQuartz>(), 1, 3, 6));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MOCIris>(), 20, 1, 1));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CommunicatorIris>(), 40, 1, 1));
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 1; i <= 5; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MartianHit, 0, -1);
                Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<PurpurineDust>(), 0, -1);
            }
            if (NPC.life <= 0)
            {
                for (int i = 1; i <= 20; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.MartianHit, 0, -1);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<PurpurineDust>(), 0, -1);
                }
                Helper.SpawnGore(NPC, "MythosOfMoonlight/StrMartian", 2, 1, Vector2.One * hit.HitDirection * 2);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/StrMartian", 2, 2, Vector2.One * hit.HitDirection * 2);
                Helper.SpawnGore(NPC, "MythosOfMoonlight/StrMartian", 1, 3, Vector2.One * hit.HitDirection * 2);
            }
        }
    }
}
