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
        /// Draws a trail behind the provided projectile.
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="data"></param>
        /// <param name="opacity"></param>
        /// <param name="saturation"></param>
        /// <param name="colorFunction"></param>
        /// <param name="widthFunction"></param>
        /// <param name="UseBacksides"></param>
        /// <param name="TryFixingMissalignment"></param>
        public static void DrawTrail(Projectile proj, MiscShaderData data, float opacity, float saturation, VertexStrip.StripColorFunction colorFunction, VertexStrip.StripHalfWidthFunction widthFunction, bool UseBacksides = true, bool TryFixingMissalignment = false)
        {
            MiscShaderData miscShaderData = data;

            miscShaderData.UseSaturation(saturation);
            miscShaderData.UseOpacity(opacity);
            miscShaderData.Apply();

            _vertexStrip.PrepareStripWithProceduralPadding(proj.oldPos, proj.oldRot, colorFunction, widthFunction, -Main.screenPosition + (proj.Size * 0.5f), UseBacksides, TryFixingMissalignment);
            _vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
    }
}
