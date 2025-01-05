using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using MythosOfMoonlight.Projectiles;
using MythosOfMoonlight.Projectiles.VFXProjectiles;

namespace MythosOfMoonlight.Items.Weapons.Ranged
{
    public class Starcaller : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 66;
            Item.crit = 5;
            Item.damage = 17;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.channel = true;
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.LightRed;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<StarcallerP>();
            Item.useAmmo = AmmoID.Arrow;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return false;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<StarcallerP>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }
    }
    public class StarcallerP : ModProjectile
    {
        const float holdOffset = 20;
        const int maxTime = 42;
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.Size = new(28, 66);
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.timeLeft = maxTime;
        }
        public override bool? CanDamage() => false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DontCancelChannelOnKill[Type] = true;
        }
        public virtual float Ease(float x)
        {
            return 1 - MathF.Pow(1 - x, 5);
        }
        public virtual float ScaleFunction(float progress)
        {
            return 0.7f + (float)Math.Sin(progress * Math.PI) * 0.5f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            float off = MathHelper.Lerp(2, 0, Projectile.ai[2] / 30);
            if (Projectile.ai[2] > 30)
                off = 0;
            Utils.DrawLine(Main.spriteBatch, Projectile.Center - new Vector2(1, 30).RotatedBy(Projectile.rotation), Projectile.Center - new Vector2(5 + Projectile.ai[2], -off).RotatedBy(Projectile.rotation), new Color(45, 67, 137), new Color(45, 67, 137), 2);
            Utils.DrawLine(Main.spriteBatch, Projectile.Center - new Vector2(3, -30).RotatedBy(Projectile.rotation), Projectile.Center - new Vector2(5 + off + Projectile.ai[2], off).RotatedBy(Projectile.rotation), new Color(45, 67, 137), new Color(45, 67, 137), 2);

            Texture2D tex = Helper.GetTex("MythosOfMoonlight/Projectiles/StarcallerBolt");

            Main.spriteBatch.Draw(tex, Projectile.Center + Vector2.Lerp(new Vector2(20, 0).RotatedBy(Projectile.rotation), Vector2.Zero, arrowAlpha) - Main.screenPosition, null, Color.White * arrowAlpha, Projectile.rotation - MathHelper.PiOver2, tex.Size() / 2, 1, SpriteEffects.None, 0);
            return true;
        }
        float arrowAlpha;
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.25f, 0.25f, 0.35f);
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.CCed || player.noItems)
            {
                Projectile.Kill();
                return;
            }
            else
            {
                if (Projectile.timeLeft == 1 && player.channel)
                {
                    Projectile.timeLeft = maxTime;
                }
            }
            Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
            Vector2 pos = player.RotatedRelativePoint(player.MountedCenter);
            player.ChangeDir(Projectile.velocity.X < 0 ? -1 : 1);
            player.itemRotation = (Projectile.velocity.ToRotation() + Projectile.ai[0]) * player.direction;
            pos += (Projectile.velocity.ToRotation()).ToRotationVector2() * holdOffset;
            Projectile.Center = pos;
            Projectile.rotation = Projectile.velocity.ToRotation();
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
            player.SetCompositeArmBack(true, Projectile.timeLeft > 25 || Projectile.timeLeft < 2 ? Player.CompositeArmStretchAmount.Quarter : Player.CompositeArmStretchAmount.None, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
            if (Projectile.timeLeft > 12)
            {
                if (Projectile.timeLeft == maxTime - 1)
                {
                    bool success = false;
                    for (int j = 54; j < 58; j++)
                    {
                        if (player.inventory[j].ammo == player.HeldItem.useAmmo && player.inventory[j].stack > 0)
                        {
                            if (player.inventory[j].maxStack > 1)
                                player.inventory[j].stack--;
                            success = true;
                            break;
                        }
                    }
                    if (!success)
                    {
                        Projectile.Kill();
                        return;
                    }
                }
                if (Projectile.timeLeft == maxTime - 3)
                    SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/bowPull") { PitchVariance = 0.25f, Volume = 0.7f }, Projectile.Center);
                float progress = Ease(Utils.GetLerpValue(0f, maxTime - 12, Projectile.timeLeft));
                arrowAlpha = MathHelper.Lerp(arrowAlpha, 1f, 0.1f);
                if (Projectile.timeLeft > 25 && Projectile.timeLeft < maxTime - 1)
                    Projectile.ai[2] += 0.5f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Helper.FromAToB(player.Center, Main.MouseWorld), 0.5f - MathHelper.Lerp(0, 0.2f, Projectile.timeLeft / 15)).SafeNormalize(Vector2.UnitX);
            }
            else
            {
                if (Projectile.timeLeft == 12)
                {
                    SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/bowRelease") { PitchVariance = 0.25f, Volume = 0.7f }, Projectile.Center);
                    SoundEngine.PlaySound(new SoundStyle("MythosOfMoonlight/Assets/Sounds/miscMagicPulse") { PitchVariance = 0.25f, Pitch = 0.5f, Volume = 0.25f }, Projectile.Center);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 3.5f, ModContent.ProjectileType<StarcallerBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity * 15, Projectile.velocity, ModContent.ProjectileType<StarcallerShotVFX>(), 0, 0, Projectile.owner);

                    arrowAlpha = 0;
                }
                if (Projectile.timeLeft > 5)
                    Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], -5, 0.7f);
                else
                    Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 0, 0.4f);
            }
        }
    }
}
