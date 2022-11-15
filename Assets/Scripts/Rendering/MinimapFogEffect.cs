using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenu("Custom/MinimapFog")]
public class MnimapFogEffect : VolumeComponent, IPostProcessComponent
{
	/* IsActive is pretty self-explanitory. No idea what IsTileCompatible is for, but
	 * it's required so that's why it's here. Hopefully it's sane to set it to true.*/
	public bool IsActive() => true;
	public bool IsTileCompatible() => true;
}