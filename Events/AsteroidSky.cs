using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.Main;

namespace MythosOfMoonlight.Events
{
    public class AsteroidSky : CustomSky
    {
        public bool isActive;
        public float Intensity;
        public Star[] stars;
        public struct Star
        {
            public string texture;
            public Vector2 pos;
            public float depth;
        }
        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            stars = new Star[100];
            for (int i = 0; i < stars.Length; i++)
            {
                int variant = rand.Next(2);
                if (rand.NextBool(50))
                    variant = 2;
                stars[i].texture = "MythosOfMoonlight/Assets/Textures/star" + variant;
                stars[i].pos = new Vector2(rand.NextFloat(screenWidth), rand.NextFloat(screenHeight * 0.25f));
                if (variant != 2)
                    stars[i].depth = rand.NextFloat(0.1f, 0.5f);
                else
                    stars[i].depth = rand.NextFloat(0.5f, 0.7f);
            }
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

        }
        public override bool IsActive()
        {
            return Intensity > 0;
        }
        //float glow;
        //float intensity;
        public override float GetCloudAlpha()
        {
            return MathHelper.Lerp(1, 0.1f, Intensity);
        }
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 3.40282347E+38f && minDepth < 3.40282347E+38f)
            {
                Texture2D Tex = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/gradation").Value;
                Texture2D Tex2 = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/gradation2").Value;
                Texture2D Tex3 = ModContent.Request<Texture2D>("MythosOfMoonlight/Assets/Textures/Extra/trail").Value;
                Vector2 Pos = new(screenWidth / 2, screenHeight / 2);

                spriteBatch.Reload(BlendState.Additive);
                for (int i = 0; i < stars.Length; i++)
                {
                    stars[i].pos.X += stars[i].depth * 3;
                    if (stars[i].pos.X > screenWidth + 100)
                    {
                        int variant = rand.Next(2);
                        if (rand.NextBool(50))
                            variant = 2;
                        stars[i].texture = "MythosOfMoonlight/Assets/Textures/star" + variant;
                        if (variant != 2)
                            stars[i].depth = rand.NextFloat(0.1f, 0.5f);
                        else
                            stars[i].depth = rand.NextFloat(0.5f, 0.7f);
                        stars[i].pos.X = -100;
                        stars[i].pos.Y = rand.NextFloat(screenHeight * 0.25f);
                    }
                    /*if (stars[i].texture == "MythosOfMoonlight/Assets/Textures/star2")
                    {
                        spriteBatch.Draw(Tex3, stars[i].pos, null, Color.White * Intensity * 0.5f * stars[i].depth, 0, new Vector2(Tex3.Width, Tex3.Height / 2), stars[i].depth * 0.1f, SpriteEffects.None, 0);
                    }
                    */
                    spriteBatch.Draw(ModContent.Request<Texture2D>(stars[i].texture).Value, stars[i].pos, null, Color.White * Intensity * 2 * stars[i].depth, GameUpdateCount * 0.01f * stars[i].depth, ModContent.Request<Texture2D>(stars[i].texture).Value.Size() / 2, stars[i].depth, SpriteEffects.None, 0);
                    //Dust.NewDustPerfect(stars[i].pos + Main.screenPosition, 1);
                }

                spriteBatch.Draw(Tex, new Rectangle(0, 0 - (int)screenPosition.Y, screenWidth, 3500), null, Color.DodgerBlue * Intensity * 0.65f, 0, Vector2.Zero, SpriteEffects.None, 0);
                for (int i = 0; i < 2; i++)
                    spriteBatch.Draw(Tex2, new Rectangle(0, -50, screenWidth, screenHeight), null, Color.DodgerBlue * Intensity * 0.7f, 0, Vector2.Zero, SpriteEffects.None, 0);


            }
            spriteBatch.Reload(BlendState.AlphaBlend);
        }

        public override Color OnTileColor(Color inColor)
        {
            Vector4 value = inColor.ToVector4();
            return new Color(Vector4.Lerp(value, Vector4.One, Intensity * 0.2f));
        }
    }
}
