using System.Collections.Generic;

namespace BaseScripts.SurfaceTypes
{ 
    public abstract class BaseSurfaceTypes
    {
        public int? Lava;
        public int? Honey;
        public int? Ice;
        public int? Normal;
        public IEnumerable<int?> GetSurfaceTypes()
        {
            return new [] {Lava, Honey, Ice, Normal};
        }
    }
}