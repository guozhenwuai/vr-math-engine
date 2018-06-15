using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MPrefab
{
    private static Mesh sphereMesh = null;

    private static Mesh cubeMesh = null;

    public static Mesh GetSphereMesh()
    {
        if(sphereMesh == null)
        {
            float radius = 0.5f;
            int statck = 20;
            float statckStep = Mathf.PI / statck;
            int slice = 20;
            float sliceStep = Mathf.PI / slice;

            float r, x, y, z;
            float alpha = 0;
            float beta = 0;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            for(int i = 0; i < statck + 1; i++)
            {
                alpha = -Mathf.PI / 2 + i * statckStep;
                y = radius * Mathf.Sin(alpha);
                r = radius * Mathf.Cos(alpha);

                for(int j = 0; j < 2 * slice; j++)
                {
                    beta = j * sliceStep;
                    x = r * Mathf.Cos(beta);
                    z = -r * Mathf.Sin(beta);
                    vertices.Add(new Vector3(x, y, z));
                }
            }
            for(int i = 0; i < statck; i++)
            {
                for(int j = 0; j < 2 * slice; j++)
                {
                    int a = i * 2 * slice + j;
                    int b = i * 2 * slice + (j + 1) % (2 * slice);
                    int c = (i + 1) * 2 * slice + (j + 1) % (2 * slice);
                    int d = (i + 1) * 2 * slice + j;
                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);
                    triangles.Add(a);
                    triangles.Add(c);
                    triangles.Add(d);
                }
            }
            sphereMesh = new Mesh();
            sphereMesh.vertices = vertices.ToArray();
            sphereMesh.triangles = triangles.ToArray();
            sphereMesh.RecalculateNormals();
        }
        return Object.Instantiate(sphereMesh);
    }

    public static Mesh GetCubeMesh()
    {
        if(cubeMesh == null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeMesh = cube.GetComponent<MeshFilter>().mesh;
            Object.Destroy(cube);
        }
        return Object.Instantiate(cubeMesh);
    }

    public static Mesh GetCircleFaceMesh(Vector3 center, Vector3 normal, float radius)
    {
        int pieces = 48;
        float angle = 2 * Mathf.PI / pieces;
        Vector3[] vertices = new Vector3[pieces];
        Vector3[] normals = new Vector3[pieces];
        int[] triangles = new int[3 * (pieces - 2)];
        Vector3 pos = MHelperFunctions.RandomPointInCircle(center, normal, radius);
        for (int i = 0; i < pieces; i++)
        {
            vertices[i] = pos;
            normals[i] = normal;
            pos = MHelperFunctions.CalcRotate(pos - center, normal, angle) + center;
        }
        for (int i = 0; i < pieces - 2; i++)
        {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        return mesh;
    }

    public static Mesh GetCylinderFaceMesh(Vector3 center1, Vector3 center2, Vector3 normal, float radius)
    {
        int pieces = 48;
        float angle = 2 * Mathf.PI / pieces;
        Vector3[] vertices = new Vector3[pieces * 2];
        int[] triangles = new int[6 * pieces];
        Vector3 startPos = MHelperFunctions.RandomPointInCircle(center1, normal, radius);
        Vector3 pos = startPos;
        for (int i = 0; i < 2 * pieces; i++)
        {
            vertices[i] = pos;
            pos = (i == pieces - 1? startPos + center2 - center1: MHelperFunctions.CalcRotate(pos - center1, normal, angle) + center1);
        }
        for(int i = 0; i < pieces; i++)
        {
            triangles[6 * i] = i;
            triangles[6 * i + 1] = i + pieces;
            triangles[6 * i + 2] = pieces + (i + 1) % pieces;
            triangles[6 * i + 3] = i;
            triangles[6 * i + 4] = pieces + (i + 1) % pieces;
            triangles[6 * i + 5] = (i + 1) % pieces;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }

    public static Mesh GetConeFaceMesh(Vector3 bottom, Vector3 top, Vector3 normal, float radius)
    {
        int pieces = 48;
        float angle = 2 * Mathf.PI / pieces;
        Vector3[] vertices = new Vector3[pieces + 1];
        int[] triangles = new int[3 * pieces];
        Vector3 pos = MHelperFunctions.RandomPointInCircle(bottom, normal, radius);
        for(int i = 0; i < pieces; i++)
        {
            vertices[i] = pos;
            pos = MHelperFunctions.CalcRotate(pos - bottom, normal, angle) + bottom;
        }
        vertices[pieces] = top;
        for(int i = 0; i < pieces; i++)
        {
            triangles[3 * i] = pieces;
            triangles[3 * i + 1] = i;
            triangles[3 * i + 2] = (i + 1) % pieces;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }

    public static Mesh GetCircleEdgeMesh(Vector3 center, Vector3 normal, float circleRadius,  float radius)
    {
        int pieces = 48;
        float angle = 2 * Mathf.PI / pieces;
        Vector3 lastPos = MHelperFunctions.RandomPointInCircle(center, normal, circleRadius);
        Vector3 lastNormal = Vector3.Cross(lastPos - center, normal).normalized;
        CombineInstance[] combines= new CombineInstance[pieces];
        Matrix4x4 transform = Matrix4x4.Scale(Vector3.one);
        for (int i = 0; i < pieces; i++)
        {
            Vector3 curPos = MHelperFunctions.CalcRotate(lastPos - center, normal, angle) + center;
            Vector3 curNormal = Vector3.Cross(curPos - center, normal).normalized;
            combines[i].mesh = GetLineMesh(lastPos, lastNormal, curPos, curNormal, radius);
            combines[i].transform = transform;
            lastPos = curPos;
            lastNormal = curNormal;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combines);
        return mesh;
    }

    public static Mesh GetLineMesh(Vector3 startPosition, Vector3 startNormal, Vector3 endPosition, Vector3 endNormal, float radius)
    {
        float angle = Mathf.PI / 6;
        Vector3[] _Vertices = new Vector3[24];
        Vector3 lastPos = MHelperFunctions.RandomPointInCircle(startPosition, startNormal, radius);
        _Vertices[0] = lastPos;
        for (int i = 1; i < 12; i++)
        {
            Vector3 curPos = MHelperFunctions.CalcRotate(lastPos - startPosition, startNormal, angle) + startPosition;
            _Vertices[i] = curPos;
            lastPos = curPos;
        }
        lastPos = (MHelperFunctions.PointProjectionInFace(_Vertices[0], endNormal, endPosition) - endPosition).normalized * radius + endPosition;
        _Vertices[12] = lastPos;
        for (int i = 13; i < 24; i++)
        {
            Vector3 curPos = MHelperFunctions.CalcRotate(lastPos - endPosition, endNormal, angle) + endPosition;
            _Vertices[i] = curPos;
            lastPos = curPos;
        }

        //生成所有面
        int[] _Triangles = new int[72]
        {
            0,1,12,
            1,13,12,
            1,2,13,
            2,14,13,
            2,3,14,
            3,15,14,
            3,4,15,
            4,16,15,
            4,5,16,
            5,17,16,
            5,6,17,
            6,18,17,
            6,7,18,
            7,19,18,
            7,8,19,
            8,20,19,
            8,9,20,
            9,21,20,
            9,10,21,
            10,22,21,
            10,11,22,
            11,23,22,
            11,0,23,
            0,12,23
        };

        Mesh mesh = new Mesh();
        mesh.vertices = _Vertices;
        mesh.triangles = _Triangles;
        return mesh;
    }

    public static Mesh GetLineMesh(Vector3 startPosition, Vector3 endPosition, float radius)
    {
        Vector3 normal = (endPosition - startPosition).normalized;
        float angle = Mathf.PI / 6;
        Vector3[] _Vertices = new Vector3[24];
        Vector3 lastPos = MHelperFunctions.RandomPointInCircle(startPosition, normal, radius);
        _Vertices[0] = lastPos;
        for (int i = 1; i < 12; i++)
        {
            Vector3 curPos = MHelperFunctions.CalcRotate(lastPos - startPosition, normal, angle) + startPosition;
            _Vertices[i] = curPos;
            lastPos = curPos;
        }
        lastPos = _Vertices[0] + (endPosition - startPosition);
        _Vertices[12] = lastPos;
        for (int i = 13; i < 24; i++)
        {
            Vector3 curPos = MHelperFunctions.CalcRotate(lastPos - endPosition, normal, angle) + endPosition;
            _Vertices[i] = curPos;
            lastPos = curPos;
        }

        //生成所有面
        int[] _Triangles = new int[72]
        {
            0,1,12,
            1,13,12,
            1,2,13,
            2,14,13,
            2,3,14,
            3,15,14,
            3,4,15,
            4,16,15,
            4,5,16,
            5,17,16,
            5,6,17,
            6,18,17,
            6,7,18,
            7,19,18,
            7,8,19,
            8,20,19,
            8,9,20,
            9,21,20,
            9,10,21,
            10,22,21,
            10,11,22,
            11,23,22,
            11,0,23,
            0,12,23
        };

        Mesh mesh = new Mesh();
        mesh.vertices = _Vertices;
        mesh.triangles = _Triangles;
        return mesh;
    }

    static GameObject textMeshObj = null;

    public static GameObject GetTextMesh()
    {
        if(textMeshObj == null)
        {
            textMeshObj = new GameObject();
            textMeshObj.SetActive(false);
            textMeshObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            textMeshObj.AddComponent<MeshRenderer>();
            TextMesh textMesh = textMeshObj.AddComponent<TextMesh>();
            textMesh.text = "";
            textMesh.anchor = TextAnchor.LowerCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 40;
        }
        return Object.Instantiate(textMeshObj);
    }
}