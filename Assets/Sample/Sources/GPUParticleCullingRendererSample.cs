using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPUParticleCullingRendererSample : GPUParticleCullingRendererBase<GPUParticleData> {

    protected override void SetMaterialParam()
    {
        material.SetBuffer("_Particles", particleBuffer);
        material.SetBuffer("_ParticleActiveList", activeIndexBuffer);
        material.SetFloat("_Scale", scale);

        material.SetPass(0);
    }

    void OnGUI()
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

                    GUI.Label(new Rect(10, 72 + i * 64, 240, 64), "InViews / Active " + data.GetInViewNum() + " / " + particle.GetActiveParticleNum());
                    i++;
                }
            });
        }
        else
        {
            GUI.Label(new Rect(10, 72, 480, 64), "[No Culling]Active / Pool " + particle.GetActiveParticleNum() + " / " + particle.GetPoolParticleNum());
        }
    }
}
