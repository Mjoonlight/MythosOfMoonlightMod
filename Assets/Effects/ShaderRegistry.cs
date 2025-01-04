using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;

namespace MythosOfMoonlight.Assets.Effects
{
    public class ShaderRegistry : ModSystem
    {
        public const string DirectoryPath = "Assets/Effects/";

        internal static Effect BasicTrailShader;

        private static void RegisterShader(Effect effect, string shaderPassName, string shaderRetrivalName)
        {
            Ref<Effect> shader = new Ref<Effect>(effect);

            MiscShaderData newData = new(shader, shaderPassName);
            GameShaders.Misc[$"{shaderRetrivalName}"] = newData;
        }

        private static Effect LoadShader(string xnbName)
        {
            return MythosOfMoonlight.Instance.Assets.Request<Effect>($"{DirectoryPath}{xnbName}").Value;
        }

        private static void LoadAllShaders()
        {
            if (Main.dedServ)
                return;

            BasicTrailShader = LoadShader("BasicTrailShader");
            RegisterShader(BasicTrailShader, "TrailPass", "BasicTrail");
        }

        public override void Load()
        {
            LoadAllShaders();
        }

        public override void PostSetupContent()
        {
            LoadAllShaders();
        }
    }
}
