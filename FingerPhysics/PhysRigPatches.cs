using HarmonyLib;
using Il2CppSLZ.Marrow;

namespace FingerPhysics
{
    [HarmonyPatch(typeof(PhysicsRig))]
    public static class PhysRigPatches
    {
        [HarmonyPatch(nameof(PhysicsRig.OnAwake))]
        [HarmonyPostfix]
        public static void OnAwake(PhysicsRig __instance)
        {
            if (Bone_Menu_Creator.ModEnabled)
            {
                BitsAndBobs newRig = new BitsAndBobs();
                newRig.leftHandFingers = PhysicalFingersController.CreatePhysicalFingers(__instance.leftHand.physHand);
                newRig.rightHandFingers = PhysicalFingersController.CreatePhysicalFingers(__instance.rightHand.physHand);

                int instanceId = __instance.artOutput.GetInstanceID();
                ArtOutputUpdatePatch.artRigToUsefulReferences.Add(instanceId, newRig);
            }
        }
    }
}
