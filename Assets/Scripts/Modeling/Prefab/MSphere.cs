using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MSphere
{
    static MMesh mesh;

    public static MMesh GetMMesh()
    {
        mesh = new MMesh();
        MPoint center = mesh.CreatePoint(Vector3.zero);
        MSphereFace face = mesh.CreateSphereFace(center, 0.5f);
        return mesh;
    }
}