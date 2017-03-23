using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GPUParticleのレンダリング処理
/// </summary>
/// <typeparam name="T"></typeparam>
public class GPUParticleRendererBase <T> : MonoBehaviour where T : struct {

    #region public
    public Material material;
    #endregion

    #region protected
    protected GPUParticleBase<T> particle;
    protected int particleNum;
    protected ComputeBuffer particleBuffer;
    #endregion

    #region abstract
    protected virtual void SetMaterialParam() { }
    #endregion

    #region virtual
    protected void Start()
    {
        particle = GetComponent<GPUParticleBase<T>>();
        if (particle != null)
        {
            particleNum = particle.GetParticleNum();
            particleBuffer = particle.GetParticleBuffer();
            Debug.Log("particleNum " + particleNum);
        }else
        {
            Debug.LogError("Particle Class Not Found!!" + typeof(GPUParticleBase<T>).FullName);
        }
    }

    protected virtual void OnRenderObjectInternal()
    {
        SetMaterialParam();

        Graphics.DrawProcedural(MeshTopology.Points, particleNum);
    }
    #endregion

    #region private
    void OnRenderObject()
    {
        OnRenderObjectInternal();
    }
    #endregion
}
