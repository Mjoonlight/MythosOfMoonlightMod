using Microsoft.Xna.Framework;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Systems;
using MythosOfMoonlight.Common.Utilities;
using MythosOfMoonlight.Dusts;
using System.Drawing.Drawing2D;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace MythosOfMoonlight.Items.Weapons.Ranged
{
    /*

    public class Crawshot : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Crawshot");
            // Tooltip.SetDefault("Triple-shot, Crawshot, Target-shot");

            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 15;
            Item.height = 15;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 0.87f;
            Item.value = Item.sellPrice(silver: 65); 
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Bullet;
            Item.noMelee = true;
            Item.shootSpeed = 55f;

            Item.shoot = ProjectileID.Bullet;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 25f;

            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
                position += muzzleOffset;

            position += Vector2.Normalize(velocity) * -2f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float numberProjectiles = 3;
            float rotation = ToRadians(7);

            for (int i = 0; i < numberProjectiles; i++)
            {
                Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))) * .5f;

                for (int j = 0; j < 5; j++)
                {
                    var d = Dust.NewDustPerfect(position, DustID.Water, perturbedSpeed.RotatedByRandom(0.2f).SafeNormalize(Vector2.UnitY) * 5, Scale: Main.rand.NextFloat(0.5f, 1f));
                }

                Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override bool IsQuestFish()
        {
            return true;
        }

        public override bool IsAnglerQuestAvailable()
        {
            return !Main.hardMode;
        }

        public override void AnglerQuestChat(ref string description, ref string catchLocation)
        {
            description = "I'm no expert with guns and weaponry, but I'm positive I saw a lil' crustacean with a couple of gun barrels stuck to its mouth! Can you catch it so i dont have to worry about being shot by a lobster?";
            catchLocation = "Caught in the Ocean.";
        }

        public override Vector2? HoldoutOffset()
        {
            var offset = new Vector2(0, 0);
            return offset;
        }
    }

    */

    public class Crawshot : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 30;

            Item.useTime = Item.useAnimation = 20;

            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 20; //kinda slow so more dmg
            Item.crit = 8;
            Item.knockBack = 3f;
            Item.shoot = ProjectileType<CrawshotProj>();
            Item.shootSpeed = 1f;

            Item.useTurn = false;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;

            Item.value = Item.sellPrice(0, 0, 65, 0);
            Item.useAmmo = AmmoID.Bullet; //forgor
        }

        public override bool IsQuestFish()
        {
            return true;
        }

        public override bool IsAnglerQuestAvailable()
        {
            return !Main.hardMode;
        }

        public override void AnglerQuestChat(ref string description, ref string catchLocation)
        {
            description = "I'm no expert with guns and weaponry, but I'm positive I saw a lil' crustacean with a couple of gun barrels stuck to its mouth! Can you catch it so i dont have to worry about being shot by a lobster?";
            catchLocation = "Caught in the Ocean.";
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ProjectileType<CrawshotProj>()] < 1;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, velocity, ProjectileType<CrawshotProj>(), damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class CrawshotProj : ModProjectile
    {
        public override string Texture => TryGetTextureFromOther<Crawshot>();

        public ref float Time => ref Projectile.ai[0];

        public ref float KillTimer => ref Projectile.ai[1];

        private Player Owner => Main.player[Projectile.owner];

        private Texture2D cannon1, cannon2, cannon3;
        private Texture2D cannon1W, cannon2W, cannon3W;

        public int FiringOrder = -1;

        public bool die = false;

        public float[] CannonManagementTime = [0f, 0f, 0f];

        public float[] CannonScaleInterpolant = [0f, 0f, 0f];

        public Color[] CannonColor = [Color.Transparent, Color.Transparent, Color.Transparent]; //lerp to a hot color when firing!!!

        public bool[] CannonJustFiredAndIsCooling = [false, false, false]; //so they can each manage their own color and scale

        public float TimeToFire = 36f;

        public float RotOffset = 0f;

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

        public Vector2[] tipPositions = [Vector2.Zero, Vector2.Zero, Vector2.Zero];

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
                Projectile.Opacity = Lerp(Projectile.Opacity, 1f, 0.12f);
        
            //top
            tipPositions[0] = (armPos + Projectile.velocity * Projectile.width * 0.7f) + new Vector2(0f, -10f * Projectile.direction).RotatedBy(Projectile.velocity.ToRotation());

            //big middle
            tipPositions[1] = (armPos + Projectile.velocity * Projectile.width * 0.785f);

            //bottom
            tipPositions[2] = (armPos + Projectile.velocity * Projectile.width * 0.7f) + new Vector2(0f, 10f * Projectile.direction).RotatedBy(Projectile.velocity.ToRotation());

            Projectile.ManageHeldProj(armPos, new HeldprojSettings(12f, (Projectile.spriteDirection == -1 ? Pi : 0f), Owner, RotOffset));

            if (Owner.channel && !die)
            {
                Projectile.timeLeft++;

                if(++Time >= TimeToFire + 1)
                {
                    int index = ++FiringOrder;

                    Vector2 pos = tipPositions[index];
                    Vector2 vel = pos.DirectionTo(Main.MouseWorld);

                    if (!ConsumeAmmo(Owner.HeldItem, Owner, out int to, out int the, 1, true))
                    {
                        die = true;
                        return;
                    }

                    ConsumeAmmo(Owner.HeldItem, Owner, out int teAmo, out int ammoDmg, 1, false);

                    SoundEngine.PlaySound(SoundID.Item40 with { Pitch = -0.3f, Volume = 0.8f }, pos);
                    SoundEngine.PlaySound(SoundID.Item42 with { Pitch = -0.2f, Volume = 0.45f }, pos);

                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, vel * 12f, teAmo, Projectile.damage + ammoDmg, 2f, Projectile.owner); //16f is the max for 1 update
                                                                                                                                                       //they have like 3 or smth i forgor
                    for(int i = 0; i < 4; i++)
                    {
                        Vector2 pos2 = pos + vel * Main.rand.NextFloat(1.02f, 1.35f);
                        Vector2 vel2 = NormalizeBetter(Projectile.velocity) * 5f * Main.rand.NextFloat(0.3f, 1f);

                        var d = CreateDust(DustType<StretchyGlow>(), vel2.RotatedByRandom(Pi * 0.285f), pos2, Color.Lerp(Color.OrangeRed, Color.LightYellow, 0.4875f), Main.rand.NextFloat(0.24f, 0.5f));
                        d.customData = new Vector2(1f, Main.rand.NextFloat(0.6f, 0.78f));

                        var d2 = CreateDust(DustType<StretchyGlow>(), vel2.RotatedByRandom(Pi * 0.1285f) * 0.1f, pos2, Color.Lerp(Color.OrangeRed, Color.LightYellow, 0.14875f), Main.rand.NextFloat(0.4f, 0.75f));
                        d2.customData = new Vector2(1f, Main.rand.NextFloat(0.6f, 0.8f));

                        //if(i == 0)
                        //CreateDust(DustType<Smoke>(), vel2.RotatedByRandom(Pi * 0.01285f) * 0.01125f, pos + vel * 0.18f, Color.Lerp(Color.OrangeRed, Color.LightYellow, 0.4745f), Main.rand.NextFloat(0.3f, 0.6f));
                    }

                    CannonJustFiredAndIsCooling[index] = true;

                    Time = 0f;

                    if (FiringOrder >= 2) //why does it randomly choose to go above 2
                        FiringOrder = -1;
                }
            }

            if(!Owner.channel || die)
            {
                die = true;

                Projectile.Opacity = Lerp(Projectile.Opacity, 0f, 0.3f);

                if (++KillTimer >= 16)
                    Projectile.Kill();
            }

            ManageCannons();
        }

        void ManageCannons()
        {
            for (int i = 0; i < CannonJustFiredAndIsCooling.Length; i++)
            {
                //Main.NewText($"cannon {i}:" + CannonJustFiredAndIsCooling[i]);

                if (CannonJustFiredAndIsCooling[i])
                {
                    CannonManagementTime[i]++;

                    if (CannonManagementTime[i] > 36) //end this cannon's shenanigans
                    {
                        CannonColor[i] = Color.Transparent;
                        CannonManagementTime[i] = 0f;
                        Projectile.netUpdate = true;
                    }

                    if (CannonManagementTime[i] >= 20) //out
                    {
                        CannonScaleInterpolant[i] = Lerp(CannonScaleInterpolant[i], 0f, 0.14f);
                        CannonColor[i] = Color.Lerp(CannonColor[i], Color.Transparent, 0.14f);
                        RotOffset = Lerp(RotOffset, 0f, 0.1f);
                    }

                    if (CannonManagementTime[i] < 20) //lerp in
                    {
                        CannonScaleInterpolant[i] = Lerp(CannonScaleInterpolant[i], 0.15f, 0.1f);
                        CannonColor[i] = Color.Lerp(CannonColor[i], Color.Lerp(Color.OrangeRed, Color.LightYellow, 0.3f), 0.1f);
                        RotOffset = Lerp(RotOffset, -PiOver4 * 0.35f * Projectile.direction, 0.1f);
                    }
                }

                if (CannonManagementTime[i] >= 36f)
                {
                    CannonColor[i] = Color.Transparent;
                    CannonManagementTime[i] = 0f;
                    CannonJustFiredAndIsCooling[i] = false;
                    RotOffset = 0f;
                    Projectile.netUpdate = true;
                }
            }
        }

        #region drawing

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.SimpleDrawProjectile(TextureAssets.Projectile[Type].Value, lightColor * Projectile.Opacity, true, 1f);

            cannon1 ??= Request<Texture2D>(Texture + "_Cannon1").Value;
            cannon2 ??= Request<Texture2D>(Texture + "_Cannon2").Value;
            cannon3 ??= Request<Texture2D>(Texture + "_Cannon3").Value;

            cannon1W ??= Request<Texture2D>(Texture + "_Cannon1_White").Value;
            cannon2W ??= Request<Texture2D>(Texture + "_Cannon2_White").Value;
            cannon3W ??= Request<Texture2D>(Texture + "_Cannon3_White").Value;

            VFXManager.DrawCache.Add(() =>
            {
                Projectile.SimpleDrawProjectile(cannon1W, CannonColor[0] * Projectile.Opacity, true, 1f + CannonScaleInterpolant[0]);
                Projectile.SimpleDrawProjectile(cannon2W, CannonColor[1] * Projectile.Opacity, true, 1f + CannonScaleInterpolant[1]);
                Projectile.SimpleDrawProjectile(cannon3W, CannonColor[2] * Projectile.Opacity, true, 1f + CannonScaleInterpolant[2]);
            });

            Projectile.SimpleDrawProjectile(cannon1W, CannonColor[0] * Projectile.Opacity, true, 1.01f + CannonScaleInterpolant[0]);
            Projectile.SimpleDrawProjectile(cannon2W, CannonColor[1] * Projectile.Opacity, true, 1.01f + CannonScaleInterpolant[1]);
            Projectile.SimpleDrawProjectile(cannon3W, CannonColor[2] * Projectile.Opacity, true, 1.01f + CannonScaleInterpolant[2]);

            Projectile.SimpleDrawProjectile(cannon1, lightColor * Projectile.Opacity, true, 1f + CannonScaleInterpolant[0]);
            Projectile.SimpleDrawProjectile(cannon2, lightColor * Projectile.Opacity, true, 1f + CannonScaleInterpolant[1]);
            Projectile.SimpleDrawProjectile(cannon3, lightColor * Projectile.Opacity, true, 1f + CannonScaleInterpolant[2]);

            return false;
        }

        #endregion
    }
}
