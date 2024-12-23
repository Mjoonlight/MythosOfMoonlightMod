using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Common.Datastructures
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="holdOffset"></param>
    /// <param name="rotationOffset"></param>
    /// <param name="owner"></param>
    /// <param name="aimOffset"></param>
    public struct HeldprojSettings(float holdOffset, float rotationOffset, Player owner, float aimOffset)
    {
        public float HoldOffset = holdOffset; 
        public float RotationOffset = rotationOffset;
        public float AimOffset = aimOffset;
        public Player Owner = owner;
    }
}
