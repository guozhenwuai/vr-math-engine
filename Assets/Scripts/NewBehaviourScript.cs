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

        Debug.Log(MHelperFunctions.CalcAngle(v1, v2));
        Debug.Log(MHelperFunctions.CalcAngle(v2, v3));
    }
    

    // Update is called once per frame
    void Update () {
	}
}
