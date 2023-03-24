using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[SerializeField, VolumeComponentMenu("Kampter/ColotTint")]
public class ColorTintVolume : VolumeComponent, IPostProcessComponent
{
    public BoolParameter on = new BoolParameter(false);
    public ColorParameter ColorChange = new ColorParameter(Color.white, true);
    public bool IsActive() => on.value;
    public bool IsTileCompatible() => false;
}
