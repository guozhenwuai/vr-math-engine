using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MRegularTriangle
{
    public static MMesh GetMMesh()
    {
        MMesh mesh = new MMesh();
        MPoint a = mesh.CreatePoint(new Vector3(Mathf.Sqrt(3) / 2, 0, -0.5f));
        MPoint b = mesh.CreatePoint(new Vector3(-Mathf.Sqrt(3) / 2, 0, -0.5f));
        MPoint c = mesh.CreatePoint(new Vector3(0, 0, 1));
        MLinearEdge ab = mesh.CreateLinearEdge(a, b);
        MLinearEdge bc = mesh.CreateLinearEdge(b, c);
        MLinearEdge ca = mesh.CreateLinearEdge(c, a);
        List<MLinearEdge> abc = new List<MLinearEdge>();
        abc.Add(ab);
        abc.Add(bc);
        abc.Add(ca);
        mesh.CreatePolygonFace(abc);
        return mesh;
    }
}