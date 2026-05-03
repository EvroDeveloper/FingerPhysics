using System;
using Il2CppSLZ.Marrow;
using UnityEngine;
using MelonLoader;
using UnityEngine.Playables;

namespace FingerPhysics;

[RegisterTypeInIl2Cpp]
public class PhysicalFinger : MonoBehaviour
{
    public struct FingerLayout
    {
        public Quaternion defaultProximalRotation;
        public Vector3 proximalOffsetInHandSpace;
        public float intermediateDistance;
        public float distalDistance;
        public float distalLength;
        public float proximalRadius;
        public float distalRadius;
    }
    public Rigidbody handBodyReference;
    public Rigidbody proximalBody;
    public ConfigurableJoint proximalJoint;
    public Rigidbody intermediateBody;
    public ConfigurableJoint intermediateJoint;
    public Rigidbody distalBody;
    public ConfigurableJoint distalJoint;

    public static PhysicalFinger CreateFinger(PhysHand handBody, FingerLayout fingerLayout)
    {
        GameObject fingerControllerParent = new GameObject("New Finger");
        PhysicalFinger output = fingerControllerParent.AddComponent<PhysicalFinger>();

        GameObject fingerProximal = new GameObject("Proximal");
        fingerProximal.transform.SetParent(fingerControllerParent.transform);
        output.proximalBody = fingerProximal.AddComponent<Rigidbody>();
        output.proximalJoint = fingerProximal.AddComponent<ConfigurableJoint>();

        return output;
    }

    public void UpdateFingerLayout(FingerLayout fingerLayout)
    {
        DestroyImmediate(proximalJoint);
        proximalBody.position = handBodyReference.transform.TransformPoint(fingerLayout.proximalOffsetInHandSpace);
        proximalBody.rotation = handBodyReference.transform.rotation * fingerLayout.defaultProximalRotation;
        proximalBody.velocity = handBodyReference.velocity;
        proximalBody.angularVelocity = handBodyReference.angularVelocity;
        proximalJoint = proximalBody.gameObject.AddComponent<ConfigurableJoint>();
    }
}
