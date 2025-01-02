using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Assets.Textures
{
    public class TextureRegistry : ModSystem
    {
        public static Texture2D GlowSuperSmall;

        public override void Load() => LoadAll();

        public override void PostSetupContent() => LoadAll();

        private static void LoadAll()
        {
            const string basePath = "MythosOfMoonlight/Assets/Textures/";
            const string extrasPath = basePath + "Extra/";

            GlowSuperSmall = Request<Texture2D>(extrasPath + "GlowSuperSmall").Value;
        }
    }
}
