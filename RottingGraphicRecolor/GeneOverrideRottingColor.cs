using Verse;
using UnityEngine;

namespace RottingGraphicRecolor {
    public class GeneOverrideRottingColor : DefModExtension {
#if v15
        public Color color = new Color(0.29f, 0.25f, 0.22f);
#else
        public Color color = new Color(0.34f, 0.32f, 0.3f);
#endif

        public Color GetColor() {
            return color;
        }
    }
}
