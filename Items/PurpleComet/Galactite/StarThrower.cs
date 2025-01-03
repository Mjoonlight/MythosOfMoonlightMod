using MythosOfMoonlight.Assets.Textures;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Graphics.MoMParticles;
using MythosOfMoonlight.Common.Graphics.MoMParticles.Types;
using MythosOfMoonlight.Dusts;
using MythosOfMoonlight.Items.PurpleComet.IridicSet;
using ReLogic.Utilities;
using System;
using Terraria.ModLoader;

namespace MythosOfMoonlight.Items.PurpleComet.Galactite
{
    public class StarThrower : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 30;

            Item.useTime = Item.useAnimation = 20;

            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 15; 
            Item.crit = 8;
            Item.knockBack = 3f;
            Item.shoot = ProjectileType<StarThrowerProj>();
            Item.shootSpeed = 1f;
            Item.rare = ItemRarityID.LightRed;

            Item.useTurn = false;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;

            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.useAmmo = AmmoID.Gel; //forgor
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => false; //spawning it shouldnt use 1 gel
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ProjectileType<StarThrowerProj>()] < 1;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, velocity, ProjectileType<StarThrowerProj>(), damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class StarThrowerProj : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<StarThrower>();

        public ref float Time => ref Projectile.ai[0];

        public ref float KillTimer => ref Projectile.ai[1];

        private Player Owner => Main.player[Projectile.owner];

        private Texture2D tex, glowTex = null;

        public bool die = false;

        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 30;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.netImportant = true;
            Projectile.Opacity = 0f;
            Projectile.extraUpdates = 1;
        }

        public override bool? CanDamage() => false;

        public Vector2 tipPosition = Vector2.Zero;
        SlotId theOund;

        public override void AI()
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            if (Owner.dead || !Owner.active || Owner.noItems || Owner.CCed || !ConsumeAmmo(Owner.HeldItem, Owner, out int i22, out int e22, 1, false))
                Projectile.Kill();

            Projectile.damage = Owner.GetWeaponDamage(Owner.HeldItem);
            Projectile.CritChance = Owner.GetWeaponCrit(Owner.HeldItem);
            Projectile.knockBack = Owner.GetWeaponKnockback(Owner.HeldItem, Owner.HeldItem.knockBack);

            Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, false, true);

            if (!die) //fade in
                Projectile.Opacity = Lerp(Projectile.Opacity, 1f, 0.0712f);

            tipPosition = (armPos + Projectile.velocity * Projectile.width * 0.7f) + new Vector2(0f, -1f * Projectile.direction).RotatedBy(Projectile.velocity.ToRotation());

            Projectile.ManageHeldProj(armPos, new HeldprojSettings(19f, (Projectile.spriteDirection == -1 ? Pi : 0f), Owner, 0f));

            if (Owner.channel && !die)
            {
                Projectile.timeLeft++;

                if (++Time % 8 == 0)
                {
                    if (ConsumeAmmo(Owner.HeldItem, Owner, out int type, out int damage, 1, Main.rand.NextBool(3)))
                    {
                        Vector2 vel = (NormalizeBetter(Projectile.velocity) * Main.rand.NextFloat(3.29f, 3.8f)).RotatedByRandom(Pi / 117.2f);
                        Vector2 pos = tipPosition + vel * 3f;

                        SpawnProjectle(Owner, ProjectileType<StarryFire>(), pos, vel, Projectile.damage / 2, 3f, true);

                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 pos2 = tipPosition + vel * 2.5f;
                            Vector2 vel2 = (NormalizeBetter(Projectile.velocity) * Main.rand.NextFloat(3.29f, 3.8f)).RotatedByRandom(Pi * 0.68f);

                            CreateDust(DustType<PurpurineDust>(), vel2, pos2, Color.White, Main.rand.NextFloat(0.9f, 1.4f));
                        }
                    }

                    else die = true;
                }

                if (Time > 7)
                {
                    if (SoundEngine.TryGetActiveSound(theOund, out var ound) && ound.IsPlaying)
                        ound.Position = Projectile.Center;

                    else theOund = SoundEngine.PlaySound(SoundID.DD2_KoboldIgniteLoop with { Pitch = -0.123f }, Projectile.Center);
                }
            }

            if (!Owner.channel || die)
            {
                die = true;

                Projectile.Opacity = Lerp(Projectile.Opacity, 0f, 0.13f);

                if (++KillTimer >= 30)
                {
                    if (SoundEngine.TryGetActiveSound(theOund, out var ound) && ound.IsPlaying)
                        ound.Stop();

                    Projectile.Kill();
                }
            }
        }

        #region drawing

        public override bool PreDraw(ref Color lightColor)
        {
            tex ??= Request<Texture2D>(Texture).Value;
            glowTex ??= Request<Texture2D>(Texture + "_Glow").Value;

            Projectile.SimpleDrawProjectile(tex, lightColor * Projectile.Opacity, false, 1f);
            Projectile.SimpleDrawProjectile(glowTex, Color.White * Projectile.Opacity, true, 1f);

            return false;
        }

        #endregion
    }

    public class StarryFire : ModProjectile
    {
        public ref float Timer => ref Projectile.ai[0];

        public const int Lifetime = 100;

        public override string Texture => TryGetTextureFromOther<StarThrower>(); //not used

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.MaxUpdates = 7;
            Projectile.timeLeft = Lifetime;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 4;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Projectile.Opacity * 0.09f, Projectile.Opacity * 0.01f, Projectile.Opacity * 0.09f);

            float t = Timer / (float)Lifetime;
            float scale = Lerp(0.15f, 1.4f, Pow(t, 0.53f));
            float opacity = Utils.GetLerpValue(0.96f, 0.7f, t, true);

            Color color = Color.Lerp(Color.Purple, Color.Lerp(Color.BlueViolet, Color.Purple, 0.145f), 1.2f * t);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * opacity);

            float rand = 2.5f * (scale * 7f);

            if (Main.rand.NextBool())
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(rand * 1.1f, rand * 1.1f);
                Vector2 vel = Projectile.velocity * 0.12f;

                GlowyBall ballz = new GlowyBall(pos, vel, color, Color.Violet, 60, 1f, -1f, 1.1f * scale * Main.rand.NextFloat(0.8f, 1.1f), 4f * scale, new Vector2(Main.rand.NextFloat(1.15f, 1.45f), Main.rand.NextFloat(0.9f, 1.1f)), 0.98f, 0.00535f) 
                {
                    DrawWithBloom = true, 
                    BloomColor = color,
                    BlendState = BlendState.Additive
                };

                ParticleHandler.SpawnParticle(ballz);

                Vector2 pos2 = Projectile.Center + Main.rand.NextVector2Circular(rand * 1.29f, rand * 1.29f);
                Vector2 vel2 = Projectile.velocity;

                CreateDust(DustType<StretchyGlow>(), vel * Main.rand.NextFloat(1.3f, 1.5f), pos2, color * ((1.15f - t)), scale * 2.9f, 0, noGrav: true).customData = new Vector2(1.18f, Main.rand.NextFloat(0.6f, 0.71f));
            }

            Timer++;
        }

        public override void OnHitNPC(NPC t, NPC.HitInfo hit, int damageDone) => t.AddBuff(BuffType<StarFlames>(), 240);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!target.SuperArmor)
            {
                modifiers.ArmorPenetration += 25f; //vanilla flamethrower has 15
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            float t = Timer / Lifetime;
            float scale = Lerp(0.3f, 1.1f, t);

            width = (int)(width * scale);
            height = (int)(height * scale);

            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    class StarFlames : ModBuff
    {
        public override string Texture => "MythosOfMoonlight/Assets/Textures/Extra/blank";

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<StarflameNPC>().BuffActive = true;
        }
    }

    class StarflameNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool BuffActive;

        float time = 0f;

        public override void ResetEffects(NPC npc) => BuffActive = false;

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (BuffActive)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 40;
                damage = 4;
            }

            base.UpdateLifeRegen(npc, ref damage);
        }

        public override bool PreAI(NPC npc)
        {
            if (BuffActive)
            {
                if (++time % 4 == 0)
                {
                    Vector2 pos = npc.Center + new Vector2(Main.rand.NextFloat(-npc.width / 2, (npc.width / 2) + 1), Main.rand.NextFloat(10f, 17.2f) - (npc.height / 2f));
                    Vector2 vel = -Vector2.UnitY * Main.rand.NextFloat(0.3f, 1.2f);

                    ParticleHandler.SpawnParticle(new GlowyBall(Main.rand.NextVector2FromRectangle(npc.getRect()), vel, Color.DarkViolet, Color.Violet, Main.rand.Next(19, 27), 1f, -0.3f, Main.rand.NextFloat(0.8f, 1.5f), 0f, Vector2.One, 0.99f, 0.002f) { BloomColor = Color.Purple, DrawWithBloom = true });

                    CreateDust(DustType<PurpurineDust>(), (vel * Main.rand.NextFloat(2.4f, 4f)).RotatedByRandom(Pi / 2.5f), Main.rand.NextVector2FromRectangle(npc.getRect()), Color.White, Main.rand.NextFloat(1.2f, 2.1f));
                }
            }

            else time = 0f;

            return base.PreAI(npc);
        }
    }
}
