using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public struct GPUParticleData
{
    public bool isActive;       // 有効フラグ
    public Vector3 position;    // 座標
    public Vector3 velocity;    // 加速度
    public Color color;         // 色
    public float duration;      // 生存時間
    public float scale;         // サイズ
}

public class GPUParticleSample : GPUParticleBase<GPUParticleData> {

    #region public
    public float velocityMax = 1000f;
    public float lifeTime = 1;
    public float scaleMin = 1;
    public float scaleMax = 2;
    public float gravity = 9.8f;

    [Range(0,1)]
    public float sai = 1;   // 彩度
    [Range(0,1)]
    public float val = 1;   // 明るさ

    public Camera camera;
    #endregion

    /// <summary>
    /// パーティクルの更新
    /// </summary>
    protected override void UpdateParticle()
    {
        particleActiveBuffer.SetCounterValue(0);

        cs.SetFloat("_DT", Time.deltaTime);
        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetFloat("_Gravity", gravity);
        cs.SetBuffer(updateKernel, "_Particles", particleBuffer);
        cs.SetBuffer(updateKernel, "_DeadList", particlePoolBuffer);
        cs.SetBuffer(updateKernel, "_ActiveList", particleActiveBuffer);

        cs.Dispatch(updateKernel, particleNum / THREAD_NUM_X, 1, 1);

        particleActiveCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particleActiveBuffer, particleActiveCountBuffer, 0);
        particleActiveCountBuffer.GetData(particleCounts);
        particleActiveNum = particleCounts[0];
    }

    /// <summary>
    /// パーティクルの発生
    /// THREAD_NUM_X分発生
    /// </summary>
    /// <param name="position"></param>
    void EmitParticle(Vector3 position)
    {
        particlePoolCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particlePoolBuffer, particlePoolCountBuffer, 0);
        particlePoolCountBuffer.GetData(particleCounts);
        //Debug.Log("EmitParticle Pool Num " + particleCounts[0] + " position " + position);
        particlePoolNum = particleCounts[0];

        if (particleCounts[0] < emitNum) return;   // emitNum未満なら発生させない

        cs.SetVector("_EmitPosition", position);
        cs.SetFloat("_VelocityMax", velocityMax);
        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetFloat("_ScaleMin", scaleMin);
        cs.SetFloat("_ScaleMax", scaleMax);
        cs.SetFloat("_Sai", sai);
        cs.SetFloat("_Val", val);
        cs.SetFloat("_Time", Time.time);
        cs.SetBuffer(emitKernel, "_ParticlePool", particlePoolBuffer);
        cs.SetBuffer(emitKernel, "_Particles", particleBuffer);

        //cs.Dispatch(emitKernel, particleCounts[0] / THREAD_NUM_X, 1, 1);
        cs.Dispatch(emitKernel, emitNum / THREAD_NUM_X, 1, 1);   // emitNumの数だけ発生
    }

    // Update is called once per frame
    protected override void Update () {
        if (Input.GetMouseButton(0))
        {
            Vector3 mpos = Input.mousePosition;
            mpos.z = 10;
            Vector3 pos = camera.ScreenToWorldPoint(mpos);
            EmitParticle(pos);
        }
        UpdateParticle();
    }

    void OnGUI() {
        GUILayout.Box("Active " + particleActiveNum + " : Pool " + particlePoolNum + "/" + particleNum);
    }

}
