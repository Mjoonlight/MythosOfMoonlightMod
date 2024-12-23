using MythosOfMoonlight.Common.Datastructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Enums;
using Terraria.Graphics.CameraModifiers;

namespace MythosOfMoonlight.Common.Base
{
    //based off of MEAC's, but drastically improved and tweaked for general use!

    public abstract class SwordProjectile : ModProjectile
    {
        #region Fields and Properties

        public const string TrailTexturePath = "MythosOfMoonlight/Assets/Textures/Trails/";

        /// <summary>
        /// The current attack being performed.
        /// </summary>
        public int attackType = 0;

        /// <summary>
        /// The max number of swings / attacks that can be done. <see cref="attackType"/> is reset to 0 once it hits this value automatically.
        /// </summary>
        public int maxAttackType = 3;

        /// <summary>
        /// The pass to use for the trail shader. (0, 1 or 2)
        /// </summary>
        public int shaderType = 0;

        /// <summary>
        /// The main rotation vector for the swing. USe <see cref="Vector2Ellipse(float, float, float, float, float)"/> to generate a proper curve.
        /// </summary>
        public Vector2 mainVec;

        /// <summary>
        /// guh?
        /// </summary>
        public Queue<Vector2> trailVecs;

        /// <summary>
        /// The maximum length of the trail.
        /// </summary>
        public int trailLength = 30;

        /// <summary>
        /// An array with 13 slots (0- 12) that can be used for pretty much anything, such as storing rotation. 
        /// <br>Calling <see cref="NextAttackType"/> will reset each index to 0.</br>
        /// </summary>
        public float[] MiscArray = new float[13];

        /// <summary>
        /// Used for managing the current attack, and more!
        /// </summary>
        public int timer = 0;

        /// <summary>
        /// Whether or not the sword should deal any damage
        /// </summary>
        public bool isAttacking = false;

        /// <summary>
        /// Whether or not to render the trail
        /// </summary>
        public bool useTrail = true;

        /// <summary>
        /// If true, the sword will die if you release whatever mouse button spawned it!
        /// </summary>
        public bool AutoEnd = true;

        /// <summary>
        /// Whether or not you can hit stuff through tiles.
        /// </summary>
        public bool CanIgnoreTiles = false;

        public bool longHandle = false;

        public float drawScaleFactor = 1f;

        /// <summary>
        /// distance from the center to the player's arm
        /// </summary>
        public float disFromPlayer = 6;

        /// <summary>
        /// Self explanitory...
        /// </summary>
        public bool isRightClick = false;

        /// <summary>
        /// Useful for checking if you havent already hit something for onhit effects. use <see cref="ResetOnHit"/> to reset this to false.
        /// </summary>
        public bool HitNPC = false;

        public Player Player => Main.player[Projectile.owner];

        #endregion

        #region Helper methods

        public void DrawTrailSection(string color, string texture, float opacityMod = 1f, float start = 0.5f, float end = 1f)
        {
            List<Vector2> SmoothTrailX = GenerateCatmullRomPoints([..trailVecs]);
            var SmoothTrail = new List<Vector2>();

            for (int x = 0; x < SmoothTrailX.Count - 1; x++)
            {
                Vector2 vec = SmoothTrailX[x];

                SmoothTrail.Add(Vector2.Normalize(vec) * (vec.Length() + disFromPlayer));
            }

            if (trailVecs.Count != 0)
            {
                Vector2 vec = trailVecs.ToArray()[trailVecs.Count - 1];

                SmoothTrail.Add(Vector2.Normalize(vec) * (vec.Length() + disFromPlayer));
            }

            Vector2 center = Projectile.Center - Vector2.Normalize(mainVec) * disFromPlayer;

            int length = SmoothTrail.Count;

            if (length <= 3) 
                return;

            Vector2[] trail = [..SmoothTrail];
            var indicies = new List<VertexInfo>();

            for (int i = 0; i < length; i++)
            {
                float factor = i / (length - 1f);
                float w = TrailAlpha(factor) * opacityMod;

                Color c = Color.Blue;
                if (i is 0)
                {
                    c = Color.Transparent;
                }

                indicies.Add(new VertexInfo(center + trail[i] * start * Projectile.scale, new Vector3(factor, 1, 0f), c * opacityMod));
                indicies.Add(new VertexInfo(center + trail[i] * end * Projectile.scale,  new Vector3(factor, 0, w), c * opacityMod));
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, TrailBlendState(), SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);

            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0)) * Main.GameViewMatrix.TransformationMatrix;

            Effect MeleeTrail = Request<Effect>("MythosOfMoonlight/Assets/Effects/MeleeTrail", AssetRequestMode.ImmediateLoad).Value;
            MeleeTrail.Parameters["uTransform"].SetValue(model * projection);
            Main.graphics.GraphicsDevice.Textures[0] = Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad).Value;
            MeleeTrail.Parameters["tex1"].SetValue(Request<Texture2D>(color, AssetRequestMode.ImmediateLoad).Value);
            MeleeTrail.CurrentTechnique.Passes[shaderType].Apply();

            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, indicies.ToArray(), 0, indicies.Count - 2);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public void DrawTrailSection_Faded(string color, string texture, float opacityMod = 1f, float start = 0.5f, float end = 1f)
        {
            List<Vector2> SmoothTrailX = GenerateCatmullRomPoints([..trailVecs]);
            var SmoothTrail = new List<Vector2>();

            for (int x = 0; x < SmoothTrailX.Count - 1; x++)
            {
                Vector2 vec = SmoothTrailX[x];

                SmoothTrail.Add(Vector2.Normalize(vec) * (vec.Length() + disFromPlayer));
            }

            if (trailVecs.Count != 0)
            {
                Vector2 vec = trailVecs.ToArray()[trailVecs.Count - 1];

                SmoothTrail.Add(Vector2.Normalize(vec) * (vec.Length() + disFromPlayer));
            }

            Vector2 center = Projectile.Center - Vector2.Normalize(mainVec) * disFromPlayer;

            int length = SmoothTrail.Count;

            if (length <= 3) 
                return;

            Vector2[] trail = [.. SmoothTrail];
            var indices = new List<VertexInfo>();

            for (int i = 0; i < length; i++)
            {
                float factor = i / (length - 1f);
                float w = TrailAlpha(factor) * opacityMod;

                Color c = Color.Blue;
                if (i is 0)
                {
                    c = Color.Transparent;
                }

                indices.Add(new VertexInfo(center + trail[i] * start * Projectile.scale, new Vector3(factor, 1, 0f), c * opacityMod));
                indices.Add(new VertexInfo(center + trail[i] * end * Projectile.scale, new Vector3(factor, 0, w), c * opacityMod));
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);

            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
            var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0)) * Main.GameViewMatrix.TransformationMatrix;

            Effect MeleeTrailF = Request<Effect>("MythosOfMoonlight/Assets/Effects/FadedMeleeTrail", AssetRequestMode.ImmediateLoad).Value;
            MeleeTrailF.Parameters["uTransform"].SetValue(model * projection);
            Main.graphics.GraphicsDevice.Textures[0] = Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad).Value;
            MeleeTrailF.Parameters["tex1"].SetValue(Request<Texture2D>(color, AssetRequestMode.ImmediateLoad).Value);
            MeleeTrailF.CurrentTechnique.Passes[shaderType].Apply();

            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, indices.ToArray(), 0, indices.Count - 2);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }


        public Vector2 MainVec_WithoutGravDir
        {
            get
            {
                Vector2 v = mainVec;

                if (Player.gravDir == -1)
                    v.Y *= -1;

                return v;
            }
        }

        public Vector2 MouseWorld_WithoutGravDir
        {
            get
            {
                Vector2 vecRet = Main.MouseWorld;

                if (Player.gravDir == -1)
                    vecRet = WrapY(vecRet);

                return vecRet;
            }
        }

        public Vector2 ProjCenter_WithoutGravDir
        {
            get
            {
                Vector2 v = Projectile.Center;

                if (Player.gravDir == -1)
                    v = WrapY(v);

                return v;
            }
        }

        public static Vector2 GetPlayerVelocityToMouse(Player p, float speed, out float x, out float y)
        {
            Vector2 ret = NormalizeBetter(p.DirectionTo(Main.MouseWorld)) * speed;
            x = ret.X;
            y = ret.Y;

            return ret;
        }

        /// <summary>
        /// Handles "preparing" a swing (readies the mouse angle and arcs the sword back as if readying a strike)
        /// <br>MiscArray[1] is used for the anlgle to the mouse.</br>
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="angle"></param>
        /// <param name="timeMul"></param>
        /// <param name="lockDir"></param>
        /// <param name="getMouseAngle"></param>
        public void PrepareSwing(float radius, float angle, float timeMul, bool lockDir = true, bool getMouseAngle = true, float rot1 = -12f)
        {
            useTrail = false;

            if (lockDir)
                LockPlayerDir(Player);

            float targetRot = angle;

            mainVec = Vector2.Lerp(mainVec, Vector2Ellipse(radius, targetRot, rot1), 0.08f / timeMul);

            Projectile.rotation = mainVec.ToRotation();

            if (getMouseAngle)
                MiscArray[1] = GetAngleToMouse();
        }

#nullable enable

        /// <summary>
        /// Handles enacting a swing. 
        /// </summary>
        /// <param name="radius">Radius of the swing ellipse.</param>
        /// <param name="start">when to start the swing (though, you should be checking for this before even calling this method)</param>
        /// <param name="end">When to end the swing</param>
        /// <param name="angle">the angle of the swing. use <see cref="BaseSwordProj.MiscArray"/>'s second index [1] for aiming swings towards the mouse.</param>
        /// <param name="rotStrength">How fast the sword rotates. larger values can cause the sword to swing around you more than once. Set to a - value for an opposite dir.</param>
        /// <param name="viewZ">the z translation applied to the swing. (larger = more 3d, but too large = what the sigma)</param>
        /// <param name="timeMul">the time multipler applied to all timers. affects swing time and swing speed.</param>
        /// <param name="swingActions">an array (only [0] and [1] are used) of actions that are invoked when:
        /// <br>[0]: the swing starts.</br>
        /// <br>[1]: <paramref name="timeToPerfomMiscAction"/> is reached by <see cref="BaseSwordProj.timer"/>.</br></param>
        /// <param name="timeToPerfomMiscAction">when <paramref name="swingActions"/>'s 2nd index should be invoked.</param>
        public void EnactSwing(float radius, float start, float end, float angle, float rotStrength, float viewZ, float timeMul, Action[]? swingActions = null, float timeToPerfomMiscAction = -1f, float rot1 = -12f)
        {
            if (timer > start * timeMul)
            {
                if (MiscArray[2] == 0)
                {
                    swingActions?[0]?.Invoke();

                    MiscArray[2] = 1;
                }

                if (timeToPerfomMiscAction != -1)
                {
                    if (timer >= (timeToPerfomMiscAction - 1) * timeMul && timer <= (timeToPerfomMiscAction + 1) * timeMul)
                    {
                        if (MiscArray[3] == 0)
                        {
                            swingActions?[1]?.Invoke();

                            MiscArray[3] = 1;
                        }
                    }
                }

                isAttacking = true;

                if (timer < end * timeMul)
                {
                    Projectile.rotation += Projectile.spriteDirection * rotStrength / timeMul;
                    mainVec = Vector2Ellipse(radius, Projectile.rotation, rot1, angle, viewZ);
                }
            }
        }

        /// <summary>
        /// Handles performing a simple 2D swing. MiscArray[2] and 3 are used here. MiscArray[1] is globally used for the mouse angle. 
        /// </summary>
        /// <param name="swingSize">The length of the swing (the sword) during this swing.</param>
        /// <param name="swingStart">When to start the swing.</param>
        /// <param name="swingEnd">When to stop the swing.</param>
        /// <param name="swingSpeed">How fast to spin.</param>
        /// <param name="timeMultiplier">Modifies all time values for the entire swing.</param>
        /// <param name="swingStartAction">Called when the swing starts, usually play sounds/ modify damage all in one method and pass it here.</param>
        /// <param name="miscAction">An action that represents a parameterless method, and can be used for whatever.</param>
        /// <param name="miscActionActivationTime">When to invoke the other action.</param>
        /// <param name="angled">Whether or not the swing should be relative to the mouse.</param>
        public void EnactSwing_Simple(float swingSize, float swingStart, float swingEnd, float swingSpeed, float timeMultiplier = 1f, Action? swingStartAction = null, Action? miscAction = null, float miscActionActivationTime = -1f, bool angled = false)
        {
            //right at the start (kinda, 1 tick offset), invoke the first action. Usuallypass a method that plays a sound and resets the projectile's damage at the same time.

            if (timer.WithinRange(swingStart * timeMultiplier, swingStart * timeMultiplier, true, (int)Math.Min(Math.Max(1 * timeMultiplier, 1), 2)))
            {
                if (MiscArray[2] == 0f)
                {
                    swingStartAction?.Invoke();
                    MiscArray[2] = 1f;
                }
            }

            // same here, but check for the provided time, rather than do this at the start.

            if (miscActionActivationTime is not -1)
            {
                if (timer.WithinRange(miscActionActivationTime * timeMultiplier, miscActionActivationTime * timeMultiplier, true, (int)Math.Min(Math.Max(1 * timeMultiplier, 1), 2)))
                {
                    if (MiscArray[3] == 0f)
                    {
                        miscAction?.Invoke();
                        MiscArray[3] = 1f;
                    }
                }
            }

            //actually swing now

            if (timer.WithinRange(swingStart * timeMultiplier, swingEnd * timeMultiplier, false, (int)Math.Min(Math.Max(1 * timeMultiplier, 1), 2)))
            {
                isAttacking = true;
                Projectile.rotation += Projectile.spriteDirection * swingSpeed / timeMultiplier;
                mainVec = Vector2Ellipse(swingSize, Projectile.rotation, 0f);
            }
        }

#nullable disable

        /// <summary>
        /// Use in <see cref="SafeOnHitNPC(NPC, NPC.HitInfo, int)"/> to check if a hit killed an npc. Automatically checks for dummies.
        /// </summary>
        /// <param name="n"></param>
        /// <returns>True if the hit did, false otherwise.</returns>
        public bool HitKilledNPC(NPC n) => TargetIsNotADummy(n) && !n.active && n.life <= 0;

        /// <summary>
        /// Checks whether or not the given target is a target dummy. OnHit and such shouldnt apply to them, so make sure to check for this when doing so!
        /// </summary>
        /// <param name="n">The npc to check.</param>
        /// <returns>True if the npc is, false otherwise.</returns>
        public bool TargetIsNotADummy(NPC n) => n.type is not NPCID.TargetDummy;

        /// <summary>
        /// Call to enable onhit effects again, if you already hit something
        /// </summary>
        public void ResetOnHit()
        {
            HitNPC = false;
        }

        /// <summary>
        /// Call in <see cref="SafeModifyHitNPC(NPC, ref NPC.HitModifiers)"/> to ensure that this weapon doesnt do any less damage than <paramref name="dmg"/>. 
        /// <br>Useful for mods like calamity that add res from multiple hits from projectiles, effectively shitting on "melee" weapons like these's damage.</br>
        /// </summary>
        /// <param name="hMods"></param>
        /// <param name="dmg">The minimum amount of damage that can be done. If the damage that would be dealt is ever less than this, it is set to this.
        /// <br>It is advised that you pass <code>(int)(Projectile.damage / x)</code>to keep things within balance.</br></param>
        public static void SafeguardDamage(ref NPC.HitModifiers hMods, int dmg)
        {
            if (dmg < 1)
                dmg = 1;

            if (hMods.FinalDamage.Base < dmg)
                hMods.FinalDamage.Base = dmg;
        }

        /// <summary>
        /// Gets a percent modifier based on your current melee speed. 
        /// <br>NOTE: Melee speed can go over 100%, so dont feel foreced to use 100 as a limit!</br>
        /// </summary>
        /// <param name="player"></param>
        /// <param name="max">The maximum % that can be returned</param>
        /// <returns>The modifier, clamped at <paramref name="max"/>.</returns>
        public float GetMeleeSpeed(Player player, float max = 100)
        {
            return Math.Min((player.GetAttackSpeed(DamageClass.Melee) - 1) * 100, max);
        }

        /// <summary>
        /// Add a percent modifier (usually -1 to 1) to this projectile's damage. Add a negative number to reduce damage.
        /// </summary>
        /// <param name="percent">The percent of this projectiles damge to add on to itself <br>EX: Adding 10% (0.1f) to a projectile with 80 damage will result in the projectile having 88 damage.</br></param>
        /// <param name="max">The maximum percent that can be added. -1 is the minimum, as it would result in 0% damage.</param>
        public void ModifyProjectileDamage(float percent, float max = 1f)
        {
            percent = Clamp(percent, -1f, max);
            Projectile.damage = (int)(Projectile.damage * (1 + percent));
        }

        /// <summary>
        /// Call before starting a new attack or swing to reset the projectile's damage to what it was on spawn. Also calls <see cref="ResetOnHit"/>
        /// <br>If you decide to use <see cref="ModifyProjectileDamage(float, float)"/>, make sure to call this beforehand.</br>
        /// </summary>
        public void ResetProjectileDamage()
        {
            Projectile.damage = Projectile.originalDamage;
            ResetOnHit();
        }

        public void ApplyScreenshake(Vector2 pos, float power, float vibrations, float distFalloff, int time)
        {
            PunchCameraModifier modifier = new(pos, (Main.rand.NextFloat() * MathF.Tau).ToRotationVector2(), power, vibrations, time, distFalloff, FullName);
            Main.instance.CameraModifiers.Add(modifier);
        }

        public void DrawVertexByTwoLine(Texture2D texture, Color drawColor, Vector2 textureCoordStart, Vector2 textureCoordEnd, Vector2 positionStart, Vector2 positionEnd, float rot = PiOver2)
        {
            Vector2 coordVector = textureCoordEnd - textureCoordStart;

            coordVector.X *= texture.Width;
            coordVector.Y *= texture.Height;

            float theta = MathF.Atan2(coordVector.Y, coordVector.X);

            Vector2 drawVector = positionEnd - positionStart;

            Vector2 mainVectorI = drawVector.RotatedBy(theta * -Projectile.spriteDirection) * MathF.Cos(theta);
            Vector2 mainVectorJ = drawVector.RotatedBy((theta - rot) * -Projectile.spriteDirection) * MathF.Sin(theta);

            List<VertexInfo> vertex2Ds =
            [
                new VertexInfo(positionStart, new Vector3(textureCoordStart, 0), drawColor),
                new VertexInfo(positionStart + mainVectorI, new Vector3(textureCoordEnd.X, textureCoordStart.Y, 0), drawColor),

                new VertexInfo(positionStart + mainVectorJ, new Vector3(textureCoordStart.X, textureCoordEnd.Y, 0), drawColor),
                new VertexInfo(positionEnd, new Vector3(textureCoordEnd, 0), drawColor),
            ];

            Main.graphics.GraphicsDevice.Textures[0] = texture;
            Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertex2Ds.ToArray(), 0, vertex2Ds.Count - 2);
        }

        /// <summary>
        /// Moves <see cref="attackType"/> to the next stage, capping at <see cref="maxAttackType"/>, in which it will reset back to 0 once it reaches it.
        /// <br>Also resets <see cref="timer"/> and <see cref="MiscArray"/>'s indexes all to 0.</br>
        /// </summary>
        public void NextAttackType(bool shouldResetAttack = true)
        {
            if (!isAttacking && !AutoEnd)
            {
                if (!isRightClick)
                {
                    if (Player.dead)
                    {
                        End();
                        Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = false;
                    }
                }

                else
                {
                    if (!Player.controlUseTile || Player.dead)
                    {
                        End();
                        Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = false;
                    }
                }
            }

            timer = 0;

            for (int i = 0; i < 10; i++)
                MiscArray[i] = 0;

            if (++attackType > maxAttackType)
            {
                if (shouldResetAttack)
                    attackType = 0;
            }
        }

        public void ResetTo(int type)
        {
            timer = 0;

            for (int i = 0; i < 10; i++)
                MiscArray[i] = 0;

            attackType = type;
        }

        /// <summary>
        /// Call to ensure the player cannot look away from where the sword is facing.
        /// </summary>
        /// <param name="player"></param>
        public void LockPlayerDir(Player player)
        {
            Projectile.spriteDirection = Main.MouseWorld.X > player.Center.X ? 1 : -1;
            player.direction = Projectile.spriteDirection;
        }

        public static Vector2 Vector2Ellipse(float radius, float rot0, float rot1, float rot2 = 0, float viewZ = 1000)
        {
            Vector3 v = Vector3.Transform(Vector3.UnitX, Matrix.CreateRotationZ(rot0)) * radius;
            v = Vector3.Transform(v, Matrix.CreateRotationX(-rot1));

            if (rot2 != 0)
                v = Vector3.Transform(v, Matrix.CreateRotationZ(-rot2));

            return (-viewZ / (v.Z - viewZ)) * new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a rotation to a vector, super handy for spin attacks
        /// </summary>
        /// <param name="rot"></param>
        /// <returns></returns>
        public static Vector2 RotationalVelocity(float rot) => new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));

        /// <summary>
        /// Set one of either <see cref="Projectile.ai"/> or <see cref="MiscArray"/>'s indexes to this before a swing to swing towards the mouse.
        /// <br>Make sure to set it to <see cref="Vector2Ellipse(float, float, float, float, float)"/>'s rot2 for proper use!</br>
        /// </summary>
        /// <returns>The angle, in radians, to the mouse.</returns>
        public float GetAngleToMouse()
        {
            Vector2 vec = MouseWorld_WithoutGravDir - Main.player[Projectile.owner].Center;
            if (vec.X < 0)
                vec = -vec;
            return -vec.ToRotation();
        }

        public void AttackSound(SoundStyle sound) => SoundEngine.PlaySound(sound, Projectile.Center);

        private static Vector2 WrapY(Vector2 vec)
        {
            vec.Y -= Main.screenPosition.Y;
            float d = vec.Y - Main.screenHeight / 2;
            vec.Y -= 2 * d;
            vec.Y += Main.screenPosition.Y;
            return vec;
        }

        #endregion

        #region Overrides
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 15;

            Projectile.aiStyle = -1;
            Projectile.timeLeft = 30;

            Projectile.scale = 1f;

            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;

            Projectile.noEnchantmentVisuals = true;

            Projectile.DamageType = DamageClass.Melee;

            SafeSetDefaults();

            trailVecs = new Queue<Vector2>(trailLength + 1);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(mainVec);
            writer.Write(disFromPlayer);
            writer.Write(MiscArray.Length);
            writer.Write(Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            mainVec = reader.ReadVector2();
            disFromPlayer = reader.ReadSingle();

            for (int i = 0; i < 10; i++)
                MiscArray[i] = reader.ReadSingle();

            Projectile.spriteDirection = reader.ReadInt32();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (isAttacking && Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), ProjCenter_WithoutGravDir + MainVec_WithoutGravDir * Projectile.scale * (longHandle ? 0.235f : 0.2f), ProjCenter_WithoutGravDir + MainVec_WithoutGravDir * Projectile.scale))
                return true;

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail(lightColor);
            DrawSelf(Main.spriteBatch, lightColor);

            return false;
        }

        public sealed override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X > Main.player[Projectile.owner].Center.X ? 1 : -1;

            SafeguardDamage(ref modifiers, (int)(Projectile.damage * 0.2f));

            SafeModifyHitNPC(target, ref modifiers);
        }

        public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SafeOnHitNPC(target, hit, damageDone);
        }

        public override void AI()
        {
            if (Player.dead || !Player.active || Player.CCed || Player.noItems)
                End();

            Player.heldProj = Projectile.whoAmI;
            Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = true;

            Projectile.Center = Player.Center + Utils.SafeNormalize(mainVec, Vector2.One) * disFromPlayer;

            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, mainVec.ToRotation() - 1.57f);

            isAttacking = false;

            Projectile.ownerHitCheck = !CanIgnoreTiles;
            Projectile.timeLeft++;
           
            Attack();

            timer++;

            if (!isAttacking)
            {
                if (!isRightClick)
                {
                    bool IsEnd = AutoEnd ? !Player.controlUseItem || Player.dead : Player.dead;
                    if (IsEnd)
                        End();
                }

                else
                {
                    bool IsEnd = AutoEnd ? !Player.controlUseTile || Player.dead : Player.dead;
                    if (IsEnd)
                        End();
                }
            }

            if (isAttacking)
                Player.direction = Projectile.spriteDirection;

            if (useTrail)
            {
                trailVecs.Enqueue(mainVec);
                if (trailVecs.Count > trailLength)
                    trailVecs.Dequeue();
            }

            else
            {
                trailVecs.Clear();
            }

            Projectile.friendly = isAttacking;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            var cut = new Terraria.Utils.TileActionAttempt(DelegateMethods.CutTiles);
            Vector2 beamStartPos = Projectile.Center;
            Vector2 beamEndPos = beamStartPos + mainVec;
            Utils.PlotTileLine(beamStartPos, beamEndPos, Projectile.width * Projectile.scale, cut);
        }

        #endregion

        #region Virtuals

        /// <summary>
        /// Set things like <see cref="trailLength"/>, <see cref="disFromPlayer"/>, <see cref="shaderType"/>, and <see cref="Projectile.scale"/> here.
        /// </summary>
        public virtual void SafeSetDefaults() { }

        /// <summary>
        /// All attack logic should be done here. <see cref="timer"/> is used to increment "phases", and <see cref="maxAttackType"/> is used to control what you can do.
        /// <br>Call <see cref="NextAttackType"/>to reset everything and move on to the next attack. This will return you back to 0 if you hit <see cref="maxAttackType"/>.</br>
        /// </summary>
        public virtual void Attack() { }

        /// <summary>
        /// Call when you want to end an attack, and kill the projectile.
        /// </summary>
        public virtual void End()
        {
            Projectile.Kill();
            Player.GetModPlayer<SwordPlayer>().isUsingMeleeProj = false;
        }

        public virtual void DrawSelf(SpriteBatch spriteBatch, Color lightColor, Vector4 diagonal = new Vector4(), Vector2 drawScale = new Vector2(), Texture2D glowTexture = null)
        {
            if (diagonal == new Vector4())
            {
                diagonal = new Vector4(0, 1, 1, 0);
            }

            if (drawScale == new Vector2())
            {
                drawScale = new Vector2(0, 1);

                if (longHandle)
                {
                    drawScale = new Vector2(-0.6f, 1);
                }

                drawScale *= drawScaleFactor;
            }

            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = Request<Texture2D>(GlowTexture).Value;
            Vector2 drawCenter = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawVertexByTwoLine(tex, lightColor, diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);

            if (UseGlowmask)
                DrawVertexByTwoLine(glowTex, Color.White, diagonal.XY(), diagonal.ZW(), drawCenter + mainVec * drawScale.X, drawCenter + mainVec * drawScale.Y);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// The alpha for the trail. <paramref name="factor"/> is the completion from start to end
        /// <br>By default, lerps from 0-1 based on <paramref name="factor"/>.</br>
        /// </summary>
        /// <param name="factor">The trail completion.</param>
        /// <returns>The alpha for the trail.</returns>
        public virtual float TrailAlpha(float factor)
        {
            return Lerp(0f, 1, factor);
        }

        /// <summary>
        /// The texture for the trail. Stick to 128x128 or 256x256 for texture size if you decide to make your own. 
        /// </summary>
        /// <returns>The path to the texture.</returns>
        public virtual string TrailShapeTex() => TrailTexturePath + "TrailShape_White";

        public virtual string TrailShapeTex2() => TrailTexturePath + "TrailShape_White";

        /// <summary>
        /// The texture for the trail's gradient. Note that the length must match the length of the shape texture.
        /// </summary>
        /// <returns></returns>
        public virtual string TrailColorTex() => TrailTexturePath + "Colors/White";

        /// <summary>
        /// The 2nd texture for the trail's gradient. Note that the length must match the length of the shape texture.
        /// </summary>
        /// <returns></returns>
        public virtual string TrailColorTex2() => TrailTexturePath + "Colors/White";

        /// <summary>
        /// The blend state for the trail. 
        /// </summary>
        /// <returns></returns>
        public virtual BlendState TrailBlendState() => BlendState.NonPremultiplied;

        public virtual void DrawTrail(Color color) { }

        /// <summary>
        /// Whether or not to draw a glowmask over the base texture. Override <see cref="GlowTexture"/> if you override this!
        /// </summary>
        public virtual bool UseGlowmask { get; } = false;

        /// <summary>
        /// The path to the glowmask texture.
        /// </summary>
        public virtual new string GlowTexture => Texture;

        /// <summary>
        /// Called in <see cref="ModifyHitNPC(NPC, ref NPC.HitModifiers)"/>, and functions the same
        /// </summary>
        /// <param name="n"></param>
        /// <param name="hitMods"></param>
        public virtual void SafeModifyHitNPC(NPC n, ref NPC.HitModifiers hitMods) { }

        /// <summary>
        /// Called in <see cref="OnHitNPC(NPC, NPC.HitInfo, int)"/>, and functions the same
        /// </summary>
        /// <param name="n"></param>
        /// <param name="info"></param>
        /// <param name="dmgDone"></param>
        public virtual void SafeOnHitNPC(NPC n, NPC.HitInfo info, int dmgDone) { }

        #endregion
    }

    public class SwordPlayer : ModPlayer
    {
        public bool isUsingMeleeProj = false;

        public bool SwordMovement = false;

        public bool CanClickTimerDecrement;

        public int CurrentClickBasedAttack = 0;
        public int ClickInterval = 0;

        public int MiscWindowForEffects = 0;

        public override void PreUpdate()
        {
            if (isUsingMeleeProj)
                Player.itemAnimation = 2;

            if (SwordMovement)
                Player.maxFallSpeed = 10000;

            if (CanClickTimerDecrement)
                ClickInterval--;

            if (ClickInterval <= 0 && CurrentClickBasedAttack != 0)
                CurrentClickBasedAttack = 0;

            if (MiscWindowForEffects > 0)
                MiscWindowForEffects--;
        }
    }
}
