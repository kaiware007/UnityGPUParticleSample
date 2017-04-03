using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// シェーダー定義
    /// </summary>
    public static class ShaderDefines
    {
        public enum TextureID
        {
            _MainTex,
            _MainTex2,

            _Length
        }

        public enum IntID
        {
            // GPUParticle関係
            _EmitNum,

            _Length,
        }

        public enum BoolID
        {
            _Length,
        }

        public enum FloatID
        {
            _DT,
            _Time,

            _Length,
        }

        public enum ColorID
        {
            _Color,

            _Length
        }

        public enum VectorID
        {
            _RotationOffsetAxis,
            
            _Length
        }

        public enum BufferID
        {
            // GPUParticle関係
            _Particles,
            _ParticlePool,
            _DeadList,
            _ActiveList,
            _EmitList,

            _Length
        }

        private static readonly int[] _TextureIDs;
        private static readonly int[] _intIDs;
        private static readonly int[] _floatIDs;
        private static readonly int[] _colorIDs;
        private static readonly int[] _vectorIDs;
        private static readonly int[] _boolIDs;
        private static readonly int[] _bufferIDs;

        static ShaderDefines()
        {
            _TextureIDs = new int[(int)TextureID._Length];
            for (int i = 0; i < (int)TextureID._Length; i++)
            {
                _TextureIDs[i] = Shader.PropertyToID(((TextureID)i).ToString());
            }

            _intIDs = new int[(int)IntID._Length];
            for (int i = 0; i < (int)IntID._Length; i++)
            {
                _intIDs[i] = Shader.PropertyToID(((IntID)i).ToString());
            }

            _floatIDs = new int[(int)FloatID._Length];
            for (int i = 0; i < (int)FloatID._Length; i++)
            {
                _floatIDs[i] = Shader.PropertyToID(((FloatID)i).ToString());
            }

            _colorIDs = new int[(int)ColorID._Length];
            for (int i = 0; i < (int)ColorID._Length; i++)
            {
                _colorIDs[i] = Shader.PropertyToID(((ColorID)i).ToString());
            }

            _vectorIDs = new int[(int)VectorID._Length];
            for (int i = 0; i < (int)VectorID._Length; i++)
            {
                _vectorIDs[i] = Shader.PropertyToID(((VectorID)i).ToString());
            }

            _boolIDs = new int[(int)BoolID._Length];
            for (int i = 0; i < (int)BoolID._Length; i++)
            {
                _boolIDs[i] = Shader.PropertyToID(((BoolID)i).ToString());
            }

            _bufferIDs = new int[(int)BufferID._Length];
            for (int i = 0; i < (int)BufferID._Length; i++)
            {
                _bufferIDs[i] = Shader.PropertyToID(((BufferID)i).ToString());
            }
        }

        public static void SetGlobalTexture(TextureID id, Texture value)
        {
            Shader.SetGlobalTexture(_TextureIDs[(int)id], value);
        }

        public static void SetGlobalInt(IntID id, int value)
        {
            Shader.SetGlobalInt(_intIDs[(int)id], value);
        }

        public static void SetGlobalFloat(FloatID id, float value)
        {
            Shader.SetGlobalFloat(_floatIDs[(int)id], value);
        }

        public static void SetGlobalColor(ColorID id, Color value)
        {
            Shader.SetGlobalColor(_colorIDs[(int)id], value);
        }

        public static void SetGlobalBool(BoolID id, bool value)
        {
            Shader.SetGlobalInt(_boolIDs[(int)id], value ? 1 : 0);
        }

        public static void SetGlobalVector(VectorID id, Vector4 value)
        {
            Shader.SetGlobalVector(_vectorIDs[(int)id], value);
        }

        public static void SetGlobalBuffer(BufferID id, ComputeBuffer value)
        {
            Shader.SetGlobalBuffer(_bufferIDs[(int)id], value);
        }

        public static int GetTexturePropertyID(TextureID id)
        {
            return _TextureIDs[(int)id];
        }

        public static int GetIntPropertyID(IntID id)
        {
            return _intIDs[(int)id];
        }

        public static int GetFloatPropertyID(FloatID id)
        {
            return _floatIDs[(int)id];
        }

        public static int GetColorPropertyID(ColorID id)
        {
            return _colorIDs[(int)id];
        }

        public static int GetBoolPropertyID(BoolID id)
        {
            return _boolIDs[(int)id];
        }

        public static int GetVectorPropertyID(VectorID id)
        {
            return _vectorIDs[(int)id];
        }

        public static int GetBufferPropertyID(BufferID id)
        {
            return _bufferIDs[(int)id];
        }
    }
}