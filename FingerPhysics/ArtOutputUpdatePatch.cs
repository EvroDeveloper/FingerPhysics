using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.VRMK;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace FingerPhysics
{
    public class BitsAndBobs
    {
        public bool inRangeOfPlayer = true;
        public Avatar lastSetAvatar;
        public PhysicalFingersController leftHandFingers;
        public PhysicalFingersController rightHandFingers;
    }

    [HarmonyPatch(typeof(ArtRig))]
    public static class ArtOutputUpdatePatch
    {
        /// <summary>
        /// Maps ArtRig instance IDs to their associated finger physics data.
        /// Uses instance IDs instead of Il2Cpp object references to ensure reliable dictionary lookups,
        /// as Il2Cpp wrapped objects do not have stable hash codes or equality comparisons.
        /// </summary>
        public static Dictionary<int, BitsAndBobs> artRigToUsefulReferences = [];

        public static bool TryGetReferences(ArtRig artRig, out BitsAndBobs references)
        {
            return artRigToUsefulReferences.TryGetValue(artRig.GetInstanceID(), out references);
        }

        [HarmonyPatch(nameof(ArtRig.ArtOutputLateUpdate))]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(ArtRig __instance)
        {
            if(!TryGetReferences(__instance, out var refs)) return;

            float distanceToPlayer = Vector3.Distance(__instance.artHead.position, BoneLib.Player.Head.position);
            bool inRange = distanceToPlayer < 10f;
            if (refs.inRangeOfPlayer != inRange)
            {
                refs.inRangeOfPlayer = inRange;
                refs.leftHandFingers.OnPlayerRangeChanged(refs.inRangeOfPlayer);
                refs.rightHandFingers.OnPlayerRangeChanged(refs.inRangeOfPlayer);
                // Update player range things
            }

            if (refs.inRangeOfPlayer)
            {
                refs.leftHandFingers.OnApplyPoseToTransforms();
                refs.rightHandFingers.OnApplyPoseToTransforms();

                if (refs.lastSetAvatar == null)
                {
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
        }

        public static void CopyFingerToTransforms(PhysicalFinger finger, Transform proximal, Transform intermediate, Transform distal)
        {
            if(proximal != null)
                proximal.SetPositionAndRotation(finger.proximalArtOffset.position, finger.proximalArtOffset.rotation);
            if(intermediate != null)
                intermediate.SetPositionAndRotation(finger.intermediateArtOffset.position, finger.intermediateArtOffset.rotation);
            if(distal != null)
                distal.SetPositionAndRotation(finger.distalArtOffset.position, finger.distalArtOffset.rotation);
        }

        [HarmonyPatch(nameof(ArtRig.SetArtOutputAvatar))]
        [HarmonyPostfix]
        public static void SetAvatarPostfix(ArtRig __instance, Avatar avatar)
        {
            if(!TryGetReferences(__instance, out var refs)) return;
            refs.lastSetAvatar = avatar;

            refs.leftHandFingers.OnAvatarSwapped(avatar);
            refs.rightHandFingers.OnAvatarSwapped(avatar);
        }
    }
}
