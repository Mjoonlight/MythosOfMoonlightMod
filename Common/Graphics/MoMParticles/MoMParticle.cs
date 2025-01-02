using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Common.Graphics.MoMParticles
{
    public abstract class MoMParticle
    {
        public int MaxLifeTime;

        public float Scale;

        public float Rotation;

        public float Opacity;

        public Vector2 Position;

        public Vector2 Velocity;

        public Vector2 ScaleSquishFactor;

        public bool DrawWithBloom;

        public BlendState BlendState;

        public Color ParticleDrawColor;

        public Color BloomColor;

        public int LifeTimeTimer;

        public abstract Texture2D Texture { get; }

        public virtual bool Important { get; } = false;

        public int TimeLeft => MaxLifeTime - LifeTimeTimer;

        /// <summary>
        /// A ratio of time since spawn / max time. 1f = dead, 0f = just spawned.
        /// </summary>
        public float LifeRatio
        {
            get
            {
                return LifeTimeTimer / (float)MaxLifeTime;
            }
        }

        public abstract void Update();

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = Texture.Size() * 0.5f;
            Vector2 screenPos = Position - Main.screenPosition;

            spriteBatch.Draw(Texture, screenPos, null, ParticleDrawColor * Opacity, Rotation, origin, Scale * ScaleSquishFactor, SpriteEffects.None, 0f);
        }

        public virtual void DrawBloom(SpriteBatch spriteBatch) { }
    }

    public class ParticleHandler : ModSystem
    {
        public static List<MoMParticle> ActiveParticles = [];

        public static List<MoMParticle> ParticlesToRemove = [];

        public static int MaxParticleCount => MythosOfMoonlightConfig.Instance.ParticleLimit;

        #region utils

        public static void SpawnParticle(MoMParticle particleToSpawn)
        {
            if (Main.netMode == NetmodeID.Server)
                return; //don't spawn particles serverside, that results in additional overhead and they get spawned for every player on the server.

            if (particleToSpawn.GetType().IsAbstract)
                return; //abstract particles should not be spawned, they are likely a blueprint for other particles

            if (ActiveParticles.Count > MaxParticleCount)
            {
                MoMParticle particleToRemove = ActiveParticles.FirstOrDefault(particle => !particle.Important);

                if (particleToRemove != null)
                    ActiveParticles.Remove(particleToRemove);
            }

            ActiveParticles.Add(particleToSpawn);
        }

        public static void KillParticle(MoMParticle particleToRemove)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (particleToRemove.GetType().IsAbstract)
                return;

            ParticlesToRemove.Add(particleToRemove);
        }

        public static void DrawParticles(SpriteBatch sb)
        {
            if (ActiveParticles.Count == 0)
                return;

            List<MoMParticle> additiveParticles = [];
            List<MoMParticle> nonPreMultipliedParticles = [];
            List<MoMParticle> alphablendParticles = [];

            foreach (var p in ActiveParticles)
            {
                if (p.BlendState == BlendState.Additive)
                    additiveParticles.Add(p);

                if (p.BlendState == BlendState.AlphaBlend)
                    alphablendParticles.Add(p);

                if (p.BlendState == BlendState.NonPremultiplied)
                    nonPreMultipliedParticles.Add(p);
            }

            // what

            if (additiveParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (var particle in additiveParticles)
                {
                    particle.Draw(sb);
                }

                sb.End();
            }

            // the

            if (alphablendParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (var particle in alphablendParticles)
                {
                    particle.Draw(sb);
                }

                sb.End();
            }

            // gyatt?

            if (nonPreMultipliedParticles.Count > 0)
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (var particle in nonPreMultipliedParticles)
                {
                    particle.Draw(sb);
                }

                sb.End();
            }
        }

        internal static void UpdateActiveParticles()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (ActiveParticles.Count == 0)
                return;

            foreach (var particle in ActiveParticles)
            {
                particle.Update();
                particle.Position += particle.Velocity;
                particle.LifeTimeTimer++;
            }

            ActiveParticles.RemoveAll(p => p.LifeTimeTimer > p.MaxLifeTime || ParticlesToRemove.Contains(p));

            ParticlesToRemove.Clear();
        }

        #endregion

        public override void PreUpdateDusts() => UpdateActiveParticles();

        public override void Load()
        {
            On_Main.DrawDust += DrawAllParticles_Dust;
            ActiveParticles = [];
        }

        public override void Unload()
        {
            On_Main.DrawDust -= DrawAllParticles_Dust;
            ActiveParticles.Clear();
            ActiveParticles = null; 
        }


        private void DrawAllParticles_Dust(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            DrawParticles(Main.spriteBatch); 
        }

        public override void ClearWorld() => ActiveParticles?.Clear();
    }
}
