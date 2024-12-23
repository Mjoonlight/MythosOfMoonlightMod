using MythosOfMoonlight.Common.Datastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Common.Utilities
{
    public static class MoMUtils
    {
        public static List<Vector2> GenerateCatmullRomPoints(IEnumerable<Vector2> origPath, int? precision = null)
        {
            int count = origPath.Count();

            if (count <= 2)
                return origPath.ToList();

            var path = new Vector2[count + 2];
            var it = origPath.GetEnumerator();
            int index = 0;

            while (it.MoveNext())
            {
                path[++index] = it.Current;
            }

            path[0] = path[1] * 2 - path[2];
            path[^1] = path[^2] * 2 - path[^3];

            List<Vector2> result = new(count * 3);

            for (int i = 1; i < count; i++)
            {
                float rotCurrent = new Rotation(path[i] - path[i - 1]).Radian;
                float rotNext = new Rotation(path[i + 2] - path[i + 1]).Radian;
                int denominator;

                if (precision is null)
                {
                    if (float.IsNaN(rotCurrent) || float.IsNaN(rotNext))
                    {
                        denominator = 2;
                    }

                    else
                    {
                        float dis = Math.Abs(rotCurrent - rotNext);
                        denominator = (int)((dis >= Pi ? TwoPi - dis : dis) / 0.22f + 2);
                    }
                }

                else
                {
                    denominator = precision.Value;
                }

                float factor = 1.0f / denominator;

                for (float j = 0; j < 1.0f; j += factor)
                {
                    result.Add(Vector2.CatmullRom(path[i - 1], path[i], path[i + 1], path[i + 2], j));
                }
            }

            result.Add(path[^2]);
            return result;
        }

        /// <summary>
        /// Normalizes a vector (magnitude => 1) with a fallback of (0, essentially 0) if the provided vector is of 0 magnitude or contains NaN's.
        /// </summary>
        /// <param name="v"></param>
        /// <returns>The normalized vector if it could have been, or a fallback of (0f, float.Epsilon) if it could not have been.</returns>
        public static Vector2 NormalizeBetter(Vector2 v)
        {
            return (v == Vector2.Zero || v.HasNaNs()) ? new Vector2(0f, float.Epsilon) : Vector2.Normalize(v);
        }

        /// <summary>
        /// Checks if a value is within the specified range. 
        /// </summary>
        /// <param name="f">The value to check.</param>
        /// <param name="min">The minumum value of the value to check.</param>
        /// <param name="max">The maximum value of the value to check.</param>
        /// <param name="inclusive">Whether or not to include <paramref name="max"/> and <paramref name="min"/> as acceptable values of <paramref name="f"/>.</param>
        /// <returns>True if it is, false if it isn't.</returns>
        public static bool WithinRange(this float f, float min, float max, bool inclusive = true, float offset = 0f)
        {
            if (inclusive)
            {
                return f >= min - offset && f <= max + offset;
            }

            else
            {
                return f > min - offset && f < max + offset;
            }
        }

        public static bool WithinRange(this int i, float min, float max, bool inclusive = true, int offset = 0)
        {
            if (inclusive)
            {
                return i >= min - offset && i <= max + offset;
            }

            else
            {
                return i > min - offset && i < max + offset;
            }
        }

        public static float Sin01(float x)
        {
            return Sin(x) * 0.5f + 0.5f;
        }

        public static float AbsSin(float t) => Abs(Sin(t));

        public static float PerlinNoise(float x)
        {
            float value = Sin01(2 * x) + Sin01(Pi * x);
            return value / 2f;
        }

        /// <summary>
        /// lerp between abs sine main time
        /// </summary>
        /// <returns></returns>
        public static Color LBASMT(Color a, Color b, float factor)
        {
            return Color.Lerp(a, b, AbsSin((float)Main.time) * factor);
        }

        public static Dust CreateDust(int type, Vector2 vel, Vector2 pos, Color col, float scale = 1f, int alpha = 0, bool rotate = false, bool noGrav = true)
        {
            var d = Dust.NewDustPerfect(pos, type);
            d.position = pos;
            d.velocity = !rotate ? vel : vel.RotatedByRandom(TwoPi);
            d.color = col;
            d.alpha = alpha;
            d.scale = scale;
            d.noGravity = noGrav;

            return d; //returns the dust so you can freely modify it afterwards if needed
        }

        public static Color PickRandom(Color[] options) => options[Main.rand.Next(options.Length)];

        public static void DrawSimpleTrail(this Projectile Projectile, Texture2D texture, Vector2 scale, Color startColor, Color endColor, float extraRot = 0f)
        {
            Texture2D afterimageTexture = texture;

            for (float i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += 0.5f)
            {
                float t = (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];

                Color color = Color.Lerp(startColor, endColor, t) * t;

                int max0 = (int)i - 1;

                if (max0 < 0)
                    continue;

                float rot = Projectile.oldRot[max0] + extraRot;
                Vector2 center = Vector2.Lerp(Projectile.oldPos[(int)i], Projectile.oldPos[max0], 1 - i % 1);
                center += Projectile.Size / 2;

                Main.EntitySpriteDraw(afterimageTexture, center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, color, rot, afterimageTexture.Size() / 2f, scale * Projectile.scale, SpriteEffects.None, 0);
            }
        }

        public static void SimpleDrawProjectile(this Projectile Projectile, Texture2D texture, Color Color, bool IsGlow, float scaleMod = 1f, float extraRot = 0f)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new(0, startY, texture.Width, frameHeight);

            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPos, new Microsoft.Xna.Framework.Rectangle?(sourceRectangle), IsGlow ? Color : Projectile.GetAlpha(Color), Projectile.rotation + extraRot, origin, Projectile.scale * scaleMod, spriteEffects, 0);
        }

        public static void SimpleDrawProjectile_Offset(this Projectile Projectile, Texture2D texture, Vector2 drawOffset, Color Color, bool IsGlow, float scaleMod = 1f, float extraRot = 0f)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new(0, startY, texture.Width, frameHeight);

            Vector2 origin = sourceRectangle.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPos + drawOffset, new Microsoft.Xna.Framework.Rectangle?(sourceRectangle), IsGlow ? Color : Projectile.GetAlpha(Color), Projectile.rotation + extraRot, origin, Projectile.scale * scaleMod, spriteEffects, 0);
        }

        public static void ManageHeldProj(this Projectile Projectile, Vector2 armPos, HeldprojSettings settings)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 oldVelocity = Projectile.velocity;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Main.MouseWorld), Utils.GetLerpValue(5f, 55f, Projectile.Distance(Main.MouseWorld), true)).RotatedBy(settings.AimOffset);

                if (Projectile.velocity != oldVelocity)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }

            Projectile.Center = armPos + Projectile.velocity * settings.HoldOffset;
            Projectile.rotation = Projectile.velocity.ToRotation() + settings.RotationOffset;
            Projectile.spriteDirection = Projectile.direction;

            settings.Owner.ChangeDir(Projectile.direction);
            settings.Owner.heldProj = Projectile.whoAmI;
            settings.Owner.itemTime = 2;
            settings.Owner.itemAnimation = 2;
            settings.Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        public static string TryGetTextureFromOther<T>() where T : ModType => GetInstance<T>().GetType().Namespace.Replace(".", "/") + "/" + GetInstance<T>().Name;
    }
}
