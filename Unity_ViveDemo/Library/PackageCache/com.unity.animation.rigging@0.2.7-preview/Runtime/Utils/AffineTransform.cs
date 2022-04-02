namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct AffineTransform
    {
        public Vector3 translation;
        public Quaternion rotation;

        public AffineTransform(Vector3 t, Quaternion r)
        {
            translation = t;
            rotation = r;
        }

        public void Set(Vector3 t, Quaternion r)
        {
            translation = t;
            rotation = r;
        }

        public Vector3 Transform(Vector3 p) =>
            rotation * p + translation;

        public Vector3 InverseTransform(Vector3 p) =>
            Quaternion.Inverse(rotation) * (p - translation);

        public AffineTransform Inverse()
        {
            var invR = Quaternion.Inverse(rotation);
            return new AffineTransform(invR * -translation, invR);
        }

        public AffineTransform InverseMul(AffineTransform transform)
        {
            var invR = Quaternion.Inverse(rotation);
            return new AffineTransform(invR * (transform.translation - translation), invR * transform.rotation);
        }

        public static Vector3 operator *(AffineTransform lhs, Vector3 rhs) =>
            lhs.rotation * rhs + lhs.translation;

        public static AffineTransform operator *(AffineTransform lhs, AffineTransform rhs) =>
            new AffineTransform(lhs.Transform(rhs.translation), lhs.rotation * rhs.rotation);

        public static AffineTransform operator *(Quaternion lhs, AffineTransform rhs) =>
            new AffineTransform(lhs * rhs.translation, lhs * rhs.rotation);

        public static AffineTransform operator *(AffineTransform lhs, Quaternion rhs) =>
            new AffineTransform(lhs.translation, lhs.rotation * rhs);

        public static AffineTransform identity { get; } = new AffineTransform(Vector3.zero, Quaternion.identity);
    }
}
