using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Animations.Rigging
{
    public struct CacheIndex
    {
        internal int idx;
    }

    public struct AnimationJobCache : System.IDisposable
    {
        NativeArray<float> m_Data;

        public AnimationJobCache(float[] data)
        {
            m_Data = new NativeArray<float>(data, Allocator.Persistent);
        }

        public void Dispose()
        {
            m_Data.Dispose();
        }

        public float GetRaw(CacheIndex index, int offset = 0)
        {
            return m_Data[index.idx + offset];
        }

        public void SetRaw(float val, CacheIndex index, int offset = 0)
        {
            m_Data[index.idx + offset] = val;
        }

        unsafe public T Get<T>(CacheIndex index, int offset = 0) where T : unmanaged
        {
            int size = UnsafeUtility.SizeOf<T>();
            int stride = size / UnsafeUtility.SizeOf<float>();

            T val = default(T);
            UnsafeUtility.MemCpy(&val, (float*)m_Data.GetUnsafeReadOnlyPtr() + index.idx + offset * stride, size);
            return val;
        }

        unsafe public void Set<T>(T val, CacheIndex index, int offset = 0) where T : unmanaged
        {
            int size = UnsafeUtility.SizeOf<T>();
            int stride = size / UnsafeUtility.SizeOf<float>();

            UnsafeUtility.MemCpy((float*)m_Data.GetUnsafePtr() + index.idx + offset * stride, &val, size);
        }

        unsafe public void SetArray<T>(T[] v, CacheIndex index, int offset = 0) where T : unmanaged
        {
            int size = UnsafeUtility.SizeOf<T>();
            int stride = size / UnsafeUtility.SizeOf<float>();

            fixed (void* ptr = v)
            {
                UnsafeUtility.MemCpy((float*)m_Data.GetUnsafePtr() + index.idx + offset * stride, ptr, size * v.Length);
            }
        }
    }

    public class AnimationJobCacheBuilder
    {
        List<float> m_Data;

        public AnimationJobCacheBuilder()
        {
            m_Data = new List<float>();
        }

        public CacheIndex Add(float v)
        {
            m_Data.Add(v);
            return new CacheIndex { idx = m_Data.Count - 1 };
        }

        public CacheIndex Add(Vector2 v)
        {
            m_Data.Add(v.x);
            m_Data.Add(v.y);
            return new CacheIndex { idx = m_Data.Count - 2 };
        }

        public CacheIndex Add(Vector3 v)
        {
            m_Data.Add(v.x);
            m_Data.Add(v.y);
            m_Data.Add(v.z);
            return new CacheIndex { idx = m_Data.Count - 3 };
        }

        public CacheIndex Add(Vector4 v)
        {
            m_Data.Add(v.x);
            m_Data.Add(v.y);
            m_Data.Add(v.z);
            m_Data.Add(v.w);
            return new CacheIndex { idx = m_Data.Count - 4 };
        }

        public CacheIndex Add(Quaternion v)
        {
            return Add(new Vector4(v.x, v.y, v.z, v.w));
        }

        public CacheIndex Add(AffineTransform tx)
        {
            Add(tx.translation);
            Add(tx.rotation);
            return new CacheIndex { idx = m_Data.Count - 7 };
        }

        public CacheIndex AllocateChunk(int size)
        {
            m_Data.AddRange(new float[size]);
            return new CacheIndex { idx = m_Data.Count - size };
        }

        public void SetValue(CacheIndex index, int offset, float value)
        {
            if (index.idx + offset < m_Data.Count)
                m_Data[index.idx + offset] = value;
        }

        public AnimationJobCache Build() => new AnimationJobCache(m_Data.ToArray());
    }
}
