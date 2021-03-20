using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class MathUtils
{
    public static Quaternion RotateSlerp(Transform transform, Vector3 direction, float turnSpeed)
    {
        if (direction.magnitude <= 0)
            return transform.rotation;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Vector3 finalRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed).eulerAngles;
        return Quaternion.Euler(0f, finalRotation.y, 0f);
    }

    public static bool PointInCone(Vector3 origin, Vector3 facing, float coneWidth, float radius, Vector3 point)
    {
        // Check it is within a circle
        if (!PointInCircle(origin, radius, point))
            return false;

        // Check to see the angle required to turn the facing towards the target is less than our conewidth.
        facing.Normalize();
        var direction = (point - origin).normalized;

        float angle = Mathf.Acos(Vector3.Dot(facing, direction));

        if (Mathf.Abs(angle) < coneWidth / 2)
            return true;
        return false;
    }

    public static bool PointInCircle(Vector3 origin, float radius, Vector3 point)
    {
        // This tries to avoid doing a square root by checking it as a bounding box
        var dx = Mathf.Abs(point.x - origin.x);
        var dy = Mathf.Abs(point.z - origin.z);
        if (dx > radius)
            return false;
        if (dy > radius)
            return false;

        // Then as a diamond shape
        if (dx + dy <= radius)
            return true;

        // Ayy
        if (Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2) <= Mathf.Pow(radius, 2))
            return true;
        return false;
    }
}