using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RegressionPlane : MonoBehaviour {
	public enum Axes{ verticalX,verticalY,verticalZ};

	[Tooltip("Number of seconds")]
	[Range(1f,5f)] 
	public float updatePeriod = 1;//time period(in seconds) to check for an update(in case the points configuration has changed, another plane equation is calculated)


	public List<GameObject> objects = new List<GameObject>();

	Vector3[] points = null; 
	bool corUp = false; 

	public Axes verticalAxis = Axes.verticalY;
	Axes prevVerticalAxis;

	Transform cachedTransform;

	Quaternion initialRotation;

	public bool showGizmos = true;

	// Use this for initialization
	void Start () {
		cachedTransform = transform;
		initialRotation = cachedTransform.localRotation;
		prevVerticalAxis = verticalAxis;
		StartCoroutine (PeriodicPlaneCalculation ());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos(){
		if(showGizmos){
			Gizmos.color = Color.red;
			foreach(GameObject peak in objects){
				Gizmos.DrawSphere(peak.transform.position, 0.3f);
			}
		}
	}

	bool GetNewPoints(){
		bool newConf = false;//gets true if at least one object's position has been changed
		//first count the number of objects,to know how many to allocate
		int count = 0;
		foreach(GameObject gameObject in objects){
			if(gameObject != null){
				count += 1;
			}
		}

		if((count == 0) && (null != points)){
			points = null;
			System.GC.Collect();
			newConf = true;
		}
		else if(((null != points) && (count != points.Length)) || ((null == points) && (count > 0))) {
			points = new Vector3[count];
			System.GC.Collect();
			newConf = true;
		}

		int i = 0;
		foreach(GameObject gameObject in objects){
			if(gameObject != null){
				if(points[i] != gameObject.transform.position){
					points[i] = gameObject.transform.position;
					if(false == newConf){
						newConf = true;
					}
				}
				i += 1;
			}
		}

		return newConf;
	}

	void CalcPlaneSizeRotation(float A,float B,float C,float D){
		if ((A == 0) && (B == 0) && (C == 0)) {
			cachedTransform.localScale = Vector3.zero;
			return;
		}

		Quaternion rotation = Quaternion.FromToRotation( new Vector3(A,B,C) ,Vector3.up);
		float minX = 0;
		float maxX = 0;
		float minZ = 0;
		float maxZ = 0;
		bool limitsSet = false;

		Vector3 lineAxis = Vector3.zero;
		Vector3 pointOnLine = Vector3.zero;//point on line

		//intersection line between plane and Oxz is Ax + Cz + D = 0 if A,C are not both zero
		if (Mathf.Approximately (A, 0) && Mathf.Approximately (C, 0)) {
			//particular case, no need for any rotation (TODO avoid the code duplication, use some delegate)
			foreach(Vector3 point in points){
				if(false == limitsSet){
					limitsSet = true;
					minX = point.x;
					maxX = point.x;
					minZ = point.z;
					maxZ = point.z;
				} else {
					if(point.x < minX){
						minX = point.x;
					}
					else if(point.x > maxX){
						maxX = point.x;
					}
					if(point.z < minZ){
						minZ = point.z;
					}
					else if(point.z > maxZ){
						maxZ = point.z;
					}
				}
			}
		} else {
			if(false == Mathf.Approximately (C, 0)){
				pointOnLine.z = -D/C;
			} else {
				pointOnLine.x = -D/A;
			}

			if(Mathf.Approximately (A, 0)){
				lineAxis.z = 1;
			} else if(Mathf.Approximately (C, 0)){
				lineAxis.x = 1;
			} else {
				lineAxis.x = 1;
				lineAxis.z = -A/C;
			}

			foreach(Vector3 point in points){
				Vector3 prj = Calculus.pointToLinePrj(point,lineAxis,pointOnLine);

				Vector3 rotPoint = prj + rotation * (point - prj);

				if(false == limitsSet){
					limitsSet = true;
					minX = rotPoint.x;
					maxX = rotPoint.x;
					minZ = rotPoint.z;
					maxZ = rotPoint.z;
				} else {
					if(rotPoint.x < minX){
						minX = rotPoint.x;
					}
					else if(rotPoint.x > maxX){
						maxX = rotPoint.x;
					}
					if(rotPoint.z < minZ){
						minZ = rotPoint.z;
					}
					else if(rotPoint.z > maxZ){
						maxZ = rotPoint.z;
					}
				}
			}
		}

		Vector3 middlePoint = new Vector3((minX + maxX)*0.5f , 0 , (minZ + maxZ)*0.5f);
		float scaleX = 1;
		float scaleZ = 1;
		float xWidth = Mathf.Abs(maxX - minX);
		float zWidth = Mathf.Abs(maxZ - minZ);
		if(xWidth > 10){
			scaleX = xWidth * 0.1f;
		}
		if(zWidth > 10){
			scaleZ = zWidth * 0.1f;
		}

		Quaternion quatInverse = Quaternion.identity;
		if (lineAxis != Vector3.zero) {
			Vector3 prjMiddlePoint = Calculus.pointToLinePrj (middlePoint, lineAxis, pointOnLine);
			quatInverse = Quaternion.Inverse (rotation);
			cachedTransform.localPosition = prjMiddlePoint + quatInverse * (middlePoint - prjMiddlePoint);
		} else {
			cachedTransform.localPosition = middlePoint;
		}

		Vector3 virtMiddPoint = Vector3.zero;
		foreach (Vector3 point in points) {
			virtMiddPoint += point;
		}
		virtMiddPoint /= 3;

		cachedTransform.localScale = new Vector3 (scaleX,scaleZ,1);//scaleZ is used for scaling on Y(cause the plane on loading, is in OXY plane)
		cachedTransform.localRotation = quatInverse*initialRotation;
	}

	IEnumerator PeriodicPlaneCalculation(){
		corUp = true;
		while(corUp){
			if((true == GetNewPoints()) || (prevVerticalAxis != verticalAxis)){
				float A = 0,B = 0,C = 0,D = 0;//plane equation
				prevVerticalAxis = verticalAxis;
				if(points.Length == 3){
					Plane plane = new Plane(points[0],points[1],points[2]);
					A = plane.normal.x;
					B = plane.normal.y;
					C = plane.normal.z;
					D = -(A*points[0].x + B*points[0].y + C*points[0].z);
				} else if(points.Length > 3) {
					Calculus.CalcPlaneEquation(points,out A,out B,out C,out D,verticalAxis);
				}
				CalcPlaneSizeRotation(A,B,C,D);
			}
			if(updatePeriod <= Time.deltaTime){//this shouldn't happen(the recalculation of plane,shouldn't take so often)
				yield return null;
			}
			else{
				yield return new WaitForSeconds(updatePeriod);
			}
		}
	}
	
}
