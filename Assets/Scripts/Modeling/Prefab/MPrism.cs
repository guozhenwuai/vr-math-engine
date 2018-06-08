using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MPrism
{
    static MMesh mesh;

    public static MMesh GetMMesh()
    {
        mesh = new MMesh();
        MPoint a = mesh.CreatePoint(new Vector3(-Mathf.Sqrt(3)/2, -0.75f, -0.5f));
        MPoint b = mesh.CreatePoint(new Vector3(Mathf.Sqrt(3)/2, -0.75f, -0.5f));
        MPoint c = mesh.CreatePoint(new Vector3(0, -0.75f, 1));
        MPoint e = mesh.CreatePoint(new Vector3(-Mathf.Sqrt(3)/2, 0.75f, -0.5f));
        MPoint f = mesh.CreatePoint(new Vector3(Mathf.Sqrt(3)/2, 0.75f, -0.5f));
        MPoint g = mesh.CreatePoint(new Vector3(0, 0.75f, 1));
        MLinearEdge ab = mesh.CreateLinearEdge(a, b);
        MLinearEdge bc = mesh.CreateLinearEdge(b, c);
        MLinearEdge ca = mesh.CreateLinearEdge(c, a);
        MLinearEdge ef = mesh.CreateLinearEdge(e, f);
        MLinearEdge fg = mesh.CreateLinearEdge(f, g);
        MLinearEdge ge = mesh.CreateLinearEdge(g, e);
        MLinearEdge ae = mesh.CreateLinearEdge(a, e);
        MLinearEdge bf = mesh.CreateLinearEdge(b, f);
        MLinearEdge cg = mesh.CreateLinearEdge(c, g);
        List<MLinearEdge> abc = new List<MLinearEdge>();
        abc.Add(ab);
        abc.Add(bc);
        abc.Add(ca);
        MPolygonFace f1 = mesh.CreatePolygonFace(abc);
        List<MLinearEdge> efg = new List<MLinearEdge>();
        efg.Add(ef);
        efg.Add(fg);
        efg.Add(ge);
        MPolygonFace f2 = mesh.CreatePolygonFace(efg);
        List<MLinearEdge> abef = new List<MLinearEdge>();
        abef.Add(ab);
        abef.Add(ef);
        abef.Add(ae);
        abef.Add(bf);
        MPolygonFace f3 = mesh.CreatePolygonFace(abef);
        List<MLinearEdge> bcfg = new List<MLinearEdge>();
        bcfg.Add(bc);
        bcfg.Add(fg);
        bcfg.Add(bf);
        bcfg.Add(cg);
        MPolygonFace f4 = mesh.CreatePolygonFace(bcfg);
        List<MLinearEdge> cage = new List<MLinearEdge>();
        cage.Add(ca);
        cage.Add(ge);
        cage.Add(cg);
        cage.Add(ae);
        MPolygonFace f5 = mesh.CreatePolygonFace(cage);
        return mesh;
    }
}