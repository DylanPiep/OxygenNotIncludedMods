using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace StorageCompressor
{
    public static class StorageCompressorPatches
    {
        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_StorageCompressor_Patch
        {
            public static void Prefix ()
            {
                Debug.Log("[DylanPiep][StorageCompressor] ::: InitPatch @ " + System.DateTime.Now);
                StorageCompressorConfig.Setup();
            }
        }
    }
}
