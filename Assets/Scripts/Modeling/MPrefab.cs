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
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereMesh = sphere.GetComponent<MeshFilter>().mesh;
            Object.Destroy(sphere);
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
}