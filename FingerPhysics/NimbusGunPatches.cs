using HarmonyLib;
using Il2CppSLZ.Marrow;

namespace FingerPhysics
{
    [HarmonyPatch(typeof(FlyingGun))]
    public class NimbusGunPatches
    {
        [HarmonyPatch(nameof(FlyingGun.EnableNoClip))]
        [HarmonyPostfix]
        public static void EnableNoClipPostfix(FlyingGun __instance)
        {
            Hand targetHand = __instance.triggerGrip.GetHand();
            ArtRig manager = targetHand.manager.physicsRig.artOutput;
            if(!ArtOutputUpdatePatch.TryGetReferences(manager, out var refs)) return;

            refs.leftHandFingers.SetCollisions(false);
            refs.rightHandFingers.SetCollisions(false);
        }


        [HarmonyPatch(nameof(FlyingGun.DisableNoClip))]
        [HarmonyPostfix]
        public static void DisableNoClipPostfix(FlyingGun __instance)
        {
            Hand targetHand = __instance.triggerGrip.GetHand();
            if(targetHand == null) return; // Hand has left the grip, so it should just do it normally.

            ArtRig manager = targetHand.manager.physicsRig.artOutput;
            if(!ArtOutputUpdatePatch.TryGetReferences(manager, out var refs)) return;

            if (targetHand.HasAttachedObject())
            {
                refs.leftHandFingers.SetCollisions(!refs.leftHandFingers.targetPhysHand.hand.HasAttachedObject());
                refs.rightHandFingers.SetCollisions(!refs.rightHandFingers.targetPhysHand.hand.HasAttachedObject());
            }
        }
    }
}
