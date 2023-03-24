using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[SerializeField, VolumeComponentMenu("Kampter/ScreenSpaceReflection")]
public class ScreenSpaceReflectionVolume : VolumeComponent, IPostProcessComponent
{
    public BoolParameter on = new BoolParameter(false);
    public ClampedIntParameter RTsize = new ClampedIntParameter(512, 128, 720, false);
    public FloatParameter ReflectHeight = new FloatParameter(0.2f, false);
    public ClampedFloatParameter fadeOutRange = new ClampedFloatParameter(0.3f, 0.0f, 1.0f, false);
    public bool IsActive() => on.value;
    public bool IsTileCompatible() => false;
}
