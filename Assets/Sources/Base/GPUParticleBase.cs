using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// GPUParticleの更新処理
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class GPUParticleBase<T> : MonoBehaviour where T : struct {

    #region define
    // ComputeShaderのスレッド数
    protected const int THREAD_NUM_X = 16;
    #endregion

    #region public
    public int particleMax = 1024;
    public int emitMax = 8;

    public ComputeShader cs;
    //public Material material;
    #endregion

    #region private
    protected ComputeBuffer particleBuffer;
    protected ComputeBuffer particlePoolBuffer;
    protected ComputeBuffer particleCountBuffer;
    protected int particleNum = 0;
    protected int emitNum = 0;
    protected int[] particleCounts = null;

    protected int initKernel = -1;
    protected int emitKernel = -1;
    protected int updateKernel = -1;

    protected int particlePoolNum;
    #endregion

    #region virtual
    public virtual int GetParticleNum() { return particleNum; }

    public virtual ComputeBuffer GetParticleBuffer() { return particleBuffer; }

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Initialize()
    {
        particleNum = (particleMax / THREAD_NUM_X) * THREAD_NUM_X;
        emitNum = (emitMax / THREAD_NUM_X) * THREAD_NUM_X;
        Debug.Log("particleNum " + particleNum + " emitNum " + emitNum + " THREAD_NUM_X " + THREAD_NUM_X);

        particleBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(T)), ComputeBufferType.Default);
        particlePoolBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        particlePoolBuffer.SetCounterValue(0);
        particleCountBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);
        particleCounts = new int[]{ 0, 1, 0, 0 };
        particleCountBuffer.SetData(particleCounts);

        initKernel = cs.FindKernel("Init");
        emitKernel = cs.FindKernel("Emit");
        updateKernel = cs.FindKernel("Update");

        Debug.Log("initKernel " + initKernel + " emitKernel " + emitKernel + " updateKernel " + updateKernel);

        cs.SetBuffer(initKernel, "_Particles", particleBuffer);
        cs.SetBuffer(initKernel, "_DeadList", particlePoolBuffer);
        cs.Dispatch(initKernel, particleNum / THREAD_NUM_X, 1, 1);
    }

    /// <summary>
    /// パーティクルの更新
    /// </summary>
    protected abstract void UpdateParticle();

    /// <summary>
    /// パーティクルの発生
    /// THREAD_NUM_X分発生
    /// </summary>
    protected virtual void EmitParticle() { }

    /// <summary>
    /// ComputeBufferの解放
    /// </summary>
    protected virtual void ReleaseBuffer() {
        if (particlePoolBuffer != null)
        {
            particlePoolBuffer.Release();
        }
        if (particleBuffer != null)
        {
            particleBuffer.Release();
        }
        if(particleCountBuffer != null)
        {
            particleCountBuffer.Release();
        }
    }

    // Use this for initialization
    protected virtual void Awake()
    {
        ReleaseBuffer();
        Initialize();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        UpdateParticle();
    }

    #endregion

    void OnDestroy()
    {
        ReleaseBuffer();
    }
}
