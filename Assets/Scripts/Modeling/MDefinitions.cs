using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MDefinitions
{
    public static float ACTIVE_DISTANCE = 0.1f;

    public static float POINT_PRECISION = 0.02f;

    public static float LINE_RADIUS = 0.02f;

    public static float AUTO_REVISE_FACTOR = 0.02f;

    public static float FLOAT_PRECISION = 0.0000001f;

    public static float VECTOR3_PRECISION = 0.01f;

    public static Vector3 DEFAULT_POSITION = new Vector3(0, 1.2f, 0);

    public static float DEFAULT_SCALE = 0.2f;

    public static float DEFAULT_TEXT_PLANE_HEIGHT = 0.2f;

    public static Vector3 DEFAULT_TEXT_PLANE_SCALE = new Vector3(0.04f, 1f, 0.02f);

    public static Vector3 DEFAULT_ACTIVE_TEXT_OFFSET = new Vector3(0, 0.1f, 0);

    public static Vector3 DEFAULT_PREFAB_OFFSET = Vector3.zero;

    public static string SAVE_PATH = Application.dataPath + "/Models";

    public static float TOUCHPAD_AXIS_CHANGE_THRESHOLD = 0.5f;

}
