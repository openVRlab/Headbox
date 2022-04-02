namespace UnityEngine.Animations.Rigging
{
    [System.Serializable]
    public struct Vector3Bool
    {
        public bool x, y, z;

        public Vector3Bool(bool val)
        {
            x = y = z = val;
        }

        public Vector3Bool(bool x, bool y, bool z)
        {
            this.x = x; this.y = y; this.z = z;
        }
    }
}
