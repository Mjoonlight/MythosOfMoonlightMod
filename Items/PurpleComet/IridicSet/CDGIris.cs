using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using MythosOfMoonlight.Projectiles.IridicProjectiles;
using MythosOfMoonlight.Items.Materials;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using MythosOfMoonlight.Dusts;
using Microsoft.Xna.Framework.Graphics;

namespace MythosOfMoonlight.Items.PurpleComet.IridicSet
{
    public class CDGIris : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("CDG-Iris");
            // Tooltip.SetDefault("Cosmos Derived Gun? Cosmos Derived Fun.");
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.noUseGraphic = false;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.width = 46;
            Item.height = 22;
            Item.useTime = 5;
            SoundStyle style = new SoundStyle("MythosOfMoonlight/Assets/Sounds/cdg");
            //style.Volume = .5f;
            style.MaxInstances = 400;

            Item.UseSound = style;
            Item.useAnimation = 15;
            Item.reuseDelay = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAmmo = AmmoID.Bullet;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(0, 0, 0, 1);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<FragmentBullet>();
            Item.shootSpeed = 1;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Helper.GetTex(Texture + "_Glow");
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }
        public override Vector2? HoldoutOffset() => new Vector2(0, 0);
        public override bool RangedPrefix()
        {
            return true;
        }
        public override bool AllowPrefix(int pre)
        {
            if (pre == PrefixID.Unreal) return true;
            return base.AllowPrefix(pre);
        }
        /*public override void UseAnimation(Player player)
        {
            Item.useAnimation = Item.useTime * 3;
        }*/
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            //if (type == ProjectileID.Bullet)
            if (player.itemAnimation > 5)
                velocity = velocity.RotatedByRandom(MathHelper.PiOver4 / 5);
            //velocity.Normalize();

            position += new Vector2(0, -4).RotatedBy(velocity.ToRotation()) * player.direction;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<FragmentBullet>(), damage, knockback, player.whoAmI);
            Helper.SpawnDust(position + new Vector2(50, 0).RotatedBy(velocity.ToRotation()), Vector2.One, ModContent.DustType<PurpurineDust>(), velocity * 3, 5);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = Recipe.Create(Type)
                .AddIngredient(ModContent.ItemType<PurpurineQuartz>(), 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
