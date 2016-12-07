using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class GPUParticle : MonoBehaviour {

    #region define
    struct ParticleData
    {
        public bool isActive;       // 有効フラグ
        public Vector3 position;    // 座標
        public Vector3 velocity;    // 加速度
        public Color color;         // 色
        public float duration;      // 生存時間
        public float scale;         // サイズ
    }

    // ComputeShaderのスレッド数
    const int THREAD_NUM_X = 16;
    #endregion

    #region public
    public int particleMax = 1024;
    public int emitMax = 1024;
    public float velocityMax = 1000f;
    public float lifeTime = 1;
    public float scaleMin = 1;
    public float scaleMax = 2;
    public float gravity = 9.8f;

    [Range(0,1)]
    public float sai = 1;   // 彩度
    [Range(0,1)]
    public float val = 1;   // 明るさ

    public ComputeShader cs;
    public Material material;
    public Camera camera;
    #endregion

    #region private
    ComputeBuffer particleBuffer;
    ComputeBuffer particlePoolBuffer;
    ComputeBuffer particleCountBuffer;
    int particleNum = 0;
    int emitNum = 0;
    int[] particleCounts = null;

    int initKernel = -1;
    int emitKernel = -1;
    int updateKernel = -1;

    int particlePoolNum;
    #endregion

    /// <summary>
    /// 初期化
    /// </summary>
    void Initialize()
    {
        particleNum = (particleMax / THREAD_NUM_X) * THREAD_NUM_X;
        emitNum = (emitMax / THREAD_NUM_X) * THREAD_NUM_X;
        Debug.Log("particleNum " + particleNum + " emitNum " + emitNum + " THREAD_NUM_X " + THREAD_NUM_X);

        particleBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(ParticleData)), ComputeBufferType.Default);
        particlePoolBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
        particlePoolBuffer.SetCounterValue(0);
        particleCountBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);
        particleCounts = new int[]{ 0, 1, 0, 0 };
        particleCountBuffer.SetData(particleCounts);

        initKernel = cs.FindKernel("Init");
        emitKernel = cs.FindKernel("Emit");
        updateKernel = cs.FindKernel("Update");

        Debug.Log("initKernel " + initKernel + " emitKernel " + emitKernel + " updateKernel " + updateKernel);

        InitParticle();
    }

    void InitParticle()
    {
        cs.SetBuffer(initKernel, "_Particles", particleBuffer);
        cs.SetBuffer(initKernel, "_DeadList", particlePoolBuffer);
        cs.Dispatch(initKernel, particleNum / THREAD_NUM_X, 1, 1);
    }

    /// <summary>
    /// パーティクルの更新
    /// </summary>
    void UpdateParticle()
    {
        cs.SetFloat("_DT", Time.deltaTime);
        cs.SetFloat("_LifeTime", lifeTime);
        cs.SetFloat("_Gravity", gravity);
        cs.SetBuffer(updateKernel, "_Particles", particleBuffer);
        cs.SetBuffer(updateKernel, "_DeadList", particlePoolBuffer);
        
        cs.Dispatch(updateKernel, particleNum / THREAD_NUM_X, 1, 1);
    }

    /// <summary>
    /// パーティクルの発生
    /// THREAD_NUM_X分発生
    /// </summary>
    /// <param name="position"></param>
    void EmitParticle(Vector3 position)
    {
        particleCountBuffer.SetData(particleCounts);
        ComputeBuffer.CopyCount(particlePoolBuffer, particleCountBuffer, 0);
        particleCountBuffer.GetData(particleCounts);
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

    /// <summary>
    /// ComputeBufferの解放
    /// </summary>
    void ReleaseBuffer() {
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
    
    void OnDestroy()
    {
        ReleaseBuffer();
    }

    void OnRenderObject()
    {
        material.SetBuffer("_Particles", particleBuffer);
        material.SetPass(0);

        Graphics.DrawProcedural(MeshTopology.Points, particleNum);
    }

    // Use this for initialization
    void Start () {
        Initialize();
    }
	
	// Update is called once per frame
	void Update () {
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
        GUI.Label(new Rect(10, 10, 240, 64), "Count " + particlePoolNum + "/" + particleNum);
    }

}
