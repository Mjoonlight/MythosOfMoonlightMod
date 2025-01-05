using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace MythosOfMoonlight.Items.Weapons.Ranged
{
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
            float rotation = MathHelper.ToRadians(7);
            for (int i = 0; i < numberProjectiles; i++)
            {
                Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (numberProjectiles - 1))) * .5f; // Watch out for dividing by 0 if there is only 1 projectile.
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
            catchLocation = "Caught in the ocean.";
        }

        public override Vector2? HoldoutOffset()
        {
            var offset = new Vector2(0, 0);
            return offset;
        }
    }
}
