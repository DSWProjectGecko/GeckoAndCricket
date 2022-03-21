using System.Collections.Generic;
using UnityEngine;

namespace BaseScripts.SurfaceTypes
{
    public class WallTypes : BaseSurfaceTypes
    {
        public WallTypes(IEnumerable<LayerMask> surfaceLayers)
        {
            Lava = null;
            Honey = null;
            Ice = null;
            Normal = null;

            foreach (LayerMask layer in surfaceLayers)
            {
                int index = Mathc.GetExponent(layer);
                switch (LayerMask.LayerToName(index))
                {
                    case "LavaWall": 
                        Lava = layer; 
                        break;
                    case "HoneyWall": 
                        Honey = layer; 
                        break;
                    case "IceWall": 
                        Ice = layer; 
                        break;
                    case "NormalWall":
                        Normal = layer;
                        break;
                }
            }
        }
    }
}
