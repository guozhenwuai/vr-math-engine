using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MCone
{
    static MMesh mesh;

    public static MMesh GetMMesh()
    {
        mesh = new MMesh();
        MPoint a = mesh.CreatePoint(new Vector3(0, 0.75f, 0));
        MPoint b = mesh.CreatePoint(new Vector3(0, -0.75f, 0));
        MCurveEdge curvb = mesh.CreateCurveEdge(b, 0.5f, Vector3.up);
        MCircleFace fb = mesh.CreateCircleFace(curvb);
        MConeFace cf = mesh.CreateConeFace(a, curvb);
        return mesh;
    }
}