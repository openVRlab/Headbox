using System.Collections.Generic;
using System.Collections;
using System;

namespace UnityEngine.Animations.Rigging
{
    // Since we cannot animate arrays, we enforce a hard limit of 8 weighted transform
    // that can be used and animated for a single constraint.
    [System.Serializable]
    public struct WeightedTransformArray : IList<WeightedTransform>, IList
    {
        public static readonly int k_MaxLength = 8;

        [SerializeField, NotKeyable] private int m_Length;

        [SerializeField] private WeightedTransform m_Item0;
        [SerializeField] private WeightedTransform m_Item1;
        [SerializeField] private WeightedTransform m_Item2;
        [SerializeField] private WeightedTransform m_Item3;
        [SerializeField] private WeightedTransform m_Item4;
        [SerializeField] private WeightedTransform m_Item5;
        [SerializeField] private WeightedTransform m_Item6;
        [SerializeField] private WeightedTransform m_Item7;

        public WeightedTransformArray(int size)
        {
            m_Length = ClampSize(size);
            m_Item0 = new WeightedTransform();
            m_Item1 = new WeightedTransform();
            m_Item2 = new WeightedTransform();
            m_Item3 = new WeightedTransform();
            m_Item4 = new WeightedTransform();
            m_Item5 = new WeightedTransform();
            m_Item6 = new WeightedTransform();
            m_Item7 = new WeightedTransform();
        }

        public IEnumerator<WeightedTransform> GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(ref this);
        }

        int IList.Add(object value)
        {
            Add((WeightedTransform)value);
            return m_Length - 1;
        }

        public void Add(WeightedTransform value)
        {
            if (m_Length >= k_MaxLength)
                throw new ArgumentException($"This array cannot have more than '{k_MaxLength}' items.");

            Set(m_Length, value);

            ++m_Length;
        }

        public void Clear()
        {
            m_Length = 0;
        }

        int IList.IndexOf(object value) => IndexOf((WeightedTransform)value);

        public int IndexOf(WeightedTransform value)
        {
            for (int i = 0; i < m_Length; ++i)
            {
                if (Get(i).Equals(value))
                    return i;
            }

            return -1;
        }

        bool IList.Contains(object value) => Contains((WeightedTransform)value);

        public bool Contains(WeightedTransform value)
        {
            for (int i = 0; i < m_Length; ++i)
            {
                if (Get(i).Equals(value))
                    return true;
            }

            return false;
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < m_Length; i++)
            {
                array.SetValue(Get(i), i + arrayIndex);
            }
        }

        public void CopyTo(WeightedTransform[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("The starting array index cannot be negative.");
            if (Count > array.Length - arrayIndex + 1)
                throw new ArgumentException("The destination array has fewer elements than the collection.");

            for (int i = 0; i < m_Length; i++)
            {
                array[i + arrayIndex] = Get(i);
            }
        }

        void IList.Remove(object value) { Remove((WeightedTransform)value); }

        public bool Remove(WeightedTransform value)
        {
            for (int i = 0; i < m_Length; ++i)
            {
                if (Get(i).Equals(value))
                {
                    for (; i < m_Length - 1; ++i)
                    {
                        Set(i, Get(i + 1));
                    }

                    --m_Length;
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            CheckOutOfRangeIndex(index);

            for (int i = index; i < m_Length - 1; ++i)
            {
                Set(i, Get(i + 1));
            }

            --m_Length;
        }

        void IList.Insert(int index, object value) => Insert(index, (WeightedTransform)value);

        public void Insert(int index, WeightedTransform value)
        {
            if (m_Length >= k_MaxLength)
                throw new ArgumentException($"This array cannot have more than '{k_MaxLength}' items.");

            CheckOutOfRangeIndex(index);

            if (index >= m_Length)
            {
                Add(value);
                return;
            }

            for (int i = m_Length; i > index; --i)
            {
                Set(i, Get(i - 1));
            }

            Set(index, value);
            ++m_Length;
        }

        private static int ClampSize(int size)
        {
            return Mathf.Clamp(size, 0, k_MaxLength);
        }

        private void CheckOutOfRangeIndex(int index)
        {
            if (index < 0 || index >= k_MaxLength)
                throw new IndexOutOfRangeException($"Index {index} is out of range of '{m_Length}' Length.");
        }

        private WeightedTransform Get(int index)
        {
            CheckOutOfRangeIndex(index);

            switch(index)
            {
                case 0: return m_Item0;
                case 1: return m_Item1;
                case 2: return m_Item2;
                case 3: return m_Item3;
                case 4: return m_Item4;
                case 5: return m_Item5;
                case 6: return m_Item6;
                case 7: return m_Item7;
            }

            // Shouldn't happen.
            return m_Item0;
        }

        private void Set(int index, WeightedTransform value)
        {
            CheckOutOfRangeIndex(index);

            switch(index)
            {
                case 0: m_Item0 = value; break;
                case 1: m_Item1 = value; break;
                case 2: m_Item2 = value; break;
                case 3: m_Item3 = value; break;
                case 4: m_Item4 = value; break;
                case 5: m_Item5 = value; break;
                case 6: m_Item6 = value; break;
                case 7: m_Item7 = value; break;
            }
        }

        public void SetWeight(int index, float weight)
        {
            var weightedTransform = Get(index);
            weightedTransform.weight = weight;

            Set(index, weightedTransform);
        }

        public float GetWeight(int index)
        {
            return Get(index).weight;
        }

        public void SetTransform(int index, Transform transform)
        {
            var weightedTransform = Get(index);
            weightedTransform.transform = transform;

            Set(index, weightedTransform);
        }

        public Transform GetTransform(int index)
        {
            return Get(index).transform;
        }

        object IList.this[int index] { get => (object)Get(index); set => Set(index, (WeightedTransform)value); }

        public WeightedTransform this[int index] { get => Get(index); set => Set(index, value); }

        public int Count { get => m_Length; }

        public bool IsReadOnly { get => false; }

        public bool IsFixedSize { get => false; }

        bool ICollection.IsSynchronized { get => true; }

        object ICollection.SyncRoot { get => null; }

        [System.Serializable]
        public struct Enumerator : IEnumerator<WeightedTransform>
        {
            private WeightedTransformArray m_Array;
            private int m_Index;

            public Enumerator(ref WeightedTransformArray array)
            {
                m_Array = array;
                m_Index = -1;
            }

            public bool MoveNext()
            {
                m_Index++;
                return (m_Index < m_Array.Count);
            }

            public void Reset()
            {
                m_Index = -1;
            }

            void IDisposable.Dispose() { }

            public WeightedTransform Current => m_Array.Get(m_Index);

            object IEnumerator.Current => Current;
        }
    }
}

