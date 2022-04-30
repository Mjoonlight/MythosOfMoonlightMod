using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MythosOfMoonlight.NPCs
{
    public class FighterGlobalAI : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		const float StrideLimit = 70.78f;
		public void FighterAI(NPC NPC, float jumpHeight, float strideSpeed, bool canJump)
		{
			// var sqrDistance = player.DistanceSQ(NPC.position);
			if (NPC.velocity.Y < 0)
			{
				NPC.spriteDirection = NPC.direction;
				NPC.velocity.X = NPC.direction;
			}
			if (!NPC.collideY)
				return;

			var player = Main.player[NPC.target];
			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY, 1, false, 0);
			if (NPC.collideX && canJump)
			{
				NPC.velocity -= new Vector2(0, jumpHeight);
			}
			NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, strideSpeed * NPC.direction, 0.35f);
			var horizontalDistance = Math.Abs(NPC.Center.X - player.Center.X);
			if (horizontalDistance >= StrideLimit)
			{
				NPC.FaceTarget();
			}
			NPC.spriteDirection = NPC.direction;
		}
	}
}