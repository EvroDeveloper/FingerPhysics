using HarmonyLib;
using Il2CppSLZ.Marrow;
using System.Collections.Generic;

namespace FingerPhysics
{
    [HarmonyPatch(typeof(PhysHand))]
    public static class PhysHandPatch
    {
        static Dictionary<int, float> rbToOriginalMass = [];

        [HarmonyPatch(nameof(PhysHand.ApplyForce))]
        [HarmonyPrefix]
        public static void AddFingersMass(PhysHand __instance)
        {
            ArtRig thingas = __instance.physBody.artOutput;
            if(ArtOutputUpdatePatch.TryGetReferences(thingas, out _))
            {
                rbToOriginalMass[__instance.GetInstanceID()] = __instance.rbHand.mass;
                __instance.rbHand.mass += PhysicalFinger.fingerSectionMass * 3 * 5;
            }
        }

        [HarmonyPatch(nameof(PhysHand.ApplyForce))]
        [HarmonyPostfix]
        public static void RemoveFingerMass(PhysHand __instance)
        {
            ArtRig thingas = __instance.physBody.artOutput;
            if(ArtOutputUpdatePatch.TryGetReferences(thingas, out _))
            {
                __instance.rbHand.mass = rbToOriginalMass[__instance.GetInstanceID()];
            }
        }
    }
}
