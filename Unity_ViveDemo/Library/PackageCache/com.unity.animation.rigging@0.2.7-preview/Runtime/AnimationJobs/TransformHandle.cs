namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    public struct ReadWriteTransformHandle
    {
        TransformStreamHandle m_Handle;

        public Vector3 GetLocalPosition(AnimationStream stream) => m_Handle.GetLocalPosition(stream);
        public Quaternion GetLocalRotation(AnimationStream stream) => m_Handle.GetLocalRotation(stream);
        public Vector3 GetLocalScale(AnimationStream stream) => m_Handle.GetLocalScale(stream);
        public void GetLocalTRS(AnimationStream stream, out Vector3 position, out Quaternion rotation, out Vector3 scale) =>
            m_Handle.GetLocalTRS(stream, out position, out rotation, out scale);

        public void SetLocalPosition(AnimationStream stream, Vector3 position) => m_Handle.SetLocalPosition(stream, position);
        public void SetLocalRotation(AnimationStream stream, Quaternion rotation) => m_Handle.SetLocalRotation(stream, rotation);
        public void SetLocalScale(AnimationStream stream, Vector3 scale) => m_Handle.SetLocalScale(stream, scale);
        public void SetLocalTRS(AnimationStream stream, Vector3 position, Quaternion rotation, Vector3 scale, bool useMask = false) =>
            m_Handle.SetLocalTRS(stream, position, rotation, scale, useMask);

        public Vector3 GetPosition(AnimationStream stream) => m_Handle.GetPosition(stream);
        public Quaternion GetRotation(AnimationStream stream) => m_Handle.GetRotation(stream);
        public void GetGlobalTR(AnimationStream stream, out Vector3 position, out Quaternion rotation) =>
            m_Handle.GetGlobalTR(stream, out position, out rotation);

        public void SetPosition(AnimationStream stream, Vector3 position) => m_Handle.SetPosition(stream, position);
        public void SetRotation(AnimationStream stream, Quaternion rotation) => m_Handle.SetRotation(stream, rotation);
        public void SetGlobalTR(AnimationStream stream, Vector3 position, Quaternion rotation, bool useMask = false) =>
            m_Handle.SetGlobalTR(stream, position, rotation, useMask);

        public bool IsResolved(AnimationStream stream) => m_Handle.IsResolved(stream);
        public bool IsValid(AnimationStream stream) => m_Handle.IsValid(stream);
        public void Resolve(AnimationStream stream) => m_Handle.Resolve(stream);

        public static ReadWriteTransformHandle Bind(Animator animator, Transform transform)
        {
            ReadWriteTransformHandle handle = new ReadWriteTransformHandle();
            if (transform == null || !transform.IsChildOf(animator.transform))
                return handle;

            handle.m_Handle = animator.BindStreamTransform(transform);
            return handle;
        }
    }

    public struct ReadOnlyTransformHandle
    {
        TransformStreamHandle m_StreamHandle;
        TransformSceneHandle m_SceneHandle;
        byte m_InStream;

        public Vector3 GetLocalPosition(AnimationStream stream) =>
            m_InStream == 1 ? m_StreamHandle.GetLocalPosition(stream) : m_SceneHandle.GetLocalPosition(stream);

        public Quaternion GetLocalRotation(AnimationStream stream) =>
            m_InStream == 1 ? m_StreamHandle.GetLocalRotation(stream) : m_SceneHandle.GetLocalRotation(stream);

        public Vector3 GetLocalScale(AnimationStream stream) =>
            m_InStream == 1 ? m_StreamHandle.GetLocalScale(stream) : m_SceneHandle.GetLocalScale(stream);

        public void GetLocalTRS(AnimationStream stream, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            if (m_InStream == 1)
                m_StreamHandle.GetLocalTRS(stream, out position, out rotation, out scale);
            else
                m_SceneHandle.GetLocalTRS(stream, out position, out rotation, out scale);
        }

        public Vector3 GetPosition(AnimationStream stream) =>
            m_InStream == 1 ? m_StreamHandle.GetPosition(stream) : m_SceneHandle.GetPosition(stream);

        public Quaternion GetRotation(AnimationStream stream) =>
            m_InStream == 1 ? m_StreamHandle.GetRotation(stream) : m_SceneHandle.GetRotation(stream);

        public void GetGlobalTR(AnimationStream stream, out Vector3 position, out Quaternion rotation)
        {
            if (m_InStream == 1)
                m_StreamHandle.GetGlobalTR(stream, out position, out rotation);
            else
                m_SceneHandle.GetGlobalTR(stream, out position, out rotation);
        }

        public bool IsResolved(AnimationStream stream) =>
            m_InStream == 1 ? m_StreamHandle.IsResolved(stream) : true;

        public bool IsValid(AnimationStream stream) =>
            m_InStream == 1 ? m_StreamHandle.IsValid(stream) : m_SceneHandle.IsValid(stream);

        public void Resolve(AnimationStream stream)
        {
            if (m_InStream == 1)
                m_StreamHandle.Resolve(stream);
        }

        public static ReadOnlyTransformHandle Bind(Animator animator, Transform transform)
        {
            ReadOnlyTransformHandle handle = new ReadOnlyTransformHandle();
            if (transform == null)
                return handle;

            handle.m_InStream = (byte)(transform.IsChildOf(animator.transform) ? 1 : 0);
            if (handle.m_InStream == 1)
                handle.m_StreamHandle = animator.BindStreamTransform(transform);
            else
                handle.m_SceneHandle = animator.BindSceneTransform(transform);

            return handle;
        }
    }
}