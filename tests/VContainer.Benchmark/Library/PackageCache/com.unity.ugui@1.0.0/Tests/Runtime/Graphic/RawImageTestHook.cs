using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawImageTestHook : RawImage
{
    public bool isGeometryUpdated;
    public bool isCacheUsed;
    public bool isLayoutRebuild;
    public bool isMaterialRebuild;

    public void ResetTest()
    {
        isGeometryUpdated = false;
        isLayoutRebuild = false;
        isMaterialRebuild = false;
        isCacheUsed = false;
    }

    public override void SetLayoutDirty()
    {
        base.SetLayoutDirty();
        isLayoutRebuild = true;
    }

    public override void SetMaterialDirty()
    {
        base.SetMaterialDirty();
        isMaterialRebuild = true;
    }

    protected override void UpdateGeometry()
    {
        base.UpdateGeometry();
        isGeometryUpdated = true;
    }
}
