using System.Collections.Generic;
using UnityEngine;

namespace BaseScripts
{
    public readonly struct SurfaceTypes
    {
        public readonly int? Lava;
        public readonly int? Honey;
        public readonly int? Ice;

        public SurfaceTypes(IEnumerable<LayerMask> surfaceLayers)
        {
            Lava = null;
            Honey = null;
            Ice = null;
            
            foreach (LayerMask layer in surfaceLayers)
            {
                int index = Mathc.GetExponent(layer);
                switch (LayerMask.LayerToName(index))
                {
                    case "Lava": 
                        Lava = layer; 
                        break;
                    case "Honey": 
                        Honey = layer; 
                        break;
                    case "Ice": 
                        Ice = layer; 
                        break;
                }
            }
        }

        public IEnumerable<int?> GetSurfaceTypes()
        {
            return new [] {Lava, Honey, Ice};
        }
    }

    public class BaseWorld : MonoBehaviour
    {
        [Header("World layers:")]
        public LayerMask groundLayer;
        public LayerMask wallLayer;
        public LayerMask ceilingLayer;

        [Header("Surface types:")] 
        public LayerMask[] surfaceLayers;
        
        public float honeySpeed = 2.5f;
        public float iceSpeed = 10f;
        
        [Header("Gravity stuff")]
        public float gravityMultiplier = 1f;
        
        // Public:
        public static BaseWorld World;
        public static SurfaceTypes SurfaceType;

        // Private:
        private float _gravityScale;

        public ref float GetGravityScale()
        {
            return ref _gravityScale;
        }

        private void Awake()
        {
            if (World == null)
            {
                World = this;
            }
            SurfaceType = new SurfaceTypes(surfaceLayers);
        }
    }
}
