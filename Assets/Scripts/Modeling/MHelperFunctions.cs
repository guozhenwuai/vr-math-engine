using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MHelperFunctions
{

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);


		if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;

		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);

		float d = a*e - b*b;

		//lines are not parallel
		if(d != 0.0f){

			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);

			float s = (b*f - c*e) / d;
			float t = (a*f - c*b) / d;

			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;

			return true;
		}

		else{
			return false;
		}
	}  

    // 浮点数的精度判等
    public static bool FloatEqual(float a, float b)
    {
        return Mathf.Abs(a - b) < MDefinitions.FLOAT_PRECISION;
    }

    // 浮点数与零的比较
    public static int FloatZero(float f)
    {
        if (Mathf.Abs(f) < MDefinitions.FLOAT_PRECISION)
        {
            return 0;
        }
        if (f < 0) return -1;
        else return 1;
    }

    // 判断向量是否平行
    public static bool Parallel(Vector3 v1, Vector3 v2)
    {
        return FloatEqual(Mathf.Abs(Vector3.Dot(v1.normalized, v2.normalized)),1);
    }

    // 判断向量是否垂直
    public static bool Perpendicular(Vector3 v1, Vector3 v2)
    {
        return FloatEqual(Vector3.Dot(v1, v2), 0);
    }

    // 计算两向量之间的锐角角度
    public static float CalcAngle(Vector3 v1, Vector3 v2)
    {
        float angle = Mathf.Acos(Vector3.Dot(v1.normalized, v2.normalized)) * Mathf.Rad2Deg;
        if (angle > 90) angle = 180 - angle;
        return angle;
    }

    // 计算点到平面的距离
    public static float DistanceP2F(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return Mathf.Abs(Vector3.Dot(faceNormal.normalized, point - facePoint));
    }

    //计算点到直线的距离
	public static float DistanceP2L(Vector3 point, Vector3 lineVector, Vector3 linePoint)
	{
        Vector3 v = point - linePoint;
        float d =Mathf.Abs( Vector3.Dot(lineVector, v))/lineVector.magnitude ;
		float h = Mathf.Sqrt (v.magnitude * v.magnitude-d*d);
		return h;
	}

    //计算直线到直线的距离
    public static float DistanceL2L(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 temp1, temp2;
        if (Parallel(lineVec1, lineVec2))
        {
            return DistanceP2L(linePoint1, lineVec2, linePoint2);
        }
        else if (LineLineIntersection(out temp1, linePoint1, lineVec1, linePoint2, lineVec2))
        {
            return 0;
        }
        else
        {
            if (ClosestPointsOnTwoLines(out temp1, out temp2, linePoint1, lineVec1, linePoint2, lineVec2))
            {
                return DistanceP2L(temp2, lineVec1, linePoint1);
            }
            else
            {
                return 0;
            }
        }
    }

    // 计算点在平面上的投影点
    public static Vector3 PointProjectionInFace(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return point - faceNormal.normalized * Vector3.Dot(faceNormal.normalized, point - facePoint);
    }
    
    // 根据旋转前后向量计算旋转轴和旋转角
    public static void CalcRotateAxisAndAngle(out Vector3 rotateAxis, out float rotateAngle, Vector3 oldVec, Vector3 newVec)
    {
        rotateAxis = Vector3.Cross(oldVec.normalized, newVec.normalized).normalized;
        rotateAngle = Mathf.Acos(Vector3.Dot(oldVec.normalized, newVec.normalized));
    }

    // 根据旋转轴和旋转角计算给定向量（或点）旋转后的结果
    public static Vector3 CalcRotate(Vector3 r, Vector3 rotateAxis, float rotateAngle)
    {
        float cos = Mathf.Cos(rotateAngle);
        float sin = Mathf.Sin(rotateAngle);
        return r * cos + (1 - cos) * Vector3.Dot(rotateAxis, r) * rotateAxis + sin * Vector3.Cross(rotateAxis, r);
    }

    // 判断点是否在多边形内（点为在多边形平面上的投影点）
    public static bool InPolygon(Vector3 point, List<Vector3> points)
    {
        bool res = false;
        int i, j;
        int count = points.Count;

        for (i = 0, j = count - 1; i < count; j = i++)
        {
            if (((points[i].y > point.y) != (points[j].y > point.y)) &&
                (point.x < (points[j].x - points[i].x) * (point.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
            {
                res = !res;
            }
        }
        return res;
    }

    // 计算三个点围成面积
    public static float TriangleSurface(Vector3 a, Vector3 b, Vector3 c)
    {
        float la = Vector3.Distance(a, b);
        float lb = Vector3.Distance(b, c);
        float lc = Vector3.Distance(c, a);

        float p = (la + lb + lc) / 2;
        return Mathf.Sqrt(p * (p - la) * (p - lb) * (p - lc));
    }
}
