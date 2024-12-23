using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Dusts
{
    public class Smoke : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(0.8f, 2f);
            dust.frame = new Rectangle(0, Main.rand.Next(0, 2) * 36, 34, 36);
            dust.rotation = Main.rand.NextFloat(6.28f);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            Color gray = new (25, 25, 25);
            Color black = Color.Black;
            Color ret;

            if (dust.alpha < 120)
                ret = Color.Lerp(dust.color, gray, dust.alpha / 120f);

            else if (dust.alpha < 180)
                ret = Color.Lerp(gray, black, (dust.alpha - 120) / 60f);

            else
                ret = black;

            return ret * ((255 - dust.alpha) / 255f);
        }

        public override bool Update(Dust dust)
        {
            dust.velocity *= 0.98f;
            dust.velocity.X *= 0.967f;
            dust.color *= 0.98f;

            if (dust.alpha > 100)
            {
                dust.scale *= 0.97545f;
                dust.alpha += 2;
            }

            else
            {
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.12f);
                dust.scale *= 0.9875f;
                dust.alpha += 4;
            }

            dust.position += dust.velocity;
            dust.rotation += 0.004f;

            if (dust.alpha >= 255 || dust.scale <= 0.001f)
                dust.active = false;

            return false;
        }
    }
}
