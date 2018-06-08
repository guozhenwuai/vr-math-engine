using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MCylinder
{
    static MMesh mesh;

    public static MMesh GetMMesh()
    {
        mesh = new MMesh();
        MPoint a = mesh.CreatePoint(new Vector3(0, -0.75f, 0));
        MCurveEdge curva = mesh.CreateCurveEdge(a, 0.5f, Vector3.up);
        MCircleFace fa = mesh.CreateCircleFace(curva);
        MPoint b = mesh.CreatePoint(new Vector3(0, 0.75f, 0));
        MCurveEdge curvb = mesh.CreateCurveEdge(b, 0.5f, Vector3.up);
        MCircleFace fb = mesh.CreateCircleFace(curvb);
        MCylinderFace f = mesh.CreateCylinderFace(curvb, curva);
        return mesh;
    }
}