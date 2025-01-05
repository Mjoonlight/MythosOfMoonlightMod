using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MythosOfMoonlight.Dusts;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using MythosOfMoonlight.Common.Crossmod;
using MythosOfMoonlight.Items.PurpleComet.IridicSet;

namespace MythosOfMoonlight.Projectiles.IridicProjectiles
{
    public class PurpurineSaberSlice : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Iridic Saber");
            Main.projFrames[Projectile.type] = 10;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Projectile.AddElement(CrossModHelper.Celestial);
            Projectile.AddElement(CrossModHelper.Fire);
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 30;
            Projectile.netUpdate = true;
            Projectile.netUpdate2 = true;
            Projectile.netImportant = true;
            Projectile.ownerHitCheck = true;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.frameCounter++;
                Lighting.AddLight(Projectile.Center, .5f, .5f, .5f);
                foreach (Player player in Main.player)
                {
                    if (player == Main.player[Projectile.owner])
                    {
                        player.direction = Projectile.Center.X >= player.Center.X ? 1 : -1;
                        player.heldProj = Projectile.whoAmI;
                        Projectile.Center = player.Center + Utils.SafeNormalize(Main.MouseWorld - player.Center, Vector2.UnitX) * 28f;
                        Projectile.rotation = (Main.MouseWorld - player.Center).ToRotation();
                    }
                }
                if (Projectile.frameCounter % 4 == 0)
                    SoundEngine.PlaySound(SoundID.Item15, Projectile.Center);

                if (Projectile.frameCounter % 4 == 0)
                {
                    Vector2 dPos = Projectile.Center - new Vector2(20, 0).RotatedBy(Projectile.rotation);
                    Vector2 dVel = MathHelper.ToRadians(Main.rand.NextFloat(-30, 30)).ToRotationVector2().RotatedBy(Projectile.rotation) * 4f;
                    Dust dust = Dust.NewDustDirect(dPos, 1, 1, ModContent.DustType<PurpurineDust>());
                    dust.noGravity = true;
                    dust.velocity = dVel;
                    Vector2 dPos2 = Projectile.Center + new Vector2(Main.rand.NextFloat(-20, 20), Main.rand.NextFloat(-21, 21)).RotatedBy(Projectile.rotation);
                    Vector2 dVel2 = new Vector2(2, 0).RotatedBy(Projectile.rotation);
                    Dust dust1 = Dust.NewDustDirect(dPos2, 1, 1, ModContent.DustType<PurpurineDust>());
                    dust1.noGravity = true;
                    dust1.velocity = dVel2;
                }
                if (Projectile.frameCounter >= 40)
                    Projectile.frameCounter = 1;

                Projectile.frame = Projectile.frameCounter / 4;
                if (Main.player[Projectile.owner].channel && Main.player[Projectile.owner].HeldItem.type == ModContent.ItemType<PurpurineSaber>())
                    Projectile.timeLeft = 3;

                if (Projectile.frameCounter == 5 || Projectile.frameCounter == 25)
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(21, 0).RotatedBy(Projectile.rotation), new Vector2(8, 0).RotatedBy(Projectile.rotation), ModContent.ProjectileType<PurpurineSaberSlice>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.frame);
            }
            else
            {
                Projectile.usesLocalNPCImmunity = true;
                Projectile.localNPCHitCooldown = 20;
                Projectile.frame = (int)Projectile.ai[0];
                Projectile.velocity *= .96f;
                Projectile.alpha += 8;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] > 0)
            {
                Texture2D tex = ModContent.Request<Texture2D>("MythosOfMoonlight/Projectiles/IridicProjectiles/PurpurineSaberSlice").Value;
                Vector2 ori = new(20, 21);
                Rectangle rec = new(0, (int)Projectile.ai[0] * 42, 40, 42);
                float rot = Projectile.rotation;
                for (int i = 0; i <= 6; i += 1)
                {
                    Vector2 pos = Projectile.oldPos[i] + ori - Main.screenPosition;
                    Color color = Color.White * (float)((float)((float)Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * ((float)(255 - (float)Projectile.alpha) / 255f);
                    Main.EntitySpriteDraw(tex, pos, rec, color, rot, ori, 1f, SpriteEffects.None, 0);
                }
            }
            return true;
        }
    }
}
