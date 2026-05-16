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
        public static void DisableNoClipPostfix(FlyingGun __instance, Hand hand)
        {
            ArtRig manager = hand.manager.physicsRig.artOutput;
            if(!ArtOutputUpdatePatch.TryGetReferences(manager, out var refs)) return;

            if (hand.AttachedReceiver != null)
            {
                refs.leftHandFingers.SetCollisions(refs.leftHandFingers.targetPhysHand.hand.AttachedReceiver == null);
                refs.rightHandFingers.SetCollisions(refs.rightHandFingers.targetPhysHand.hand.AttachedReceiver == null);
            }
        }
    }
}
