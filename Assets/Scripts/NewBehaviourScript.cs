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
        Vector3 v1 = new Vector3(0, 1, 0);
        Vector3 v2 = new Vector3(1, 0, 0);
        Vector3 v3 = new Vector3(-1, -1, 0);

        List<Vector3> l = new List<Vector3>();
        l.Add(v1);
        l.Add(v2);
        l.Add(v3);

        Debug.Log(l.IndexOf(new Vector3(0, 1, 0)));
    }
    

    // Update is called once per frame
    void Update () {
	}
}
