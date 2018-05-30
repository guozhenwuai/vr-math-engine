using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABB
{
    public Vector3 min { get; private set; }
    public Vector3 max { get; private set; }

    public AABB()
    {
        this.min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        this.max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    }

    public AABB(AABB boundingBox)
    {
        min = boundingBox.min;
        max = boundingBox.max;
    }

    public AABB(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;
    }

    public void Enlarge(float radius)
    {
        Vector3 r = new Vector3(radius, radius, radius);
        min = min - r;
        max = max + r;
    }

    public bool Contains(Vector3 point)
    {
        return (point.x >= min.x && point.y >= min.y && point.z >= min.z)
            && (point.x <= max.x && point.y <= max.y && point.z <= max.z);
    }

    public bool Contains(Vector3 point, float precision)
    {
        return (point.x + precision >= min.x && point.y + precision >= min.y && point.z + precision >= min.z)
            && (point.x - precision <= max.x && point.y - precision <= max.y && point.z - precision <= max.z);
    }

    public void AdjustTo(Vector3 min, Vector3 max)
    {
        this.min = min;
        this.max = max;
    }

    public void AdjustToContain(AABB aabb)
    {
        min = Vector3.Min(min, aabb.min);
        max = Vector3.Max(max, aabb.max);
    }

    public void AdjustToContain(Vector3 min, Vector3 max)
    {
        this.min = Vector3.Min(this.min, min);
        this.max = Vector3.Max(this.max, max);
    }

    public void AdjustToContain(Vector3 v)
    {
        min = Vector3.Min(min, v);
        max = Vector3.Max(max, v);
    }

    public void AdjustToContain(Vector3 v, float radius)
    {
        Vector3 r = new Vector3(radius, radius, radius);
        this.min = Vector3.Min(this.min, v - r);
        this.max =Vector3.Max(this.max, v + r);
    }

    

}