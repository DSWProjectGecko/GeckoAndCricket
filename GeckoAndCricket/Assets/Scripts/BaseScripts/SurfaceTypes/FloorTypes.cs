using System.Collections.Generic;
using UnityEngine;

namespace BaseScripts.SurfaceTypes
{
    public class FloorTypes : BaseSurfaceTypes
    {
        public FloorTypes(IEnumerable<LayerMask> surfaceLayers)
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
                    case "LavaFloor": 
                        Lava = layer; 
                        break;
                    case "HoneyFloor": 
                        Honey = layer; 
                        break;
                    case "IceFloor": 
                        Ice = layer; 
                        break;
                    case "NormalFloor":
                        Normal = layer;
                        break;
                }
            }
        }
    
    }
}
