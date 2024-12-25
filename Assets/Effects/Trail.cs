using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;

namespace MythosOfMoonlight.Assets.Effects
{
    public class Trail
    {
        private static VertexStrip _vertexStrip = new VertexStrip();

        /// <summary>
        /// Draws a trail behind the provided projctile.
        /// </summary>
        /// <param name="proj">The proj to draw to.</param>
        /// <param name="col">The color to draw with.</param>
        /// <param name="width">The width of the trail.</param>
        public static void DrawTrail(Projectile proj, float opacity, float saturation, VertexStrip.StripColorFunction colorFunction, VertexStrip.StripHalfWidthFunction widthFunction, bool UseBacksides = true, bool TryFixingMissalignment = false)
        {
            MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];

            miscShaderData.UseSaturation(saturation);
            miscShaderData.UseOpacity(5f * opacity);
            miscShaderData.Apply();

            _vertexStrip.PrepareStripWithProceduralPadding(proj.oldPos, proj.oldRot, colorFunction, widthFunction, -Main.screenPosition + (proj.Size * 0.5f), UseBacksides, TryFixingMissalignment);
            _vertexStrip.DrawTrail();

            //Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
    }
}
