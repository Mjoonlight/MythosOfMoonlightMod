using System;

namespace MythosOfMoonlight.Common.Datastructures
{
    public struct Rotation
    {
        private float _radian;

        public Rotation(float radian)
        {
            _radian = radian;
        }

        public Rotation(Vector2 vector)
        {
            _radian = MathF.Atan2(vector.Y, vector.X);
        }

        public float Angle
        {
            readonly get => _radian * 180 / Pi;
            set => _radian = Wrap(value / 180 * Pi);
        }

        public float Radian
        {
            readonly get => _radian;
            set => _radian = Wrap(value);
        }

        public float Coefficient
        {
            readonly get => _radian / Pi;
            set => _radian = Wrap(value * Pi);
        }

        public float Cos
        {
            readonly get => MathF.Cos(_radian);
            set => _radian = MathF.Acos(value);
        }

        public readonly Matrix RotationMatrix => Matrix.CreateRotationZ(_radian);

        public float Sin
        {
            readonly get => MathF.Sin(_radian);
            set => _radian = MathF.Asin(value);
        }

        public readonly Vector2 XAxis => new(Cos, Sin);

        public readonly float XFilpAngle => _radian switch
        {
            < 0 => -Pi - _radian,
            _ => Pi - _radian
        };

        public readonly Vector2 YAxis => new(-Sin, Cos);

        public readonly float YFilpAngle => -_radian;

        public static float Wrap(float radian)
        {
            while (radian >= Pi)
            {
                radian -= TwoPi;
            }

            while (radian < -Pi)
            {
                radian += TwoPi;
            }

            return radian;
        }

        public override readonly int GetHashCode() => _radian.GetHashCode();

        public override readonly string ToString() => _radian.ToString();
    }
}
