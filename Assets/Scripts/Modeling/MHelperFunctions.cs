using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MHelperFunctions
{
	public static bool inface(Vector3 v1,Vector3 v2,Vector3 v3,Vector3 v4){
		Vector3f e1 = v2 - v1;
		Vector3f e2 = v3 - v1;
		Vector3f e3 = v4 - v1;
		float angle1, angle2, angle3;
		Vector3f axis;
		CalcRotateAxisAndAngle (axis,angle1,e1,e2);
		CalcRotateAxisAndAngle (axis,angle2,e2,e3);
		CalcRotateAxisAndAngle (axis,angle3,e3,e1);
		if ((angle1 + angle2 + angle3) == 0) {
			return true;
		} else
			return false;
	}
	// false=parallal true=intersect

	public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint){

		float length;
		float dotNumerator;
		float dotDenominator;
		Vector3 vector;
		intersection = Vector3.zero;

		//calculate the distance between the linePoint and the line-plane intersection point
		dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
		dotDenominator = Vector3.Dot(lineVec, planeNormal);

		//line and plane are not parallel
		if(dotDenominator != 0.0f){
			length =  dotNumerator / dotDenominator;

			//create a vector from the linePoint to the intersection point
			vector = SetVectorLength(lineVec, length);

			//get the coordinates of the line-plane intersection point
			intersection = linePoint + vector;  

			return true;    
		}

		//output not valid
		else{
			return false;
		}
	}
	public static void LineFace (out float angle,out float distance, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint){
		Vector3 intersection = Vector3.zero;
		bool intersect = LinePlaneIntersection (intersection,linePoint,lineVec,planeNormal,planePoint);
		if (intersect) {
			distance = 0;
			angle = Mathf.Acos (Mathf.dot (lineVec, planeNormal) / (lineVec.magnitude * planeNormal.magnitude));
		} else {
			distance = Mathf.Abs(Mathf.dot (linePoint - planePoint, planeNormal) / planeNormal.magnitude);
			angle = 0;
		}
	}
	public static bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position){

		linePoint = Vector3.zero;
		lineVec = Vector3.zero;

		//We can get the direction of the line of intersection of the two planes by calculating the 
		//cross product of the normals of the two planes. Note that this is just a direction and the line
		//is not fixed in space yet. We need a point for that to go with the line vector.
		lineVec = Vector3.Cross(plane1Normal, plane2Normal);

		//Next is to calculate a point on the line to fix it's position in space. This is done by finding a vector from
		//the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
		//errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
		//the cross product of the normal of plane2 and the lineDirection.      
		Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);        

		float denominator = Vector3.Dot(plane1Normal, ldir);

		//Prevent divide by zero and rounding errors by requiring about 5 degrees angle between the planes.
		if(Mathf.Abs(denominator) > 0.006f){

			Vector3 plane1ToPlane2 = plane1Position - plane2Position;
			float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / denominator;
			linePoint = plane2Position + t * ldir;

			return true;
		}

		//output not valid
		else{
			return false;
		}
	}   
	public static void FaceFace (out float angle,out float distance,Vector3 planeNormal, Vector3 planePoint,Vector3 planeNormal1, Vector3 planePoint1){
		if (Parallel (planeNormal, planeNormal1)) {
			angle = 0;
			distance = Mathf.Abs (Mathf.dot (planePoint1 - planePoint, planeNormal) / planeNormal.magnitude);
		} else {
			
				distance = 0;
				Vector3 rotateAxis;
				angle = CalcRotateAxisAndAngle (rotateAxis,angle,planeNormal,planeNormal1);

		}
	}
	public static void LineLine( out float angle,out float distance,Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){
		if (Parallel (lineVec1,lineVec2)) {
			angle = 0;
			distance = DistanceP2L(linePoint1,  lineVec2,  linePoint2,planeNormal1,planePoint1);
		} else {
			Vector3 rotateAxis = Vector3.zero;
			Vector3f closepoint1, closepoint2 = Vector3f.zero;
			CalcRotateAxisAndAngle (rotateAxis,angle,lineVec1,lineVec2);

			if (LineLineIntersection (closepoint1,linePoint1,lineVec1,linePoint2,lineVec2)) {
				distance = 0;


			} else {
				if (ClosestPointsOnTwoLines (closepoint1, closepoint2, linePoint1,lineVec1, linePoint2,lineVec2)) {
					distance = DistanceP2L(closepoint2,lineVec1,linePoint1);

				}
			}
		}
	}
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
        v1 = Vector3.Normalize(v1);
        v2 = Vector3.Normalize(v2);
        return FloatEqual(Mathf.Abs(Vector3.Dot(v1, v2)),1);
    }

    // 判断向量是否垂直
    public static bool Perpendicular(Vector3 v1, Vector3 v2)
    {
        v1 = Vector3.Normalize(v1);
        v2 = Vector3.Normalize(v2);
        return FloatEqual(Vector3.Dot(v1, v2), 0);
    }

    // 计算点到平面的距离
    public static float DistanceP2F(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return Mathf.Abs(Vector3.Dot(faceNormal, point - facePoint));
    }
	public static float DistanceP2L(Vector3 point, Vector3 linevector, Vector3 linepoint)
	{
		float d =Mathf.Abs( Mathf.dot (linevector, point - linepoint))/linevector.magnitude ;
		float h = Mathf.Sqrt ((point - linepoint).magnitude*(point - linepoint).magnitude-d*d);
		return h;
	}
    // 计算点在平面上的投影点
    public static Vector3 PointProjectionInFace(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return point - faceNormal * Vector3.Dot(faceNormal, point - facePoint);
    }
    
    // 根据旋转前后向量计算旋转轴和旋转角
    public static void CalcRotateAxisAndAngle(out Vector3 rotateAxis, out float rotateAngle, Vector3 oldVec, Vector3 newVec)
    {
        rotateAxis = Vector3.Normalize(Vector3.Cross(oldVec, newVec));
        rotateAngle = Mathf.Acos(Vector3.Dot(oldVec, newVec));
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
