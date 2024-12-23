using Microsoft.Xna.Framework;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Systems;
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
            Item.damage = 38;
            Item.crit = 8;
            Item.knockBack = 3f;
            Item.shoot = ProjectileType<CrawshotProj>();
            Item.shootSpeed = 1f;

            Item.useTurn = false;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;

            Item.value = Item.sellPrice(0, 2, 75, 0);
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

        public float FiringOrder = 0;

        public bool die = false;

        public float[] CannonScaleInterpolant = [0f, 0f, 0f];

        public Color[] CannonColor = [Color.Transparent, Color.Transparent, Color.Transparent]; //lerp to a hot color when firing!!!

        public bool[] CannonJustFiredAndIsCooling = [false, false, false]; //so they can each manage their own color and scale

        public override void SetStaticDefaults()
        {
            cannon1 = Request<Texture2D>(Texture + "_Cannon1").Value;
            cannon2 = Request<Texture2D>(Texture + "_Cannon2").Value;
            cannon3 = Request<Texture2D>(Texture + "_Cannon3").Value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 30;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.netImportant = true;
            Projectile.Opacity = 0f;
        }

        public override bool? CanDamage() => false;

        public Vector2[] tipPositions = [Vector2.Zero, Vector2.Zero, Vector2.Zero];

        public override void AI()
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            if (Owner.dead || !Owner.active || Owner.noItems || Owner.CCed)
                Projectile.Kill();

            Projectile.damage = Owner.GetWeaponDamage(Owner.HeldItem);
            Projectile.CritChance = Owner.GetWeaponCrit(Owner.HeldItem);
            Projectile.knockBack = Owner.GetWeaponKnockback(Owner.HeldItem, Owner.HeldItem.knockBack);

            Vector2 armPos = Owner.RotatedRelativePoint(Owner.MountedCenter, false, true);

            if (!die)
                Projectile.Opacity = Lerp(Projectile.Opacity, 1f, 0.12f);
        
            //top
            tipPositions[0] = (armPos + Projectile.velocity * Projectile.width * 0.7f) + new Vector2(0f, -10f).RotatedBy(Projectile.velocity.ToRotation());

            //big middle
            tipPositions[1] = (armPos + Projectile.velocity * Projectile.width * 0.785f);

            //bottom
            tipPositions[2] = (armPos + Projectile.velocity * Projectile.width * 0.7f) + new Vector2(0f, 10f).RotatedBy(Projectile.velocity.ToRotation());

            Projectile.ManageHeldProj(armPos, new HeldprojSettings(12f, (Projectile.spriteDirection == -1 ? Pi : 0f), Owner, 0f));

            if (Owner.channel && !die)
            {
                Projectile.timeLeft++;

                for (int i = 0; i < tipPositions.Length; i++)
                {
                    CreateDust(DustID.GemDiamond, Vector2.Zero, tipPositions[i], default);
                }
            }

            if(!Owner.channel || die)
            {
                die = true;

                Projectile.Opacity = Lerp(Projectile.Opacity, 0f, 0.3f);

                if (++KillTimer >= 16)
                    Projectile.Kill();
            }
        }

        #region drawing

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.SimpleDrawProjectile(TextureAssets.Projectile[Type].Value, lightColor * Projectile.Opacity, true, 1f);

            cannon1 ??= Request<Texture2D>(Texture + "_Cannon1").Value;
            cannon2 ??= Request<Texture2D>(Texture + "_Cannon2").Value;
            cannon3 ??= Request<Texture2D>(Texture + "_Cannon3").Value;

            Projectile.SimpleDrawProjectile(cannon1, lightColor * Projectile.Opacity, true, 1f + CannonScaleInterpolant[0]);
            Projectile.SimpleDrawProjectile(cannon2, lightColor * Projectile.Opacity, true, 1f + CannonScaleInterpolant[1]);
            Projectile.SimpleDrawProjectile(cannon3, lightColor * Projectile.Opacity, true, 1f + CannonScaleInterpolant[2]);

            VFXManager.DrawCache.Add(() =>
            {
                Projectile.SimpleDrawProjectile(cannon1, CannonColor[0] * Projectile.Opacity, true, 1f + CannonScaleInterpolant[0]);
                Projectile.SimpleDrawProjectile(cannon2, CannonColor[1] * Projectile.Opacity, true, 1f + CannonScaleInterpolant[1]);
                Projectile.SimpleDrawProjectile(cannon3, CannonColor[2] * Projectile.Opacity, true, 1f + CannonScaleInterpolant[2]);
            });

            return false;
        }

        #endregion
    }
}
