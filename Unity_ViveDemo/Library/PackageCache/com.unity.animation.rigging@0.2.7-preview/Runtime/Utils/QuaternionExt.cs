namespace UnityEngine.Animations.Rigging
{
    public static class QuaternionExt
    {
        const float k_FloatMin = 1e-10f;

        public static readonly Quaternion zero = new Quaternion(0f, 0f, 0f, 0f);

        public static Quaternion FromToRotation(Vector3 from, Vector3 to)
        {
            float theta = Vector3.Dot(from.normalized, to.normalized);
            if (theta >= 1f)
                return Quaternion.identity;

            if (theta <= -1f)
            {
                Vector3 axis = Vector3.Cross(from, Vector3.right);
                if (axis.sqrMagnitude == 0f)
                    axis = Vector3.Cross(from, Vector3.up);

                return Quaternion.AngleAxis(180f, axis);
            }

            return Quaternion.AngleAxis(Mathf.Acos(theta) * Mathf.Rad2Deg, Vector3.Cross(from, to).normalized);
        }

        public static Quaternion Add(Quaternion rhs, Quaternion lhs)
        {
            float sign = Mathf.Sign(Quaternion.Dot(rhs, lhs));
            return new Quaternion(rhs.x + sign * lhs.x, rhs.y + sign * lhs.y, rhs.z + sign * lhs.z, rhs.w + sign * lhs.w);
        }

        public static Quaternion Scale(Quaternion q, float scale)
        {
            return new Quaternion(q.x * scale, q.y * scale, q.z * scale, q.w * scale);
        }

        public static Quaternion NormalizeSafe(Quaternion q)
        {
            float dot = Quaternion.Dot(q, q);
            if (dot > k_FloatMin)
            {
                float rsqrt = 1.0f / Mathf.Sqrt(dot);
                return new Quaternion(q.x * rsqrt, q.y * rsqrt, q.z * rsqrt, q.w * rsqrt);
            }

            return Quaternion.identity;
        }
    }
}
