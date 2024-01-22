using Verse;
using UnityEngine;

namespace RottingGraphicRecolor {
    public class GeneOverrideRottingColor : DefModExtension {
        public Color color = new Color(0.34f, 0.32f, 0.3f);

        public Color GetColor() {
            return color;
        }
    }
}
