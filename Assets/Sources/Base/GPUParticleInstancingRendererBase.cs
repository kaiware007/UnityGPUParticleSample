using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Runtime.InteropServices;

public class GPUParticleInstancingRendererBase<T> : GPUParticleCullingRendererBase<T> where T : struct
{

    #region　DEFINE
    struct VertexData
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector2 uv;
        public Vector4 tangent;
    }
    #endregion

    public Mesh mesh;
    public Vector3 rotationOffsetAxis = Vector3.right;
    public float rotationOffsetAngle = 0;

    // メッシュデータ
    protected ComputeBuffer meshIndicesBuffer;
    protected ComputeBuffer meshVertexBuffer;
    protected int meshIndicesNum;

    /// <summary>
    /// メッシュデータの頂点データなどをコンピュートバッファをコピー
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="vertexBuffer"></param>
    /// <param name="indicesBuffer"></param>
    /// <param name="indicesNum"></param>
    void InitMeshDataBuffer(Mesh mesh, out ComputeBuffer vertexBuffer, out ComputeBuffer indicesBuffer, out int indicesNum)
    {
        Debug.Log("Mesh " + mesh.name);
        Debug.Log("Vertex " + mesh.vertexCount);
        Debug.Log("Normal " + mesh.normals.Length);
        Debug.Log("UV " + mesh.uv.Length);
        Debug.Log("TANGENTS " + mesh.tangents.Length);

        var indices = mesh.GetIndices(0);
        var vertexDataArray = Enumerable.Range(0, mesh.vertexCount).Select(b =>
        {
            //Debug.Log("b: " + b + " / " + mesh.vertexCount);
            return new VertexData()
            {
                vertex = mesh.vertices[b],
                normal = mesh.normals[b],
                uv = mesh.uv[b],
                tangent = mesh.tangents[b],
            };
        }).ToArray();

        indicesNum = indices.Length;
        indicesBuffer = new ComputeBuffer(indices.Length, Marshal.SizeOf(typeof(uint)));
        vertexBuffer = new ComputeBuffer(vertexDataArray.Length, Marshal.SizeOf(typeof(VertexData)));
        indicesBuffer.SetData(indices);
        vertexBuffer.SetData(vertexDataArray);
    }

    protected override void Start()
    {
        base.Start();

        InitMeshDataBuffer(mesh, out meshVertexBuffer, out meshIndicesBuffer, out meshIndicesNum);
    }

    protected override void SetMaterialParam()
    {
        material.SetBuffer("_vertex", meshVertexBuffer);
        material.SetBuffer("_indices", meshIndicesBuffer);
        material.SetVector("_RotationOffsetAxis", new Vector4(rotationOffsetAxis.x, rotationOffsetAxis.y, rotationOffsetAxis.z, rotationOffsetAngle * Mathf.Deg2Rad));

        material.SetBuffer("_Particles", particleBuffer);
        material.SetBuffer("_ParticleActiveList", activeIndexBuffer);

        material.SetPass(0);
    }

    protected override void OnRenderObjectInternal()
    {
        if ((Camera.current.cullingMask & (1 << gameObject.layer)) == 0)
            return;

        if (isCulling)
        {
            var cam = Camera.current;

            if (!cameraDatas.ContainsKey(cam))
            {
                cameraDatas[cam] = null; // このフレームは登録だけ
            }
            else
            {
                var data = cameraDatas[cam];
                if (data != null)
                {
                    SetMaterialParam();

                    material.EnableKeyword("GPUPARTICLE_CULLING_ON");

                    material.SetBuffer("_InViewsList", data.inViewsAppendBuffer);

                    Graphics.DrawProcedural(MeshTopology.Triangles, meshIndicesNum, data.inViewsNum);   // 視界範囲内のものだけ描画
                }
            }
        }
        else
        {
            SetMaterialParam();

            material.DisableKeyword("GPUPARTICLE_CULLING_ON");

            Graphics.DrawProcedural(MeshTopology.Triangles, meshIndicesNum, particle.GetActiveParticleNum());   // Activeなものをすべて描画
        }
    }

    protected override void ReleaseBuffer()
    {
        base.ReleaseBuffer();

        if (meshIndicesBuffer != null)
        {
            meshIndicesBuffer.Release();
            meshIndicesBuffer = null;
        }
        if (meshVertexBuffer != null)
        {
            meshVertexBuffer.Release();
            meshVertexBuffer = null;
        }
    }
}
