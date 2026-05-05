using System;
using Il2CppSLZ.Marrow;
using UnityEngine;
using MelonLoader;
using BoneLib;

namespace FingerPhysics;

[RegisterTypeInIl2Cpp]
public class PhysicalFinger : MonoBehaviour
{
    public const float fingerSectionMass = 0.5f;

    public PhysicalFingersController controllerReference
    {
        get
        {
            _controller ??= GetComponentInParent<PhysicalFingersController>();
            return _controller;
        }
    }
    private PhysicalFingersController _controller;

    private Quaternion handChildOffsetRotation => controllerReference.handedness == Il2CppSLZ.Marrow.Interaction.Handedness.LEFT ? Quaternion.Euler(270, 90, 0) : Quaternion.Euler(90, 270, 0);

    public Rigidbody handBodyReference;

    public Transform proximalTarget;
    public Rigidbody proximalBody;
    public CapsuleCollider proximalCol;
    public ConfigurableJoint proximalJoint;
    public Transform proximalArtOffset;

    public Transform intermediateTarget;
    public Rigidbody intermediateBody;
    public CapsuleCollider intermediateCol;
    public ConfigurableJoint intermediateJoint;
    public Transform intermediateArtOffset;

    public Transform distalTarget;
    public Rigidbody distalBody;
    public CapsuleCollider distalCol;
    public ConfigurableJoint distalJoint;
    public Transform distalArtOffset;

    public static PhysicalFinger CreateFinger(PhysHand handBody, Transform proxTarget, Transform intTarget, Transform distTarget, string name, Transform parent)
    {
        // Individual Finger Parent (might not be needed considering transform position performance, we will see)
        GameObject fingerControllerParent = new GameObject(name);
        PhysicalFinger physFinger = fingerControllerParent.AddComponent<PhysicalFinger>();
        fingerControllerParent.transform.SetParent(parent);

        physFinger.proximalTarget = proxTarget;
        physFinger.intermediateTarget = intTarget;
        physFinger.distalTarget = distTarget;

        // Create Proximal Bone
        {
            (_, physFinger.proximalBody, physFinger.proximalCol, physFinger.proximalJoint, physFinger.proximalArtOffset) = CreateFingerBone("Proximal", handBody.rbHand, proxTarget, intTarget.localPosition.magnitude, 0.01f, intTarget.localPosition / 2, true, handBody.rbHand.rotation * physFinger.handChildOffsetRotation);
            physFinger.proximalJoint.axis = Vector3.right;
            //physFinger.proximalJoint.angularXMotion = ConfigurableJointMotion.Limited;
            //physFinger.proximalJoint.angularYMotion = ConfigurableJointMotion.Limited;
            physFinger.proximalJoint.highAngularXLimit = new SoftJointLimit()
            {
                limit = 25f,
                bounciness = 0,
                contactDistance = 0
            };
            physFinger.proximalJoint.lowAngularXLimit = new SoftJointLimit()
            {
                limit = -25f,
                bounciness = 0,
                contactDistance = 0
            }; 
            //physFinger.proximalJoint.angularYLimit = new SoftJointLimit()
            //{
            //    limit = 45f,
            //    bounciness = 0,
            //    contactDistance = 0
            //};
            physFinger.proximalJoint.slerpDrive = new JointDrive()
            {
                positionSpring = 5000f,
                positionDamper = 100f,
                maximumForce = physFinger.proximalJoint.slerpDrive.maximumForce
            };
        }

        // Create Intermediate Bone
        {
            (_, physFinger.intermediateBody, physFinger.intermediateCol, physFinger.intermediateJoint, physFinger.intermediateArtOffset) = CreateFingerBone("Intermediate", physFinger.proximalBody, intTarget, distTarget.localPosition.magnitude, 0.01f, distTarget.localPosition / 2, true, handBody.rbHand.rotation * physFinger.handChildOffsetRotation); 
            physFinger.intermediateJoint.angularXMotion = ConfigurableJointMotion.Limited;
            physFinger.intermediateJoint.angularYMotion = ConfigurableJointMotion.Locked;
            physFinger.intermediateJoint.angularZMotion = ConfigurableJointMotion.Locked;
            physFinger.intermediateJoint.highAngularXLimit = new SoftJointLimit()
            {
                limit = 120f,
                bounciness = 0,
                contactDistance = 0
            };
            physFinger.intermediateJoint.lowAngularXLimit = new SoftJointLimit()
            {
                limit = -10f,
                bounciness = 0,
                contactDistance = 0
            };
        }

        // Create Distal Bone
        {
            (_, physFinger.distalBody, physFinger.distalCol, physFinger.distalJoint, physFinger.distalArtOffset) = CreateFingerBone("Distal", physFinger.intermediateBody, distTarget, distTarget.localPosition.magnitude, 0.01f, distTarget.localPosition / 2, true, handBody.rbHand.rotation * physFinger.handChildOffsetRotation);
            physFinger.distalJoint.angularXMotion = ConfigurableJointMotion.Limited;
            physFinger.distalJoint.angularYMotion = ConfigurableJointMotion.Locked;
            physFinger.distalJoint.angularZMotion = ConfigurableJointMotion.Locked;
            physFinger.distalJoint.highAngularXLimit = new SoftJointLimit()
            {
                limit = 120f,
                bounciness = 0,
                contactDistance = 0
            };
            physFinger.distalJoint.lowAngularXLimit = new SoftJointLimit()
            {
                limit = -10f,
                bounciness = 0,
                contactDistance = 0
            };
        }

        physFinger.UpdateFingerAnchors();

        return physFinger;

        (GameObject, Rigidbody, CapsuleCollider, ConfigurableJoint, Transform) CreateFingerBone(string extraName, Rigidbody jointedBody, Transform targetTransform, float boneLength, float boneRadius, Vector3 boneColliderCenter, bool rotOverride = false, Quaternion overrideQ = default)
        {
            GameObject fingerBone = new GameObject($"{name} {extraName}");
            fingerBone.transform.SetParent(parent);
            fingerBone.transform.position = targetTransform.position;
            fingerBone.transform.rotation = targetTransform.rotation;
            if (rotOverride) fingerBone.transform.rotation = overrideQ;
            fingerBone.layer = 8;

            // Create Rigidbody
            Rigidbody boneRb = fingerBone.AddComponent<Rigidbody>();
            boneRb.mass = fingerSectionMass;
            //boneRb.solverIterations = Physics.defaultSolverIterations * 8;

            // Create Collider
            CapsuleCollider boneCol = CreateCollider(fingerBone, 0, boneRadius, boneLength + (2 * boneRadius), boneColliderCenter);
            Physics.IgnoreCollision(boneCol, handBody.fingersCol, true);
            Physics.IgnoreCollision(boneCol, handBody.handCol, true);
            //var vis = new CapsuleColVis(boneCol);

            // Create and setup Joint
            ConfigurableJoint boneJoint = fingerBone.AddComponent<ConfigurableJoint>();
            boneJoint.connectedBody = jointedBody;
            LockMotion(boneJoint);
            boneJoint.anchor = Vector3.zero;
            boneJoint.autoConfigureConnectedAnchor = false;
            boneJoint.axis = Vector3.back;
            boneJoint.rotationDriveMode = RotationDriveMode.Slerp;
            boneJoint.slerpDrive = new JointDrive()
            {
                positionSpring = 2000f,
                positionDamper = 50f,
                maximumForce = boneJoint.slerpDrive.maximumForce
            };
            boneJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            boneJoint.projectionDistance = 0f;
            boneJoint.projectionAngle = 0f;

            // Create Art Offset
            GameObject artOffset = new GameObject("artOffset");
            artOffset.transform.SetParent(fingerBone.transform);
            artOffset.transform.localPosition = Vector3.zero;
            artOffset.transform.localRotation = Quaternion.identity;
            return (fingerBone, boneRb, boneCol, boneJoint, artOffset.transform);
        }

        CapsuleCollider CreateCollider(GameObject gobj, int direction, float radius, float height, Vector3 center)
        {
            CapsuleCollider fingerCol = gobj.AddComponent<CapsuleCollider>();
            fingerCol.direction = direction;
            fingerCol.radius = radius;
            fingerCol.height = height;
            fingerCol.center = center;
            return fingerCol;
        }

        void LockMotion(ConfigurableJoint joint)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }
    }

    public static void IgnoreFingers(PhysicalFinger finger1, PhysicalFinger finger2)
    {
        Physics.IgnoreCollision(finger1.proximalCol, finger2.proximalCol);
        Physics.IgnoreCollision(finger1.proximalCol, finger2.intermediateCol);
        Physics.IgnoreCollision(finger1.proximalCol, finger2.distalCol);

        Physics.IgnoreCollision(finger1.intermediateCol, finger2.proximalCol);
        Physics.IgnoreCollision(finger1.intermediateCol, finger2.intermediateCol);
        Physics.IgnoreCollision(finger1.intermediateCol, finger2.distalCol);

        Physics.IgnoreCollision(finger1.distalCol, finger2.proximalCol);
        Physics.IgnoreCollision(finger1.distalCol, finger2.intermediateCol);
        Physics.IgnoreCollision(finger1.distalCol, finger2.distalCol);
    }

    public void SetCollisions(bool enabled)
    {
        proximalBody.detectCollisions = enabled;
        intermediateBody.detectCollisions = enabled;
        distalBody.detectCollisions = enabled;
    }

    public void UpdateFingerAnchors()
    {
        proximalJoint.connectedAnchor = controllerReference.handedness == Il2CppSLZ.Marrow.Interaction.Handedness.LEFT
            ? Quaternion.Euler(270, 90, 0) * proximalTarget.localPosition
            : Quaternion.Euler(90, 270, 0) * proximalTarget.localPosition;
        proximalCol.center = intermediateTarget.localPosition / 2;
        proximalCol.height = intermediateTarget.localPosition.magnitude + (2 * 0.01f);

        intermediateJoint.connectedAnchor = intermediateTarget.localPosition;
        intermediateCol.center = distalTarget.localPosition / 2;
        intermediateCol.height = distalTarget.localPosition.magnitude + (2 * 0.01f);

        distalJoint.connectedAnchor = distalTarget.localPosition;
        distalCol.center = distalTarget.localPosition / 2;
        distalCol.height = distalTarget.localPosition.magnitude + (2 * 0.01f);
    }

    public void UpdateArtTransforms(Transform proximal, Transform intermediate, Transform distal)
    {
        proximalArtOffset.localPosition = proximal.localPosition;
        proximalArtOffset.localRotation = proximal.localRotation;

        intermediateArtOffset.localPosition = intermediate.localPosition;
        intermediateArtOffset.localRotation = intermediate.localRotation;

        distalArtOffset.localPosition = distal.localPosition;
        distalArtOffset.localRotation = distal.localRotation;
    }

    public void UpdateJointTargets()
    {
        proximalJoint.targetRotation = Quaternion.Inverse(proximalTarget.localRotation);

        intermediateJoint.targetRotation = Quaternion.AngleAxis(Quaternion.Angle(Quaternion.identity, intermediateTarget.localRotation), Vector3.right);
        distalJoint.targetRotation = Quaternion.AngleAxis(Quaternion.Angle(Quaternion.identity, distalTarget.localRotation), Vector3.right);
    }
}
