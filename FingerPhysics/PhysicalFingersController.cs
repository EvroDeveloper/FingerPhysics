using System;
using MelonLoader;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.VRMK;
using UnityEngine;
using System.Collections.Generic;

namespace FingerPhysics;

[RegisterTypeInIl2Cpp]
public class PhysicalFingersController : MonoBehaviour
{
    public PhysHand targetPhysHand;
    public ArtRig artOutputRig;
    public HandPoseAnimator targetHandAnimator;

    public Handedness handedness => targetHandAnimator.handedness;
    public PhysicalFinger thumb;
    public PhysicalFinger index;
    public PhysicalFinger middle;
    public PhysicalFinger ring;
    public PhysicalFinger pinky;

    private bool _previousAttachedState = false;

    private float _timeSinceLastRelease = 0f;
    private bool _waitingToEnableCollisions = false;

    void Update()
    {
        _timeSinceLastRelease += Time.deltaTime;
        if(_timeSinceLastRelease > 0.5f && _waitingToEnableCollisions)
        {
            SetCollisions(true);
            _waitingToEnableCollisions = false;
        }
    }

    public void OnGrabbedInteractable()
    {
        SetCollisions(false);
        _waitingToEnableCollisions = false;
    }

    public void OnReleasedInteractable()
    {
        _timeSinceLastRelease = 0f;
        _waitingToEnableCollisions = true;
    }

    public void SetCollisions(bool collide)
    {
        thumb.SetCollisions(collide);
        index.SetCollisions(collide);
        middle.SetCollisions(collide);
        ring.SetCollisions(collide);
        pinky.SetCollisions(collide);
    }


    public void OnAvatarSwapped(Avatar avatar)
    {
        thumb.UpdateFingerAnchors();
        index.UpdateFingerAnchors();
        middle.UpdateFingerAnchors();
        ring.UpdateFingerAnchors();
        pinky.UpdateFingerAnchors();

        if (handedness == Handedness.LEFT)
        {
            thumb.UpdateArtTransforms(artOutputRig.artThumbLf1, artOutputRig.artThumbLf2, artOutputRig.artThumbLf3);
            index.UpdateArtTransforms(artOutputRig.artFingerLf11, artOutputRig.artFingerLf12, artOutputRig.artFingerLf13);
            middle.UpdateArtTransforms(artOutputRig.artFingerLf21, artOutputRig.artFingerLf22, artOutputRig.artFingerLf23);
            ring.UpdateArtTransforms(artOutputRig.artFingerLf31, artOutputRig.artFingerLf32, artOutputRig.artFingerLf33);
            pinky.UpdateArtTransforms(artOutputRig.artFingerLf41, artOutputRig.artFingerLf42, artOutputRig.artFingerLf43);
        }
        else
        {
            thumb.UpdateArtTransforms(artOutputRig.artThumbRt1, artOutputRig.artThumbRt2, artOutputRig.artThumbRt3);
            index.UpdateArtTransforms(artOutputRig.artFingerRt11, artOutputRig.artFingerRt12, artOutputRig.artFingerRt13);
            middle.UpdateArtTransforms(artOutputRig.artFingerRt21, artOutputRig.artFingerRt22, artOutputRig.artFingerRt23);
            ring.UpdateArtTransforms(artOutputRig.artFingerRt31, artOutputRig.artFingerRt32, artOutputRig.artFingerRt33);
            pinky.UpdateArtTransforms(artOutputRig.artFingerRt41, artOutputRig.artFingerRt42, artOutputRig.artFingerRt43);
        }
    }

    public void OnApplyPoseToTransforms()
    {
        thumb.UpdateJointTargets();
        index.UpdateJointTargets();
        middle.UpdateJointTargets();
        ring.UpdateJointTargets();
        pinky.UpdateJointTargets();

        bool attached = targetPhysHand.hand.AttachedReceiver != null;
        bool hovering = targetPhysHand.hand.HoveringReceiver != null;

        bool attachedOrHovering = attached || hovering;

        if (_previousAttachedState != (targetPhysHand.hand.AttachedReceiver != null))
        {
            _previousAttachedState = (targetPhysHand.hand.AttachedReceiver != null);
            if(_previousAttachedState)
            {
                OnGrabbedInteractable();
            }
            else
            {
                OnReleasedInteractable();
            }
        }
    }

    public static PhysicalFingersController CreatePhysicalFingers(PhysHand physHand)
    {
        GameObject physicalFingersContainer = new GameObject($"PhysicalFingers Controller ({ (physHand.hand.handedness == Handedness.LEFT ? "Left" : "Right") })");
        PhysicalFingersController controller = physicalFingersContainer.AddComponent<PhysicalFingersController>();
        physicalFingersContainer.transform.SetParent(physHand.physBody.transform);
        controller.targetPhysHand = physHand;

        HandPoseAnimator targetPoseAnim = physHand.hand.Animator;
        controller.targetHandAnimator = targetPoseAnim;
        controller.artOutputRig = physHand.physBody.artOutput;

        controller.thumb = PhysicalFinger.CreateFinger(physHand, targetPoseAnim.thumb1, targetPoseAnim.thumb2, targetPoseAnim.thumb3, "Thumb", controller.transform);
        controller.index = PhysicalFinger.CreateFinger(physHand, targetPoseAnim.index1, targetPoseAnim.index2, targetPoseAnim.index3, "Index", controller.transform);
        controller.middle = PhysicalFinger.CreateFinger(physHand, targetPoseAnim.middle1, targetPoseAnim.middle2, targetPoseAnim.middle3, "Middle", controller.transform);
        controller.ring = PhysicalFinger.CreateFinger(physHand, targetPoseAnim.ring1, targetPoseAnim.ring2, targetPoseAnim.ring3, "Ring", controller.transform);
        controller.pinky = PhysicalFinger.CreateFinger(physHand, targetPoseAnim.pinky1, targetPoseAnim.pinky2, targetPoseAnim.pinky3, "Pinky", controller.transform);

        controller.thumb.proximalJoint.angularXMotion = ConfigurableJointMotion.Free;
        controller.thumb.proximalJoint.angularYMotion = ConfigurableJointMotion.Free;
        controller.thumb.proximalJoint.angularZMotion = ConfigurableJointMotion.Free;

        PhysicalFinger.IgnoreFingers(controller.index, controller.middle);
        PhysicalFinger.IgnoreFingers(controller.index, controller.ring);
        PhysicalFinger.IgnoreFingers(controller.index, controller.pinky);
        PhysicalFinger.IgnoreFingers(controller.index, controller.thumb);

        PhysicalFinger.IgnoreFingers(controller.middle, controller.ring);
        PhysicalFinger.IgnoreFingers(controller.middle, controller.pinky);
        PhysicalFinger.IgnoreFingers(controller.middle, controller.thumb);

        PhysicalFinger.IgnoreFingers(controller.ring, controller.pinky);
        PhysicalFinger.IgnoreFingers(controller.ring, controller.thumb);

        PhysicalFinger.IgnoreFingers(controller.pinky, controller.thumb);

        return controller;
    }

    public void OnPlayerRangeChanged(bool inRangeOfPlayer)
    {

    }
}
