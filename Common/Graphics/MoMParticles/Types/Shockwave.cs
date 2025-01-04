using MythosOfMoonlight.Assets.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Common.Graphics.MoMParticles.Types
{
    public class Shockwave : MoMParticle
    {
        public float StartScale;

        public float EndScale;

        public Vector2 ScaleSquish;

        public static float SineInOut(float t) => (0f - (Cos((t * Pi)) - 1f)) / 2f;

        public override Texture2D Texture => TextureRegistry.Shockwave;

        public Shockwave(Vector2 position, Vector2 velocity, Color drawColor, float rot, float startScale, float endScale, Vector2 scaleSquish, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            ParticleDrawColor = drawColor;
            Scale = StartScale = startScale;
            EndScale = endScale;
            ScaleSquish = scaleSquish;
            MaxLifeTime = lifetime;
            Rotation = rot;

            BlendState = BlendState.Additive;
        }

        public override void Update()
        {
            Opacity = Lerp(1f, 0f, SineInOut(LifeRatio));
            Scale = Lerp(StartScale, EndScale, SineInOut(LifeRatio));
            Velocity *= 0.947f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position - Main.screenPosition, null, ParticleDrawColor * Opacity * 0.6f, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquish.X, ScaleSquish.Y) * 1.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(Texture, Position - Main.screenPosition, null, ParticleDrawColor * Opacity, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquish.X, ScaleSquish.Y), SpriteEffects.None, 0f);
        }

        public override void DrawBloom(SpriteBatch spriteBatch)
        {
            if (!DrawWithBloom)
                return;

            spriteBatch.Draw(Texture, Position - Main.screenPosition, null, BloomColor * Opacity, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquish.X, ScaleSquish.Y) * 1.05f, SpriteEffects.None, 0f);

        }
    }
}
