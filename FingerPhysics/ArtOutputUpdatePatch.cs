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

            float distanceToPlayer = Vector3.Distance(__instance.artHead.position, BoneLib.Player.Head.position);
            if (refs.inRangeOfPlayer != (distanceToPlayer < 10f))
            {
                refs.inRangeOfPlayer = (distanceToPlayer < 10f);
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
            MelonLogger.Msg("Set Avatar Postfix is Called. Avatar has changed, updating cache with the thing");
            BitsAndBobs refs = artRigToUsefulReferences[__instance];
            refs.lastSetAvatar = avatar;

            refs.leftHandFingers.OnAvatarSwapped(avatar);
            refs.rightHandFingers.OnAvatarSwapped(avatar);
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


                MelonLogger.Msg("Adding THE ART RIG to the DICTIONARY");
                ArtOutputUpdatePatch.artRigToUsefulReferences.Add(__instance.artOutput, newRig);
                MelonLogger.Msg("Art Rig has been ADDED to the DICTIONARY. LETS CHECK IT NOW");
                MelonLogger.Msg("Does the art rig dictionry contain key for it? true or false: " + ArtOutputUpdatePatch.artRigToUsefulReferences.ContainsKey(__instance.artOutput));
                // The line above returns False on LemonLoader for some ungodly reason. Why is it doing this and how do i fix it.
            }
        }
    }

    [HarmonyPatch(typeof(FlyingGun), nameof(FlyingGun.EnableNoClip))]
    public class OnFlyingGunClipEnable
    {
        public static void Postfix(FlyingGun __instance)
        {
            Hand targetHand = __instance.triggerGrip.GetHand();
            ArtRig manager = targetHand.manager.physicsRig.artOutput;

            BitsAndBobs refs = ArtOutputUpdatePatch.artRigToUsefulReferences[manager];
            refs.leftHandFingers.SetCollisions(false);
            refs.rightHandFingers.SetCollisions(false);
        }
    }

    [HarmonyPatch(typeof(FlyingGun), nameof(FlyingGun.DisableNoClip))]
    public class OnFlyingGunClipDisable
    {
        public static void Postfix(FlyingGun __instance, Hand hand)
        {
            ArtRig manager = hand.manager.physicsRig.artOutput;

            BitsAndBobs refs = ArtOutputUpdatePatch.artRigToUsefulReferences[manager];
            if (hand.AttachedReceiver != null)
            {
                refs.leftHandFingers.SetCollisions(refs.leftHandFingers.targetPhysHand.hand.AttachedReceiver == null);
                refs.rightHandFingers.SetCollisions(refs.rightHandFingers.targetPhysHand.hand.AttachedReceiver == null);
            }
        }
    }
}
