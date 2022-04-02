using Unity.Collections;

namespace UnityEngine.Animations.Rigging
{
    using Experimental.Animations;

    public static class AnimationRuntimeUtils
    {
        const float k_SqrEpsilon = 1e-8f;

        public static void SolveTwoBoneIK(
            AnimationStream stream,
            ReadWriteTransformHandle root,
            ReadWriteTransformHandle mid,
            ReadWriteTransformHandle tip,
            ReadOnlyTransformHandle target,
            ReadOnlyTransformHandle hint,
            float posWeight,
            float rotWeight,
            float hintWeight,
            Vector2 limbLengths,
            AffineTransform targetOffset
            )
        {
            Vector3 aPosition = root.GetPosition(stream);
            Vector3 bPosition = mid.GetPosition(stream);
            Vector3 cPosition = tip.GetPosition(stream);
            target.GetGlobalTR(stream, out Vector3 targetPos, out Quaternion targetRot);
            Vector3 tPosition = Vector3.Lerp(cPosition, targetPos + targetOffset.translation, posWeight);
            Quaternion tRotation = Quaternion.Lerp(tip.GetRotation(stream), targetRot * targetOffset.rotation, rotWeight);
            bool hasHint = hint.IsValid(stream) && hintWeight > 0f;

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 at = tPosition - aPosition;

            float oldAbcAngle = TriangleAngle(ac.magnitude, limbLengths[0], limbLengths[1]);
            float newAbcAngle = TriangleAngle(at.magnitude, limbLengths[0], limbLengths[1]);

            // Bend normal strategy is to take whatever has been provided in the animation
            // stream to minimize configuration changes, however if this is collinear
            // try computing a bend normal given the desired target position.
            // If this also fails, try resolving axis using hint if provided.
            Vector3 axis = Vector3.Cross(ab, bc);
            if (axis.sqrMagnitude < k_SqrEpsilon)
            {
                axis = hasHint ? Vector3.Cross(hint.GetPosition(stream) - aPosition, bc) : Vector3.zero;

                if (axis.sqrMagnitude < k_SqrEpsilon)
                    axis = Vector3.Cross(at, bc);

                if (axis.sqrMagnitude < k_SqrEpsilon)
                    axis = Vector3.up;
            }
            axis = Vector3.Normalize(axis);

            float a = 0.5f * (oldAbcAngle - newAbcAngle);
            float sin = Mathf.Sin(a);
            float cos = Mathf.Cos(a);
            Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
            mid.SetRotation(stream, deltaR * mid.GetRotation(stream));

            cPosition = tip.GetPosition(stream);
            ac = cPosition - aPosition;
            root.SetRotation(stream, QuaternionExt.FromToRotation(ac, at) * root.GetRotation(stream));

            if (hasHint)
            {
                float acSqrMag = ac.sqrMagnitude;
                if (acSqrMag > 0f)
                {
                    bPosition = mid.GetPosition(stream);
                    cPosition = tip.GetPosition(stream);
                    ab = bPosition - aPosition;
                    ac = cPosition - aPosition;

                    Vector3 acNorm = ac / Mathf.Sqrt(acSqrMag);
                    Vector3 ah = hint.GetPosition(stream) - aPosition;
                    Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                    Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                    float maxReach = limbLengths[0] + limbLengths[1];
                    if (abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f)
                    {
                        Quaternion hintR = QuaternionExt.FromToRotation(abProj, ahProj);
                        hintR.x *= hintWeight;
                        hintR.y *= hintWeight;
                        hintR.z *= hintWeight;
                        root.SetRotation(stream, hintR * root.GetRotation(stream));
                    }
                }
            }

            tip.SetRotation(stream, tRotation);
        }

        static float TriangleAngle(float aLen, float aLen1, float aLen2)
        {
            float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }

        // Implementation of unconstrained FABRIK solver : Forward and Backward Reaching Inverse Kinematic
        // Aristidou A, Lasenby J. FABRIK: a fast, iterative solver for the inverse kinematics problem. Graphical Models 2011; 73(5): 243–260.
        public static bool SolveFABRIK(
            ref NativeArray<Vector3> linkPositions,
            ref NativeArray<float> linkLengths,
            Vector3 target,
            float tolerance,
            float maxReach,
            int maxIterations
            )
        {
            // If the target is unreachable
            var rootToTargetDir = target - linkPositions[0];
            if (rootToTargetDir.sqrMagnitude > Square(maxReach))
            {
                // Line up chain towards target
                var dir = rootToTargetDir.normalized;
                for (int i = 1; i < linkPositions.Length; ++i)
                    linkPositions[i] = linkPositions[i - 1] + dir * linkLengths[i - 1];

                return true;
            }
            else
            {
                int tipIndex = linkPositions.Length - 1;
                float sqrTolerance = Square(tolerance);
                if (SqrDistance(linkPositions[tipIndex], target) > sqrTolerance)
                {
                    var rootPos = linkPositions[0];
                    int iteration = 0;

                    do
                    {
                        // Forward reaching phase
                        // Set tip to target and propagate displacement to rest of chain
                        linkPositions[tipIndex] = target;
                        for (int i = tipIndex - 1; i > -1; --i)
                            linkPositions[i] = linkPositions[i + 1] + ((linkPositions[i] - linkPositions[i + 1]).normalized * linkLengths[i]);

                        // Backward reaching phase
                        // Set root back at it's original position and propagate displacement to rest of chain
                        linkPositions[0] = rootPos;
                        for (int i = 1; i < linkPositions.Length; ++i)
                            linkPositions[i] = linkPositions[i - 1] + ((linkPositions[i] - linkPositions[i - 1]).normalized * linkLengths[i - 1]);
                    }
                    while ((SqrDistance(linkPositions[tipIndex], target) > sqrTolerance) && (++iteration < maxIterations));

                    return true;
                }
            }

            return false;
        }

        public static float SqrDistance(Vector3 p0, Vector3 p1)
        {
            return (p1 - p0).sqrMagnitude;
        }

        public static float Square(float value)
        {
            return value * value;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 t)
        {
            return Vector3.Scale(a, Vector3.one - t) + Vector3.Scale(b, t);
        }

        public static float Select(float a, float b, float c)
        {
            return (c > 0f) ? b : a;
        }

        public static Vector3 Select(Vector3 a, Vector3 b, Vector3 c)
        {
            return new Vector3(Select(a.x, b.x, c.x), Select(a.y, b.y, c.y), Select(a.z, b.z, c.z));
        }

        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        {
            float sqrMag = Vector3.Dot(planeNormal, planeNormal);
            var dot = Vector3.Dot(vector, planeNormal);
            return new Vector3(vector.x - planeNormal.x * dot / sqrMag,
                    vector.y - planeNormal.y * dot / sqrMag,
                    vector.z - planeNormal.z * dot / sqrMag);
        }

        public static float Sum(AnimationJobCache cache, CacheIndex index, int count)
        {
            if (count == 0)
                return 0f;

            float sum = 0f;
            for (int i = 0; i < count; ++i)
                sum += cache.GetRaw(index, i);

            return sum;
        }

        public static float Sum(NativeArray<float> floatBuffer)
        {
            if (floatBuffer.Length == 0)
                return 0f;

            float sum = 0f;
            for (int i = 0; i< floatBuffer.Length; ++i)
            {
                sum += floatBuffer[i];
            }

            return sum;
        }

        public static void PassThrough(AnimationStream stream, ReadWriteTransformHandle handle)
        {
            handle.GetLocalTRS(stream, out Vector3 position, out Quaternion rotation, out Vector3 scale);
            handle.SetLocalTRS(stream, position, rotation, scale);
        }
    }
}
