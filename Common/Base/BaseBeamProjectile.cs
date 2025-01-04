using System;
using System.Linq;
using Terraria.Enums;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace MythosOfMoonlight.Common.Base
{
    public abstract class BeamProjectile : ModProjectile
    {
        /// <summary>
        /// The amount of time in ticks that the laser has existed, up to <see cref="MaximumTime"/>. 
        /// <br>Useful for time-based effects. <see cref="LifeRatio"/> also exists for this purpose.</br>
        /// </summary>
        public float Time = 0f;

        /// <summary>
        /// The current length of the laser. Use this for effects like dust, or length related stuff. An example of how to iterate length is below this.
        /// </summary>
        public float CurrentLength = 0f;

        #region example

        // for (float i = initialValue; i < CurrentLength; i += someValue) //someValue could be a fraction of the length. InitialValue could be larger for an offsetted starting point. (particles that are too large, etc)
        // {
        //     Vector2 currentPosition = Projectile.Center + Projectile.velocity * i;
        // }

        #endregion

        /// <summary>
        /// Whether or not the laser can expand to its desired length. Useful for making the laser stop at hit npcs.
        /// </summary>
        public bool CanLaserGrow = true;

        /// <summary>
        /// If true, then the laser will stop expanding past the npc it hit.
        /// </summary>
        public virtual bool CollidesWithNPCs { get; } = false;

        /// <summary>
        /// The maximum length the laser can reach. Values beyond 4000f can cause lag on lower end pcs, so be cautious.
        /// </summary>
        public abstract float MaximumLength { get; }

        /// <summary>
        /// How many ticks the laser may exist for.
        /// </summary>
        public abstract float MaximumTime { get; }

        /// <summary>
        /// The maximum scale of the laser. 
        /// </summary>
        public abstract float MaximumScale { get; }

        /// <summary>
        /// How fast the laser scales in (to <see cref="MaximumScale"/>) and out (to 0f).
        /// </summary>
        public virtual float ScaleInOutRate { get; } = 1f;

        /// <summary>
        /// How fast the laser approaches the maximum length. Larger values (0.7f to 0.85f) are expected for general use. 
        /// <br>Setting this to values >= 1f will cause the laser to instantly reach the maximum length.</br>
        /// </summary>
        public virtual float ElongationFactor { get; } = 0.7f;

        /// <summary>
        /// How many pixels that the projectile's center can exist offscreen while still drawing the laser. Defaults to 3 times the maximum length.
        /// </summary>
        public virtual int ScreenFluff { get => (int)(MaximumLength * 3f); }

        /// <summary>
        /// How many calcualtions to perform if the projectile should collide with tiles, to determine the new length such that the ray doesnt actually go past said tiles.
        /// <br>Higher values lead to higher accuracy, though it becomes more performance impacting too.</br>
        /// </summary>
        public virtual int TileCollisionDetectionCount { get; } = 10;

        /// <summary>
        /// A values that ranges from 0 (just spawned) to 1 (just died).
        /// </summary>
        public float LifetimeRatio => Time / (float)MaximumTime;

        /// <summary>
        /// The owner of the projectile.
        /// </summary>
        public Player Owner { get => Main.player[Projectile.owner]; }

        /// <summary>
        /// A ratio of the laser's current length over the maximum length. 1f = fully grown, 0f = just spawned
        /// </summary>
        public float LengthRatio => CurrentLength / (float)MaximumLength;

        public virtual void SafeSetDefaults() { }

        /// <summary>
        /// Called in <see cref="AI"/> every tick, handle miscellaneous effects here.
        /// <br>This exists so you do not need to override <see cref="AI"/> every time you make a unique laser.</br>
        /// </summary>
        public virtual void MiscAI() { }

        /// <summary>
        /// The length that the laser should eventually reach.
        /// </summary>
        /// <returns><see cref="MaximumLength"/> by default, which is the ideal length the laser should eventually reach by default.</returns>
        public virtual float TargetLength(int pointCount, bool withTiles = false)
        {
            if (!withTiles)
                return MaximumLength;

            else
            {
                float[] points = new float[pointCount];
                Collision.LaserScan(Projectile.Center, Projectile.velocity, Projectile.scale, MaximumLength, points);
                return points.Average();
            }
        }

        public virtual void TargetScale()
        {
            Projectile.scale = (float)Sin(Time / MaximumTime * Pi) * ScaleInOutRate * MaximumScale;

            if (Projectile.scale > MaximumScale)
                Projectile.scale = MaximumScale;
        }

        public sealed override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.timeLeft = (int)MaximumTime;

            SafeSetDefaults();
        }

        public override void AI()
        {
            if (++Time >= MaximumTime)
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = NormalizeBetter(Projectile.velocity);

            ProjectileID.Sets.DrawScreenCheckFluff[Type] = ScreenFluff;

            float dir = Projectile.velocity.ToRotation();

            Projectile.rotation = dir - PiOver2;
            Projectile.velocity = dir.ToRotationVector2();

            TargetScale();

            if (CanLaserGrow)
                CurrentLength = SmoothStep(CurrentLength, TargetLength(TileCollisionDetectionCount, Projectile.tileCollide), ElongationFactor);
        
            MiscAI(); //called after everything so you may manipulate these values if you must, but if you are doing that anyways you might as well just override AI again...
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float garbage = 0f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * CurrentLength, Projectile.Size.Length() * Projectile.scale, ref garbage);
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackMelee;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * CurrentLength, Projectile.Size.Length() * Projectile.scale, DelegateMethods.CutTiles);
        }

        public override bool ShouldUpdatePosition() => false; //needed so the center stays where it is fired from. The center is where the beam starts, and should not move.
    }
}
