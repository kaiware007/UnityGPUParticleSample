using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Runtime.InteropServices;

public class GPUParticleInstancingRendererSample : GPUParticleInstancingRendererBase<GPUParticleData> {

    Rect rect = new Rect(0, 32, 200, 32);

    private void OnGUI()
    {
        if (isCulling)
        {
            GUIStyle guiStyle = GUI.skin.box;
            guiStyle.alignment = TextAnchor.UpperLeft;
            cameraDatas.Keys
            .Where(cam => cam.isActiveAndEnabled)
            .ToList().ForEach(cam =>
            {
                CullingData data = cameraDatas[cam];
                if (data != null)
                {
                    data.inViewsCountBuffer.GetData(data.inViewsCounts);

                    GUI.Box(rect, "InViews / Active " + data.inViewsNum + " / " + particle.GetActiveParticleNum(), GUI.skin.box);
                }
            });
        }
    }
}
