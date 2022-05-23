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
        public static BaseWorld World;
        public static BaseSurfaceTypes FloorType;
        public static BaseSurfaceTypes WallType;
        public static GameObject Player;

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

            FloorType ??= new FloorTypes(floorLayers);
            WallType ??= new WallTypes(wallLayers);
            
        }
    }
}
