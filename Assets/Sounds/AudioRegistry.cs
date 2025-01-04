using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MythosOfMoonlight.Assets.Sounds
{
    public class AudioRegistry : ModSystem
    {
        const string audioFolderPath = "MythosOfMoonlight/Assets/Sounds/";

        public static readonly SoundStyle SFX_BowString;

        public static readonly SoundStyle SFX_BowFire;

        static AudioRegistry()
        {
            SFX_BowString = new SoundStyle(audioFolderPath + "bowPull");
            SFX_BowFire = new SoundStyle(audioFolderPath + "bowRelease");
        }
    }
}
