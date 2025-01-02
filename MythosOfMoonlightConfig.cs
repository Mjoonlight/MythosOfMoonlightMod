using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace MythosOfMoonlight
{
    [BackgroundColor(2, 162, 162, 150)] 
    public class MythosOfMoonlightConfig : ModConfig
    {
        public static MythosOfMoonlightConfig Instance => GetInstance<MythosOfMoonlightConfig>();
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) => true;

        [Header("Graphics")]

        [BackgroundColor(0, 148, 225, 200)]
        [SliderColor(0, 148, 225, 100)]
        [Range(0, 5000)]
        [DefaultValue(1500)]
        [Description("[c/00e6e6:The maximum amount of particles that can exist at once. Higher values allows for more satisfying (or complete) visuals, but can also be performance hindering.]")]
        public int ParticleLimit { get; set; }

        [BackgroundColor(0, 148, 225, 200)]
        [DefaultValue(true)]
        [Description("[c/00e6e6:Whether or not certain visuals can utilize a bloom effect. It is not too harsh on performance, but can cause slight lag on lower end devices.]")]
        public bool CanBloomBeUsed { get; set; }
    }
}
