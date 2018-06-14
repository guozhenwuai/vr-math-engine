using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MCircle
{

    public static MMesh GetMMesh()
    {
        MMesh mesh = new MMesh();
        MPoint a = mesh.CreatePoint(new Vector3(0, 0, 0));
        MCurveEdge curv = mesh.CreateCurveEdge(a, 0.5f, Vector3.up);
        mesh.CreateCircleFace(curv);
        return mesh;
    }
}