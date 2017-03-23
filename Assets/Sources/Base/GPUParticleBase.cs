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
    protected const int THREAD_NUM_X = 32;
    #endregion

    #region public
    public int particleMax = 1024;
    public int emitMax = 8;

    public ComputeShader cs;
    #endregion

    #region private
    protected ComputeBuffer particleBuffer;         // パーティクル構造体のバッファ
    protected ComputeBuffer particleActiveBuffer;   // 使用中のパーティクルのインデックスのリスト
    protected ComputeBuffer particlePoolBuffer;     // 未使用中のパーティクルのインデックスのリスト
    protected ComputeBuffer particleActiveCountBuffer;  // particleActiveBuffer内の個数バッファ
    protected ComputeBuffer particlePoolCountBuffer;    // particlePoolBuffer内の個数バッファ
    protected int particleNum = 0;
    protected int emitNum = 0;
    protected int[] particleCounts = null;

    protected int initKernel = -1;
    protected int emitKernel = -1;
    protected int updateKernel = -1;

    protected int particleActiveNum;
    protected int particlePoolNum;
    #endregion

    #region virtual
    public virtual int GetParticleNum() { return particleNum; }
    public virtual int GetActiveParticleNum() { return particleActiveNum; }

    public virtual ComputeBuffer GetParticleBuffer() { return particleBuffer; }
    public virtual ComputeBuffer GetActiveParticleBuffer() { return particleActiveBuffer; }

    public virtual ComputeBuffer GetParticleCountBuffer() { return particleActiveCountBuffer; }

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Initialize()
    {
        particleNum = (particleMax / THREAD_NUM_X) * THREAD_NUM_X;
        emitNum = (emitMax / THREAD_NUM_X) * THREAD_NUM_X;
        Debug.Log("particleNum " + particleNum + " emitNum " + emitNum + " THREAD_NUM_X " + THREAD_NUM_X);

        particleBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(T)), ComputeBufferType.Default);
        particleActiveBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        particleActiveBuffer.SetCounterValue(0);
        particlePoolBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        particlePoolBuffer.SetCounterValue(0);
        particleActiveCountBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);
        particlePoolCountBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);
        particleCounts = new int[]{ 0, 1, 0, 0 };
        particlePoolCountBuffer.SetData(particleCounts);
        
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
        if(particleActiveBuffer != null)
        {
            particleActiveBuffer.Release();
            particleActiveBuffer = null;
        }
        if (particlePoolBuffer != null)
        {
            particlePoolBuffer.Release();
            particlePoolBuffer = null;
        }
        if (particleBuffer != null)
        {
            particleBuffer.Release();
            particleBuffer = null;
        }
        if(particlePoolCountBuffer != null)
        {
            particlePoolCountBuffer.Release();
            particlePoolCountBuffer = null;
        }
        if(particleActiveCountBuffer != null)
        {
            particleActiveCountBuffer.Release();
            particleActiveCountBuffer = null;
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
