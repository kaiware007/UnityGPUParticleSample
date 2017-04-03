using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUParticleRendererSample : GPUParticleRendererBase<GPUParticleData> {

    protected override void SetMaterialParam()
    {
        material.SetBuffer("_Particles", particleBuffer);
        material.SetBuffer("_ParticleActiveList", activeIndexBuffer);
        material.SetFloat("_Scale", 1f);

        material.SetPass(0);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 72, 480, 64), "Active / Pool " + particle.GetActiveParticleNum() + " / " + particle.GetPoolParticleNum());
    }
}
