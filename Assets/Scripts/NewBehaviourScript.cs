using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {
    public GameObject red;
    public GameObject blue;
    public Material mat;
    Material lineMaterial;
    Mesh mesh;
	// Use this for initialization
	void Start () {
        // Unity has a built-in shader that is useful for drawing
        // simple colored things.
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);

        AABB aabb = new AABB(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
        Debug.Log(aabb.Contains(new Vector3(0, 0.5f, 0), 0.01f));

        GameObject g = Resources.Load<GameObject>("Prefabs/Cube");
        mesh = (gameObject.AddComponent<MeshFilter>().mesh = Mesh.Instantiate(g.GetComponent<MeshFilter>().sharedMesh));
        Vector3[] vertices = new Vector3[6];
        vertices[0] = new Vector3(-1.0f, 0, 1.0f);
        vertices[1] = new Vector3(1.0f, 0, 1.0f);
        vertices[2] = new Vector3(1.5f, 0, 0);
        vertices[3] = new Vector3(2.0f, 0, -1.0f);
        vertices[4] = new Vector3(1.0f, 0, -1.3f);
        vertices[5] = new Vector3(0, 0, -1.0f);

        int[] triangles = new int[12];
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 5;
        triangles[3] = 1;
        triangles[4] = 4;
        triangles[5] = 5;
        triangles[6] = 1;
        triangles[7] = 2;
        triangles[8] = 4;
        triangles[9] = 2;
        triangles[10] = 3;
        triangles[11] = 4;

        mesh.triangles = triangles;
        mesh.vertices = vertices;

        Vector3 a = new Vector3(1.0f, 2.0f, 3.0f);
        Vector3 b = new Vector3(1.0f, 2.0f, 3.0f);
        Vector3 c = new Vector3();
        Vector3 d = new Vector3(3.1f, 2.0f, 2.4f);
        Vector3 e = new Vector3(4.2f, -1.9f, 3.0f);

        MPoint p1 = new MPoint(a);
        MPoint p2 = new MPoint(b);

        MLinearEdge e1 = new MLinearEdge(p1, p2);
        MLinearEdge e2 = new MLinearEdge(p1, p2);
        MLinearEdge e3 = new MLinearEdge(p2, p1);
        MCurveEdge e4 = new MCurveEdge(p1, new Vector3(0, 1, 0), 1);
        MCurveEdge e5 = new MCurveEdge(p1, new Vector3(0, -1, 0), 1);

        List<MEntity> l = new List<MEntity>();
        l.Add(p1);
        l.Add(e1);
        Debug.Log(l.Contains(p2));
        Debug.Log(l.Contains(e2));
        Debug.Log(l.Contains(e3));
        Debug.Log(l.Contains(e4));
	}

    public void OnRenderObject()
    {
        GL.PushMatrix();
        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);
        GL.Color(Color.blue);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0.5f, 0.5f, 0);
        GL.Color(Color.yellow);
        GL.Vertex3(0.5f, 0.5f, 0);
        GL.Vertex3(1, 0, 0);
        GL.End();
        GL.PopMatrix();
    }
    

    // Update is called once per frame
    void Update () {
	}
}
