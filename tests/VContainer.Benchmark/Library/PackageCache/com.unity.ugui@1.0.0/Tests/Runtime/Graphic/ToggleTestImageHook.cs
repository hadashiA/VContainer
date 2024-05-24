using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTestImageHook : Image
{
    public float durationTween;
    public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
    {
        durationTween = duration;
        base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
    }
}
