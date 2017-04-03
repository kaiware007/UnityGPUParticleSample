using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Runtime.InteropServices;

public class GPUParticleInstancingRendererSample : GPUParticleInstancingRendererBase<GPUParticleData> {

    private void OnGUI()
    {
        if (isCulling)
        {
            int i = 0;
            cameraDatas.Keys
            .Where(cam => cam.isActiveAndEnabled)
            .ToList().ForEach(cam =>
            {
                CullingData data = cameraDatas[cam];
                if (data != null)
                {
                    data.inViewsCountBuffer.GetData(data.inViewsCounts);

                    GUI.Label(new Rect(10, 72 + i * 64, 480, 64), "[Culling] InViews / Active / Pool " + data.GetInViewNum() + " / " + particle.GetActiveParticleNum() + " / " + particle.GetPoolParticleNum());
                    i++;
                }
            });
        }else
        {
            GUI.Label(new Rect(10, 72, 480, 64), "[No Culling]Active / Pool " + particle.GetActiveParticleNum() + " / " + particle.GetPoolParticleNum());
        }
    }
}
