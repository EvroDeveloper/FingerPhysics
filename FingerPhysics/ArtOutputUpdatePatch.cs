using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Utilities;
using Il2CppSLZ.VRMK;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace FingerPhysics
{
    public class BitsAndBobs
    {
        public Avatar lastSetAvatar;
        public PhysicalFingersController leftHandFingers;
        public PhysicalFingersController rightHandFingers;
    }

    [HarmonyPatch(typeof(ArtRig))]
    public static class ArtOutputUpdatePatch
    {
        public static Dictionary<ArtRig, BitsAndBobs> artRigToUsefulReferences = [];

        [HarmonyPatch(nameof(ArtRig.ArtOutputLateUpdate))]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(ArtRig __instance)
        {
            if (!artRigToUsefulReferences.ContainsKey(__instance))
            {
                MelonLogger.Msg("No useful references for the Art Rig. Cant do anything yo");
                return;
            }

            BitsAndBobs refs = artRigToUsefulReferences[__instance];

            refs.leftHandFingers.OnApplyPoseToTransforms();
            refs.rightHandFingers.OnApplyPoseToTransforms();

            if (refs.lastSetAvatar == null)
            {
                MelonLogger.Msg("Havent found an avatar yet for this Art Rig. Bruh");
                return;
            }

            var lastFoundAvatar = refs.lastSetAvatar;

            CopyFingerToTransforms(refs.leftHandFingers.index,
                lastFoundAvatar.artTransforms.leftIndexProximal,
                lastFoundAvatar.artTransforms.leftIndexIntermediate,
                lastFoundAvatar.artTransforms.leftIndexDistal);

            CopyFingerToTransforms(refs.leftHandFingers.middle,
                lastFoundAvatar.artTransforms.leftMiddleProximal,
                lastFoundAvatar.artTransforms.leftMiddleIntermediate,
                lastFoundAvatar.artTransforms.leftMiddleDistal);

            CopyFingerToTransforms(refs.leftHandFingers.ring,
                lastFoundAvatar.artTransforms.leftRingProximal,
                lastFoundAvatar.artTransforms.leftRingIntermediate,
                lastFoundAvatar.artTransforms.leftRingDistal);

            CopyFingerToTransforms(refs.leftHandFingers.pinky,
                lastFoundAvatar.artTransforms.leftLittleProximal,
                lastFoundAvatar.artTransforms.leftLittleIntermediate,
                lastFoundAvatar.artTransforms.leftLittleDistal);

            CopyFingerToTransforms(refs.leftHandFingers.thumb,
                lastFoundAvatar.artTransforms.leftThumbProximal,
                lastFoundAvatar.artTransforms.leftThumbIntermediate,
                lastFoundAvatar.artTransforms.leftThumbDistal);


            CopyFingerToTransforms(refs.rightHandFingers.index,
                lastFoundAvatar.artTransforms.rightIndexProximal,
                lastFoundAvatar.artTransforms.rightIndexIntermediate,
                lastFoundAvatar.artTransforms.rightIndexDistal);

            CopyFingerToTransforms(refs.rightHandFingers.middle,
                lastFoundAvatar.artTransforms.rightMiddleProximal,
                lastFoundAvatar.artTransforms.rightMiddleIntermediate,
                lastFoundAvatar.artTransforms.rightMiddleDistal);

            CopyFingerToTransforms(refs.rightHandFingers.ring,
                lastFoundAvatar.artTransforms.rightRingProximal,
                lastFoundAvatar.artTransforms.rightRingIntermediate,
                lastFoundAvatar.artTransforms.rightRingDistal);

            CopyFingerToTransforms(refs.rightHandFingers.pinky,
                lastFoundAvatar.artTransforms.rightLittleProximal,
                lastFoundAvatar.artTransforms.rightLittleIntermediate,
                lastFoundAvatar.artTransforms.rightLittleDistal);

            CopyFingerToTransforms(refs.rightHandFingers.thumb, 
                lastFoundAvatar.artTransforms.rightThumbProximal,
                lastFoundAvatar.artTransforms.rightThumbIntermediate, 
                lastFoundAvatar.artTransforms.rightThumbDistal);
        }

        public static void CopyFingerToTransforms(PhysicalFinger finger, Transform proximal, Transform intermediate, Transform distal)
        {
            proximal.SetPositionAndRotation(finger.proximalArtOffset.position, finger.proximalArtOffset.rotation);
            intermediate.SetPositionAndRotation(finger.intermediateArtOffset.position, finger.intermediateArtOffset.rotation);
            distal.SetPositionAndRotation(finger.distalArtOffset.position, finger.distalArtOffset.rotation);
        }

        [HarmonyPatch(nameof(ArtRig.SetArtOutputAvatar))]
        [HarmonyPostfix]
        public static void SetAvatarPostfix(ArtRig __instance, Avatar avatar)
        {
            MelonLogger.Msg("Set Avatar Postfix is Called. Avatar has changed, updating cache with the thing");
            artRigToUsefulReferences[__instance].lastSetAvatar = avatar;

            foreach (var controller in __instance.GetComponentInParent<PhysicsRig>().GetComponentsInChildren<PhysicalFingersController>())
            {
                controller.OnAvatarSwapped(avatar);
            }
        }
    }

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
                ArtOutputUpdatePatch.artRigToUsefulReferences.Add(__instance.artOutput, newRig);
            }
        }
    }
}
