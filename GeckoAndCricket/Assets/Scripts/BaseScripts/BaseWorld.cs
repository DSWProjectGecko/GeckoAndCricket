using BaseScripts.SurfaceTypes;
using UnityEngine;

namespace BaseScripts
{ 
    public class BaseWorld : MonoBehaviour
    {
        [Header("World layers:")]
        public LayerMask ceilingLayer;

        [Header("Surface types:")] 
        public LayerMask[] floorLayers;
        public LayerMask[] wallLayers;
        
        public float honeySpeed = 2.5f;
        public float iceSpeed = 10f;
        
        [Header("Gravity stuff")]
        public float gravityMultiplier = 1f;
        
        // Public:
        public static BaseWorld world;
        public static BaseSurfaceTypes floorType;
        public static BaseSurfaceTypes wallType;
        public static GameObject player;

        // Private:
        private float _gravityScale;

        public ref float GetGravityScale()
        {
            return ref _gravityScale;
        }

        private void Awake()
        {
            if (world == null)
            {
                world = this;
            }

            floorType ??= new FloorTypes(floorLayers);
            wallType ??= new WallTypes(wallLayers);
            
        }
    }
}
