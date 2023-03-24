using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[SerializeField, VolumeComponentMenu("Kampter/GaussianBlur")]
public class GaussianBlurVolume : VolumeComponent, IPostProcessComponent
{
    [Range(0, 4), Tooltip("模糊的迭代次数")]
    public ClampedIntParameter iterations = new ClampedIntParameter(3, 0, 4);
    [Range(0.2f, 3.0f), Tooltip("模糊半径")]
    public ClampedFloatParameter blurSpread = new ClampedFloatParameter(0.6f, 0.2f, 3.0f);
    [Range(1, 8), Tooltip("降采样次数")]
    public ClampedIntParameter RTdownSample = new ClampedIntParameter(2, 1, 8);
    public bool IsActive() => RTdownSample.value > 0f;
    public bool IsTileCompatible() => false;
}
