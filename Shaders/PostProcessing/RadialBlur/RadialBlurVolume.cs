using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[SerializeField, VolumeComponentMenu("Kampter/RadialBlur")]
public class RadialBlurVolume : VolumeComponent, IPostProcessComponent
{
    [Tooltip("模糊中心点")]
    public FloatParameter X = new FloatParameter(0.5f);
    public FloatParameter Y = new FloatParameter(0.5f);

    [Range(0f, 10f), Tooltip("模糊的迭代次数")]
    public IntParameter BlurTimes = new ClampedIntParameter(1, 1, 10);
    [Range(0f, 10f), Tooltip("模糊半径")]
    public FloatParameter BlurRange = new ClampedFloatParameter(1.0f, 0.0f, 10.0f);
    [Range(0f, 10f), Tooltip("降采样次数")]
    public IntParameter RTDownSampling = new ClampedIntParameter(1, 1, 10);
    [Range(0f, 10f), Tooltip("中心忽略模糊半径")]
    public FloatParameter BufferRadius = new ClampedFloatParameter(1.0f, 0.0f, 10.0f);

    public bool IsActive() => RTDownSampling.value > 0f;

    public bool IsTileCompatible() => false;

} 