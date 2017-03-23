using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUParticleRendererSample : GPUParticleRendererBase<GPUParticleData> {

    protected override void SetMaterialParam()
    {
        material.SetBuffer("_Particles", particleBuffer);
        material.SetBuffer("_ParticleActiveList", activeIndexBuffer);

        material.SetPass(0);
    }

}
