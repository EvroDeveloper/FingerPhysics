using System;
using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.VRMK;
using UnityEngine;

namespace FingerPhysics;

public class PhysicalFingersController : MonoBehaviour
{
    public Handedness handedness;
    public PhysicalFinger index;
    public PhysicalFinger middle;
    public PhysicalFinger ring;
    public PhysicalFinger pinky;


    public void OnAvatarSwapped(Avatar avatar)
    {
        if(handedness == Handedness.LEFT)
        {
            avatar.hand
        }
    }
}
