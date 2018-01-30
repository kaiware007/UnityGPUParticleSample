using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Utility;

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
    //public Material material;
    #endregion

    #region private
    protected ComputeBuffer particleBuffer;         // パーティクル構造体のバッファ
    protected ComputeBuffer particleActiveBuffer;   // 使用中のパーティクルのインデックスのリスト
    protected ComputeBuffer particlePoolBuffer;     // 未使用中のパーティクルのインデックスのリスト
    protected ComputeBuffer particleActiveCountBuffer;  // particleActiveBuffer内の個数バッファ
    protected ComputeBuffer particlePoolCountBuffer;    // particlePoolBuffer内の個数バッファ
    protected int particleNum = 0;
    protected int emitNum = 0;
    protected int[] particleCounts = { 1, 1, 0, 0 };    // [0]インスタンスあたりの頂点数 [1]インスタンス数 [2]開始する頂点位置 [3]開始するインスタンス

    protected int initKernel = -1;
    protected int emitKernel = -1;
    protected int updateKernel = -1;

    //protected int particleActiveNum = 0;
    protected int particlePoolNum = 0;

    protected int cspropid_Particles;
    protected int cspropid_DeadList;
    protected int cspropid_ActiveList;
    protected int cspropid_EmitNum;
    protected int cspropid_ParticlePool;

	protected bool isInitialized = false;
    #endregion

    #region virtual
    public virtual int GetParticleNum() { return particleNum; }

    private int[] debugounts = { 0, 0, 0, 0 };
    /// <summary>
    /// アクティブなパーティクルの数を取得（デバッグ機能）
    /// </summary>
    /// <returns></returns>
    public virtual int GetActiveParticleNum() {
        particleActiveCountBuffer.GetData(debugounts);
        return debugounts[1];
    }

    public virtual ComputeBuffer GetParticleBuffer() { return particleBuffer; }
    public virtual ComputeBuffer GetActiveParticleBuffer() { return particleActiveBuffer; }

    public virtual ComputeBuffer GetParticleCountBuffer() { return particleActiveCountBuffer; }
    //public virtual ComputeBuffer GetActiveCountBuffer() { return particleActiveCountBuffer; }

    public virtual void SetVertexCount(int vertexNum)
    {
        particleCounts[0] = vertexNum;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Initialize()
    {
        particleNum = (particleMax / THREAD_NUM_X) * THREAD_NUM_X;
        emitNum = (emitMax / THREAD_NUM_X) * THREAD_NUM_X;
        //Debug.Log("particleNum " + particleNum + " emitNum " + emitNum + " THREAD_NUM_X " + THREAD_NUM_X);

        particleBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(T)), ComputeBufferType.Default);
        particleActiveBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        particleActiveBuffer.SetCounterValue(0);
        particlePoolBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        particlePoolBuffer.SetCounterValue(0);
        particleActiveCountBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);
        particlePoolCountBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);
        particlePoolCountBuffer.SetData(particleCounts);
        
        initKernel = cs.FindKernel("Init");
        emitKernel = cs.FindKernel("Emit");
        updateKernel = cs.FindKernel("Update");

        cspropid_Particles = ShaderDefines.GetBufferPropertyID(ShaderDefines.BufferID._Particles);
        cspropid_DeadList = ShaderDefines.GetBufferPropertyID(ShaderDefines.BufferID._DeadList);
        cspropid_ActiveList = ShaderDefines.GetBufferPropertyID(ShaderDefines.BufferID._ActiveList);
        cspropid_ParticlePool = ShaderDefines.GetBufferPropertyID(ShaderDefines.BufferID._ParticlePool);
        cspropid_EmitNum = ShaderDefines.GetIntPropertyID(ShaderDefines.IntID._EmitNum);

        //Debug.Log("initKernel " + initKernel + " emitKernel " + emitKernel + " updateKernel " + updateKernel);

        cs.SetBuffer(initKernel, cspropid_Particles, particleBuffer);
        cs.SetBuffer(initKernel, cspropid_DeadList, particlePoolBuffer);
        cs.Dispatch(initKernel, particleNum / THREAD_NUM_X, 1, 1);

		isInitialized = true;
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
        }

        if (particlePoolBuffer != null)
        {
            particlePoolBuffer.Release();
        }

        if (particleBuffer != null)
        {
            particleBuffer.Release();
        }

        if(particlePoolCountBuffer != null)
        {
            particlePoolCountBuffer.Release();
        }

        if(particleActiveCountBuffer != null)
        {
            particleActiveCountBuffer.Release();
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
