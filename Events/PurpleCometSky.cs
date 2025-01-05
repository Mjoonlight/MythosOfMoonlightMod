using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MythosOfMoonlight.Projectiles;
using rail;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.Utilities;
using static tModPorter.ProgressUpdate;

namespace MythosOfMoonlight.Events
{
    public class PurpleCometSky : CustomSky
    {
        public bool isActive;
        public float Intensity;
        int innerSeed = 0, innerSeed2 = 0, outerSeed = 0;
        float mainAlpha, progress, yOffset;
        public override void Activate(Vector2 position, params object[] args)
        {
            innerSeed = Main.rand.Next(int.MaxValue);
            innerSeed2 = Main.rand.Next(int.MaxValue);
            outerSeed = Main.rand.Next(int.MaxValue / 2);
            linesGlowOuter = 0.3f;
            linesGlowInner2 = -0.5f;
            isActive = true;
        }
        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }
        public override void Reset()
        {
            isActive = false;
        }
        public override void Update(GameTime gameTime)
        {
            if (isActive)
            {
                Intensity = Math.Min(1f, 0.01f + Intensity);
            }
            else
            {
                Intensity = Math.Max(0f, Intensity - 0.01f);
            }
            progress = Ease(MathHelper.SmoothStep(0, 2, (float)(Main.time) / ((float)Main.nightLength)));
            mainAlpha = MathHelper.Clamp((float)Math.Sin(progress * Math.PI * 0.5f), 0, 1);
            float _progress = MathHelper.SmoothStep(0, 1, (float)(Main.time) / ((float)Main.nightLength));
            float yOffset_T = MathHelper.Clamp((float)Math.Sin(_progress * Math.PI) * 0.5f, -.5f, .5f);
            yOffset = 100 + 400 * yOffset_T - Main.screenPosition.Y * 0.02f;
            linesGlowInner += 0.005f;
            linesGlowInner2 += 0.005f;
            linesGlowOuter += 0.0025f;
            if (linesGlowOuter > .75f)
            {
                linesGlowOuter = 0f;
                outerSeed = Main.rand.Next(int.MaxValue);
            }

            if (linesGlowInner > .75f)
            {
                linesGlowInner = 0f;
                innerSeed = Main.rand.Next(int.MaxValue);
            }
            if (linesGlowInner2 > .75f)
            {
                linesGlowInner2 = 0f;
                innerSeed2 = Main.rand.Next(int.MaxValue);
            }
        }
        public override bool IsActive()
        {
            return Intensity > 0;
        }
        float glow, linesGlowInner, linesGlowInner2, linesGlowOuter = 0.5f;
        //float intensity;
        float Ease(float x)
        {
            return x < 0.5 ? 2 * x * x : 1 - MathF.Pow(-2 * x + 2, 2) / 2;
        }
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 3.40282347E+38f && minDepth < 3.40282347E+38f)
            {
                float scaleAdd = mainAlpha * 0.1f;
                Texture2D Tex = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Sky").Value;
                Texture2D starTex = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/flare").Value;
                Texture2D starTex2 = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/flameEye2").Value;
                Texture2D starTex3 = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/Circle").Value;
                Texture2D comet = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/comet_tail2").Value;
                Vector2 Pos = new(Main.screenWidth / 2, Main.screenHeight / 2);
                //int cometX = (int)((Main.time) / ((float)Main.nightLength ).0 * (double)(scen.totalWidth + (float)(comet.Width * 2))) - comet.Width;
                Vector2 cometP = Vector2.SmoothStep(new Vector2(Main.screenWidth - 75, yOffset), new Vector2(-30, yOffset), Ease((float)(Main.time) / ((float)Main.nightLength)));
                //new(Main.screenWidth / 4, MathHelper.Lerp(-200, Main.screenHeight + comet.Height * 0.5f, (float)(Main.time) / ((float)Main.nightLength )));
                if (Main.screenWidth > Tex.Width || Main.screenHeight > Tex.Height)
                    spriteBatch.Draw(Tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), null, Color.White * Intensity * mainAlpha * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 0);
                else
                    spriteBatch.Draw(Tex, Pos, null, Color.White * Intensity * mainAlpha * 0.5f, 0f, new Vector2(Tex.Width >> 1, Tex.Height >> 1), 1f, SpriteEffects.None, 1f);
                spriteBatch.Reload(BlendState.Additive);
                glow += Main.rand.NextFloat(-.1f, .1f);
                glow = MathHelper.Clamp(glow, 0, 1);
                //spriteBatch.Draw(comet, cometP, null, Color.White * Intensity * 0.65f * ((((float)Math.Sin((double)Main.GlobalTimeWrappedHourly) + 1) * 0.5f) + 0.4f), MathHelper.ToRadians(245), new Vector2(comet.Width / 2, 0.15f), 1, SpriteEffects.None, 0f);
                //spriteBatch.Draw(comet, cometP, null, Color.White * Intensity * 0.85f, MathHelper.ToRadians(245), new Vector2(comet.Width / 2, 0.25f), 0.95f, SpriteEffects.None, 0f);

                DrawTrail();

                Vector2 starOffset = new Vector2(35, -14f);

                spriteBatch.Draw(starTex, cometP + starOffset, null, Color.DarkViolet * mainAlpha * mainAlpha * Intensity * 0.1f * ((((float)Math.Sin((double)Main.GlobalTimeWrappedHourly) + 1) * 0.5f) + 0.4f + glow), Main.GameUpdateCount * 0.0035f, starTex.Size() / 2, .45f + glow * 0.1f + scaleAdd, SpriteEffects.None, 0f);
                spriteBatch.Draw(starTex, cometP + starOffset, null, Color.DarkViolet * mainAlpha * mainAlpha * Intensity * 0.35f, Main.GameUpdateCount * -0.0035f, starTex.Size() / 2, .45f + glow * 0.1f + scaleAdd, SpriteEffects.None, 0f);

                spriteBatch.Draw(starTex, cometP + starOffset, null, Color.DarkViolet * mainAlpha * mainAlpha * Intensity, 0, starTex.Size() / 2, .15f + scaleAdd, SpriteEffects.None, 0f);

                spriteBatch.Draw(starTex, cometP + starOffset, null, Color.White * mainAlpha * mainAlpha * Intensity, 0, starTex.Size() / 2, .025f + scaleAdd, SpriteEffects.None, 0f);
                spriteBatch.Draw(starTex, cometP + starOffset, null, Color.White * 0.75f * mainAlpha * mainAlpha * Intensity, 0, starTex.Size() / 2, .05f + scaleAdd, SpriteEffects.None, 0f);
                spriteBatch.Draw(starTex, cometP + starOffset, null, Color.White * 0.5f * mainAlpha * mainAlpha * Intensity, 0, starTex.Size() / 2, .075f + scaleAdd, SpriteEffects.None, 0f);

                spriteBatch.Draw(starTex3, cometP + starOffset, null, Color.White * mainAlpha * 0.65f * Intensity, 0, starTex3.Size() / 2, .5f + scaleAdd, SpriteEffects.None, 0f);

                //spriteBatch.Draw(starTex2, cometP + starOffset, null, Color.White * mainAlpha * 0.15f * Intensity, 0, starTex2.Size() / 2, 2.15f + scaleAdd, SpriteEffects.None, 0f);

                spriteBatch.Draw(starTex2, cometP + starOffset, null, Color.DarkViolet * mainAlpha * mainAlpha * Intensity * 0.1f * ((((float)Math.Sin((double)Main.GlobalTimeWrappedHourly) + 1) * 0.7f) + 0.4f + glow), Main.GameUpdateCount * 0.001f, starTex2.Size() / 2, 0.32f + scaleAdd, SpriteEffects.None, 0f);
                spriteBatch.Draw(starTex2, cometP + starOffset, null, Color.DarkViolet * mainAlpha * mainAlpha * Intensity * 0.1f * ((((float)Math.Sin((double)Main.GlobalTimeWrappedHourly) + 1) * 0.7f) + 0.4f + glow), Main.GameUpdateCount * -0.001f, starTex2.Size() / 2, 0.32f + scaleAdd, SpriteEffects.None, 0f);
                DrawShine(cometP + starOffset);
            }
            spriteBatch.Reload(BlendState.AlphaBlend);
        }
        void DrawTrail()
        {
            float scaleAdd = mainAlpha * 0.025f;
            Texture2D tex = Helper.GetTex("MythosOfMoonlight/Assets/Textures/Extra/Ex3");
            float trailLength = 170 * mainAlpha * mainAlpha;
            Vector2 starOffset = new Vector2(35, -14f);
            float s = 1f;
            for (float i = 0; i < trailLength; i++)
            {
                float inverse_S = Ease(MathHelper.Lerp(mainAlpha, 0, s));
                Vector2 firstPos = Vector2.SmoothStep(new Vector2(Main.screenWidth - 75, yOffset), new Vector2(-30, yOffset), Ease((float)((Main.time) - i * 25) / ((float)Main.nightLength))) - new Vector2(0, 25 * inverse_S);
                Vector2 nextPos = Vector2.SmoothStep(new Vector2(Main.screenWidth - 75, yOffset), new Vector2(-30, yOffset), Ease((float)((Main.time) - (i + 1) * 25) / ((float)Main.nightLength))) - new Vector2(0, 25 * inverse_S);
                float globalTimeSmoothStepped = MathHelper.SmoothStep(0, 3600, Main.GlobalTimeWrappedHourly / 3600);
                float offsetFactor = MathF.Sin(globalTimeSmoothStepped * 2f + i * 0.05f);
                Vector2 offset = new Vector2(0, offsetFactor * 2.5f * s); //archived, for now

                offset = Vector2.Zero;
                for (float j = 0; j < 10; j++)
                {
                    Vector2 pos = Vector2.Lerp(firstPos, nextPos + offset, j / 10);
                    float alpha = ((0.5f + MathHelper.Lerp(0, glow * 0.1f, s)) * s) * Intensity;
                    Main.spriteBatch.Draw(tex, pos + starOffset, null, Color.Lerp(Color.Violet * mainAlpha, Color.DarkViolet, mainAlpha) * mainAlpha * mainAlpha * alpha, Helper.FromAToB(firstPos, nextPos).ToRotation(), new Vector2(0, tex.Height / 2), new Vector2(3f, mainAlpha * mainAlpha * 0.5f * s + scaleAdd), SpriteEffects.None, 0);

                    Main.spriteBatch.Draw(tex, pos + starOffset, null, Color.White * 0.65f * s * mainAlpha * mainAlpha * alpha, Helper.FromAToB(firstPos, nextPos).ToRotation(), new Vector2(0, tex.Height / 2), new Vector2(3f, mainAlpha * mainAlpha * 0.15f * s + scaleAdd), SpriteEffects.None, 0);
                    s -= 1 / (trailLength * 10);
                }
            }
        }
        void DrawShine(Vector2 pos)
        {
            float scaleAdd = mainAlpha * 0.1f;
            Texture2D tex = Helper.GetTex("MythosOfMoonlight/Assets/Textures/Extra/slash");
            Main.spriteBatch.Draw(tex, pos, null, Color.DarkViolet * (0.7f + glow * 0.1f) * Intensity, 0, tex.Size() / 2, new Vector2(0.4f - glow * 0.1f + scaleAdd, 0.2f + glow * 0.1f + scaleAdd) * mainAlpha * 2, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(tex, pos, null, Color.DarkViolet * (0.7f + glow * 0.1f) * Intensity, MathHelper.PiOver2, tex.Size() / 2, new Vector2(0.4f - glow * 0.1f + scaleAdd, 0.2f + glow * 0.1f + scaleAdd) * mainAlpha * 2, SpriteEffects.None, 0);


            Main.spriteBatch.Draw(tex, pos, null, Color.White * (0.7f + glow * 0.1f) * Intensity, 0, tex.Size() / 2, new Vector2(0.4f - glow * 0.1f + scaleAdd, 0.2f + glow * 0.1f + scaleAdd) * mainAlpha * 2, SpriteEffects.None, 0);

            Main.spriteBatch.Draw(tex, pos, null, Color.White * (0.7f + glow * 0.1f) * Intensity, MathHelper.PiOver2, tex.Size() / 2, new Vector2(0.4f - glow * 0.1f + scaleAdd, 0.2f + glow * 0.1f + scaleAdd) * mainAlpha * 2, SpriteEffects.None, 0);

            for (int k = 0; k < 3; k++)
            {
                float linesGlow = MathHelper.Clamp(k == 0 ? linesGlowInner : k == 1 ? linesGlowOuter : linesGlowInner2, 0, 1);
                float alpha = MathHelper.Lerp(0.75f, 0, MathHelper.Clamp(linesGlow * 1.5f, 0, 1)) * 2 * Intensity;
                int seed = k == 0 ? innerSeed : k == 1 ? outerSeed : innerSeed2;
                UnifiedRandom rand = new UnifiedRandom(seed);
                float max = 5;
                for (float i = 0; i < max; i++)
                {
                    float angle = Helper.CircleDividedEqually(i, max);
                    float scale = rand.NextFloat(0.5f, 0.8f) + (k == 1 ? 0.25f : 0);
                    Vector2 offset = new Vector2(Main.rand.NextFloat(30 * (k == 1 ? 1.2f : 1)) * linesGlow * scale, 0).RotatedBy(angle);
                    for (float j = 0; j < 2; j++)
                        Main.spriteBatch.Draw(tex, pos + offset, null, Color.DarkViolet * mainAlpha * mainAlpha * (k == 1 ? 0.24f : 0.4f) * alpha, angle, tex.Size() / 2, new Vector2(linesGlow + scaleAdd, alpha + scaleAdd) * scale * 2, SpriteEffects.None, 0);

                    Main.spriteBatch.Draw(tex, pos + offset * 0.5f, null, Color.White * mainAlpha * mainAlpha * alpha, angle, tex.Size() / 2, new Vector2(linesGlow + scaleAdd, alpha + scaleAdd) * scale, SpriteEffects.None, 0);


                    Main.spriteBatch.Draw(tex, pos + offset * 3, null, Color.White * mainAlpha * mainAlpha * 0.35f * (k == 1 ? 0.24f : 0.4f) * alpha, angle, tex.Size() / 2, new Vector2(linesGlow + scaleAdd, alpha + scaleAdd) * scale * 3 * 2, SpriteEffects.None, 0);
                }
            }
        }
        public override Color OnTileColor(Color inColor)
        {
            Vector4 value = inColor.ToVector4();
            return new Color(Vector4.Lerp(value, Vector4.One, Intensity * 0.2f));
        }
    }
}
