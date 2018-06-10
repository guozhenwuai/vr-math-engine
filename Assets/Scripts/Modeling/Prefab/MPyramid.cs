using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MPyramid
{
    static MMesh mesh;

    public static MMesh GetMMesh()
    {
        mesh = new MMesh();
        MPoint a = mesh.CreatePoint(new Vector3(Mathf.Sqrt(3) / 2, -0.75f, -0.5f));
        MPoint b = mesh.CreatePoint(new Vector3(-Mathf.Sqrt(3) / 2, -0.75f, -0.5f));
        MPoint c = mesh.CreatePoint(new Vector3(0, -0.75f, 1));
        MPoint d = mesh.CreatePoint(new Vector3(0, 0.75f, 0));
        MLinearEdge ab = mesh.CreateLinearEdge(a, b);
        MLinearEdge bc = mesh.CreateLinearEdge(b, c);
        MLinearEdge ca = mesh.CreateLinearEdge(c, a);
        MLinearEdge ad = mesh.CreateLinearEdge(a, d);
        MLinearEdge bd = mesh.CreateLinearEdge(b, d);
        MLinearEdge cd = mesh.CreateLinearEdge(c, d);
        List<MLinearEdge> abc = new List<MLinearEdge>();
        abc.Add(ab);
        abc.Add(bc);
        abc.Add(ca);
        mesh.CreatePolygonFace(abc);
        List<MLinearEdge> abd = new List<MLinearEdge>();
        abd.Add(ab);
        abd.Add(bd);
        abd.Add(ad);
        mesh.CreatePolygonFace(abd);
        List<MLinearEdge> bcd = new List<MLinearEdge>();
        bcd.Add(bc);
        bcd.Add(bd);
        bcd.Add(cd);
        mesh.CreatePolygonFace(bcd);
        List<MLinearEdge> cad = new List<MLinearEdge>();
        cad.Add(ca);
        cad.Add(cd);
        cad.Add(ad);
        mesh.CreatePolygonFace(cad);
        return mesh;
    }
}