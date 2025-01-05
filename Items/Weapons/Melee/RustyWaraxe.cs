using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using rail;
using MythosOfMoonlight.Projectiles;
using Terraria.Audio;
using MythosOfMoonlight.Buffs;

namespace MythosOfMoonlight.Items.Weapons.Melee
{
    public class RustyWaraxe : ModItem
    {
        public override void SetDefaults()
        {
            Item.knockBack = 10f;
            Item.width = 54;
            Item.height = 54;
            Item.crit = 10;
            Item.damage = 15;
            Item.useAnimation = 32;
            Item.useTime = 32;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.channel = true;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.rare = ItemRarityID.Green;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<RustyWaraxeP>();

            Item.value = Item.buyPrice(0, 1, 50, 0);
        }
        int dir = 1;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            dir = -dir;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, dir);
            return false;
        }
    }
    public class RustyWaraxeP : HeldSword
    {
        public override string Texture => "MythosOfMoonlight/Items/Weapons/Melee/RustyWaraxe";
        public override void SetExtraDefaults()
        {
            swingTime = 30;
            holdOffset = 38;
            Projectile.Size = new(54, 54);
        }
        public override float Ease(float x)
        {
            return (float)(x == 0
  ? 0
  : x == 1
  ? 1
  : x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2
  : (2 - Math.Pow(2, -20 * x + 10)) / 2);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.Next(100) < 15)
            {
                //SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/rustyAxe"), Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item171, Projectile.Center);
                for (int i = 0; i < 40; i++)
                    Dust.NewDust(target.position, target.width, target.height, DustID.Blood, Helper.FromAToB(Projectile.Center, target.Center).X * Main.rand.NextFloat(-10, 10), Helper.FromAToB(Projectile.Center, target.Center).Y * Main.rand.NextFloat(-10, 10), newColor: Color.Brown);
                target.AddBuff(ModContent.BuffType<RustyCut>(), 120);
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item1);
        }
    }
}
