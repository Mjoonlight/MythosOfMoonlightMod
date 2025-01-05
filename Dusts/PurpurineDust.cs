using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MythosOfMoonlight.Dusts
{
    public class PurpurineDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.noLight = true;
            // dust.scale *= 2f;
        }
        public override bool Update(Dust dust)
        {
            dust.rotation += 1f;
            dust.scale -= 0.05f;
            dust.position += dust.velocity;
            if (dust.scale <= 0)
                dust.active = false;

            Lighting.AddLight(dust.position, 0.5f * dust.scale, 0.7f * dust.scale, 1f * dust.scale);
            return false;
        }
        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(lightColor.R, lightColor.G, lightColor.B, 250);
    }
}