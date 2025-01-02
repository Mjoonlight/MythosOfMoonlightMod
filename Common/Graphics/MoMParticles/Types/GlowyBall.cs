using MythosOfMoonlight.Assets.Textures;
using MythosOfMoonlight.Common.Datastructures;
using MythosOfMoonlight.Common.Systems;

namespace MythosOfMoonlight.Common.Graphics.MoMParticles.Types
{
    internal class GlowyBall : MoMParticle
    {
        private Color EdgeColor;

        private readonly float HueShift;

        private readonly float EndScale;

        private readonly float EndOpacity;

        private readonly float VelocityModifier;

        public override Texture2D Texture => TextureRegistry.GlowSuperSmall;

        public GlowyBall(Vector2 spawn, Vector2 velocity, Color drawColor, Color edgeColor, int time, float opacity, float endOpacity, float scale, float endScale, Vector2 scaleMod, float velocityModifier = 0.89f, float hueShift = 0f)
        {
            Position = spawn;

            Velocity = velocity;
            VelocityModifier = velocityModifier;

            ParticleDrawColor = drawColor;
            EdgeColor = edgeColor;

            Opacity = opacity;
            EndOpacity = endOpacity;

            Scale = scale;
            EndScale = endScale;
            ScaleSquishFactor = scaleMod;

            MaxLifeTime = time;

            HueShift = hueShift;

            BlendState = BlendState.Additive; //required to be set, so default it to additive if you dont use the ctor to specify otherwise.
        }

        public override void Update()
        {
            float lerp = 1f / (float)MaxLifeTime;

            Opacity = Lerp(Opacity, EndOpacity, lerp);
            Scale = Lerp(Scale, EndScale, lerp);
            Velocity *= VelocityModifier;
            Rotation = Velocity.ToRotation();

            ParticleDrawColor = Main.hslToRgb((Main.rgbToHsl(ParticleDrawColor).X + HueShift) % 1, Main.rgbToHsl(ParticleDrawColor).Y, Main.rgbToHsl(ParticleDrawColor).Z);
            EdgeColor = Main.hslToRgb((Main.rgbToHsl(EdgeColor).X + HueShift) % 1, Main.rgbToHsl(EdgeColor).Y, Main.rgbToHsl(EdgeColor).Z);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position - Main.screenPosition, null, EdgeColor * Opacity * 0.5f, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquishFactor.X, ScaleSquishFactor.Y) * 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color.Lerp(ParticleDrawColor, EdgeColor, 0.5f) * Opacity * 0.85f, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquishFactor.X, ScaleSquishFactor.Y) * 0.56f, SpriteEffects.None, 0f);
            spriteBatch.Draw(Texture, Position - Main.screenPosition, null, ParticleDrawColor * Opacity, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquishFactor.X, ScaleSquishFactor.Y) * 0.3f, SpriteEffects.None, 0f);
        }

        public override void DrawBloom(SpriteBatch spriteBatch)
        {
            if (!DrawWithBloom)
                return;

            VFXManager.DrawCache.Add(() =>
            {
                //glowy-er outer bit that uses the bloom color
                spriteBatch.Draw(Texture, Position - Main.screenPosition, null, BloomColor * Opacity * 0.35f, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquishFactor.X, ScaleSquishFactor.Y) * 0.65f, SpriteEffects.None, 0f);

                //make the inner bit bold
                spriteBatch.Draw(Texture, Position - Main.screenPosition, null, ParticleDrawColor * Opacity * 0.95f, Rotation, Texture.Size() / 2f, Scale * new Vector2(ScaleSquishFactor.X, ScaleSquishFactor.Y) * 0.31f, SpriteEffects.None, 0f);
            });
        }
    }
}
