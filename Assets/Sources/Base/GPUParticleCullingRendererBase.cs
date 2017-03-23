using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

public abstract class GPUParticleCullingRendererBase <T> : GPUParticleRendererBase<T> where T : struct {

    #region define
    public class CullingData
    {
        public ComputeBuffer inViewsAppendBuffer;
        public ComputeBuffer inViewsCountBuffer;
        public int inViewsNum;

        public int[] inViewsCounts = { 0, 1, 0, 0 };

        public CullingData(int particleNum)
        {
            inViewsAppendBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(int)), ComputeBufferType.Append);
            inViewsAppendBuffer.SetCounterValue(0);
            inViewsCountBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(int)), ComputeBufferType.IndirectArguments);
            inViewsCountBuffer.SetData(inViewsCounts);
        }

        public void Release()
        {
            if(inViewsAppendBuffer != null)
            {
                inViewsAppendBuffer.Release();
                inViewsAppendBuffer = null;
            }
            if(inViewsCountBuffer != null)
            {
                inViewsCountBuffer.Release();
                inViewsCountBuffer = null;
            }
        }

        const int NUM_THREAD_X = 32;

        public void Update(ComputeShader cs, Camera camera, int particleNum, ComputeBuffer particleBuffer, ComputeBuffer activeList)
        {
            int kernel = cs.FindKernel("CheckCameraCulling");
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            List<Vector3> normals = planes.Take(4).Select(p => p.normal).ToList();
            planes.Take(4).ToList().ForEach(plane => Debug.DrawRay(camera.transform.position, plane.normal * 10f));
            float[] normalsFloat = Enumerable.Range(0, 3).SelectMany(i => normals.Select(n => n[i])).ToArray(); // row major -> column major

            inViewsAppendBuffer.SetCounterValue(0);

            var cPos = camera.transform.position;
            cs.SetFloats("_CameraPos", cPos.x, cPos.y, cPos.z);

            cs.SetInt("_ParticleNum", particleNum);
            cs.SetFloats("_CameraFrustumNormals", normalsFloat);
            cs.SetBuffer(kernel, "_InViewAppend", inViewsAppendBuffer);
            cs.SetBuffer(kernel, "_ParticleBuffer", particleBuffer);
            cs.SetBuffer(kernel, "_ParticleActiveList", activeList);
            cs.Dispatch(kernel, Mathf.CeilToInt((float)activeList.count / NUM_THREAD_X), 1, 1);

            inViewsCountBuffer.SetData(inViewsCounts);
            ComputeBuffer.CopyCount(inViewsAppendBuffer, inViewsCountBuffer, 0);
            inViewsCountBuffer.GetData(inViewsCounts);
            inViewsNum = inViewsCounts[0];
        }
    }
    #endregion

    

    #region public
    public ComputeShader cullingCS;
    public bool isCulling = true;
    #endregion

    #region protected
    protected Dictionary<Camera, CullingData> cameraDatas = new Dictionary<Camera, CullingData>();

    void UpdateVertexBuffer(Camera camera)
    {
        
        CullingData data = cameraDatas[camera];
        if (data == null)
        {
            data = cameraDatas[camera] = new CullingData(particleNum);
        }
        
        data.Update(cullingCS, camera, particleNum, particleBuffer, activeIndexBuffer);
    }
    #endregion

    protected override void OnRenderObjectInternal()
    {
        var cam = Camera.current;
        if (isCulling)
        {
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

                    Graphics.DrawProceduralIndirect(MeshTopology.Points, data.inViewsCountBuffer);

                }
            }
        }
        else
        {
            base.OnRenderObjectInternal();
        }
    }

    protected void LateUpdate()
    {
        if (isCulling)
        {
            cameraDatas.Keys.Where(cam => cam == null).ToList().ForEach(cam => cameraDatas.Remove(cam));
            cameraDatas.Keys
                .Where(cam => cam.isActiveAndEnabled)
                .ToList().ForEach(cam =>
                {
                    UpdateVertexBuffer(cam);
                });
        }
    }

    protected virtual void ReleaseBuffer()
    {
        cameraDatas.Values.Where(d => d != null).ToList().ForEach(d => d.Release());
        cameraDatas.Clear();
    }

    private void OnDestroy()
    {
        ReleaseBuffer();
    }
}
