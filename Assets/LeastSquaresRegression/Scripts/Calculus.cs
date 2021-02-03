using UnityEngine;
using System.Collections.Generic;

public static class Calculus
{

	public static void CalcPlaneEquation(Vector3[] rawPoints, out float nX, out float nY, out float nZ, out float D, RegressionPlane.Axes axis)
	{

		Vector3[] points;
		if (axis == RegressionPlane.Axes.verticalX)
		{
			points = new Vector3[rawPoints.Length];
			for(int i = 0; i < rawPoints.Length; ++i){
				points[i] = new Vector3(rawPoints[i].z, -rawPoints[i].x, -rawPoints[i].y);
			}
		}
		else if (axis == RegressionPlane.Axes.verticalY) 
		{
			points = new Vector3[rawPoints.Length];
			for(int i = 0; i < rawPoints.Length; ++i)
			{
				points[i] = new Vector3(rawPoints[i].x, rawPoints[i].z, -rawPoints[i].y);
			}
		}
		else
		{
			points = rawPoints;
		}

		int numOfPoints = points.Length;
		nX = 0;
		nY = 0;
		nZ = 0;
		D = 0;

		if(numOfPoints > 3){
			//the system of linear equations is overdetermined (the unknowns are a,b,c , so n=3 , m=numPoints)
			// { a*x1 + b*y1 + c = z1
			// { a*x2 + b*y2 + c = z2
			//   ....................
			// { a*xm + b*ym + c = zm
			
			// A*v3 = vm;
			// v3 is coloumn vector with the values a,b,c
			// vm is coloumn vector with the values z1,z2, ... , zm
			
			//A is a matrix with size 3,numOfPoints(it could be defined like this,however it is not needed to store it )
			
			//for(unsigned i = 0 ; i < numPoints; ++i){
			//	A(i,0) = x[i]; A(i,1) = y[i]; A(i,2) = 1;
			//}
			
			// A*v3 = vm ,has no exact solution , but it has to be found the v3 ,such that the 2-norm of the rezidual vector ( ||vm - A*v3||2 ) is minimum 
			// let it be the quadratic function f(v3) = ||vm - A*v3||2
			// normally there is no solution for f(v3) = 0 , but we're looking to find the v3 so that f(v3) is minimum 
			// if matrix rank of A is n ( in this case 3) , then there is only one solution for v3 so that f(v3) has minimum value(why is this like that and not differently is out of scope to demonstrate),
			// by solving the below system: 
			// At*A*v3 = At*vm ( where At is transpose of A) , At*A is positive definite and symmetric matrix

			if(false == matrixRankIs3(points)){
				Debug.Log("Infinite number of solutions !");
				return;
			}
			
			// if the At * A is calculated,the result is a 3x3 matrix(symmetric matrix) , so consider the below matrix being named AtA 
			// a11 a12 a13
			// a12 a22 a23
			// a13 a23 a33
			
			float a11 = 0;
			float a12 = 0;
			float a13 = 0;
			float a22 = 0;
			float a23 = 0;
			float a33 = 0;

			int kk =  0; 
			foreach (Vector3 point in points)
			{
				a11 += point.x * point.x;
				a12 += point.x * point.y;
				a13 += point.x;
				a22 += point.y * point.y;
				a23 += point.y;
				kk += 1;
			}
			a33 = numOfPoints;
			
			// now it's time decompose AtA by Cholesky (remember the AtA is positive definite and symmetric matrix)
			// AtA = Lt * L; // L is superior triangular matrix ( Lt is transpose)
			// a11 a12 a13   l11  0   0      l11 l21 l31
			// a12 a22 a23 = l21 l22  0   *   0  l22 l32
			// a13 a23 a33   l31 l32 l33      0   0  l33
			
			// this will result in :
			
			// a11 = l11 * l11
			// a12 = l11 * l21
			// a13 = l11 * l31
			// a22 = l21*l21 + l22*l22
			// a23 = l21*l31 + l22*l32
			// a33 = l31*l31 + l32*l32 + l33*l33
			
			float l11 = Mathf.Sqrt(a11);
			float l21 = a12 / l11;
			float l31 = a13 / l11;
			float l22 = Mathf.Sqrt(a22 - l21*l21);
			float l32 = (a23 - l21*l31) / l22;
			float l33 = Mathf.Sqrt(a33 - l31*l31 - l32*l32);
			
			//remember we had to solve At*A*v3 = At*vm ,which is now Lt*L*v3 = At*vm
			//let it be w = L*v3 ,then Lt*w = At*vm , so we have to find which is w (a column vector with 3 elements)
			//At*vm is equal with column vector ( t1,t2,t3) with the below values:
			// x1*z1 + x2*z2 + ... + xm*zm
			// y1*z1 + y2*z2 + ... + ym*zm
			// z1 + z2 + ... + zm
			
			float t1 = 0;
			float t2 = 0;
			float t3 = 0;
			
			foreach(Vector3 point in points){
				t1 += point.x*point.z;
				t2 += point.y*point.z;
				t3 += point.z;
			}
			
			//we now have to solve:
			// l11*w1 = t1
			// l21*w1 + l22*w2 = t2
			// l31*w1 + l32*w2 + l33*w3 = t3
			
			float w1 = t1 / l11;
			float w2 = (t2 - l21*w1) / l22;
			float w3 = (t3 - l31*w1 - l32*w2) / l33;
			
			//last phase now , L*v3 = w , find v3(the values a ,b ,c)
			// a*l11 + b*l21 + c*l31 = w1
			//         b*l22 + c*l32 = w2
			//                 c*l33 = w3
			
			float c = w3 / l33;
			float b = (w2 - c*l32) / l22;
			float a = (w1 - b*l21 - c*l31) / l11;
			
			//remember that the plane equation is A*x + B*y + C*z + D = 0 (and normal is defined by A,B,C or in our case the reference arguments nx,ny,nz )
			//depending on the main vertical axis ,the plane equation is deducted from a,b,c values
			//we have a*xi + b*yi + c = zi

			if (axis == RegressionPlane.Axes.verticalX) {
				nX = -1;
				nY = -a;
				nZ = -b;
			}
			else if(axis == RegressionPlane.Axes.verticalY) {
				nX = a;
				nY = 1;
				nZ = b;
			}
			else {
				nX = a;
				nY = b;
				nZ = -1;
			}
			D = c;

			Debug.Log("Plane equation:"+nX+"*x "+numberWithSign(nY)+"*y "+numberWithSign(nZ)+"*z "+numberWithSign(D)+"= 0");
		}	
	}

	static string numberWithSign(float number)
	{
		return ((number < 0)?"":"+")+number;
	}

	static bool matrixRankIs3(Vector3[] points)
	{
		int i = 0;
		int j = 0;
		int k = 0;
		int n = points.Length;

		for(i = 0; i < n-2; ++i){
			for(j = 1; j < n-1; ++j){
				for(k = 2; k < n; ++k){
					if(false == Mathf.Approximately(matrixDeterminant(points[i].x,points[i].y,1,
					                                                  points[j].x,points[j].y,1,
					                                                  points[k].x,points[k].y,1),0)){
						return true;
					}
				}
			}
		}
		return false;
	}

	//calculate the 3D projection(closest point) on a line from a given point,
	//the line is defined by a vector(lineAxis) and a random point(Pr) on it
	public static Vector3 pointToLinePrj(Vector3 point ,Vector3 lineAxis, Vector3 Pr)
	{
		float a11 = 0;
		float a12 = 0;
		float a13 = 0;
		float a21 = 0;
		float a22 = 0;
		float a23 = 0;
		float a31 = lineAxis.x;
		float a32 = lineAxis.y;
		float a33 = lineAxis.z;
		float b1 = 0;
		float b2 = 0;
		float b3 = point.x * lineAxis.x + point.y * lineAxis.y + point.z * lineAxis.z;
		
		//TODO test it with different inputs ... to include all paths
		if(!Mathf.Approximately(lineAxis.x,0))
		{
			a11 = lineAxis.y;
			a12 = -lineAxis.x;
			b1 = Pr.x * lineAxis.y - Pr.y * lineAxis.x;
			a21 = lineAxis.z;
			a23 = -lineAxis.x;
			b2 = Pr.x * lineAxis.z - Pr.z * lineAxis.x;
		} 
		else if(!Mathf.Approximately(lineAxis.y,0))
		{
			a11 = lineAxis.y;
			b1 = Pr.x * lineAxis.y;
			a22 = lineAxis.z;
			a23 = -lineAxis.y;
			b2 = Pr.y * lineAxis.z - Pr.z * lineAxis.y;
		} 
		else if (!Mathf.Approximately(lineAxis.z,0))
		{
			return new Vector3(Pr.x, Pr.y, point.z);
		}

		return solveCramer(a11, a12, a13, a21, a22, a23, a31, a32, a33, b1, b2, b3);
	}
	
	static float matrixDeterminant(float a11, float a12, float a13, float a21, float a22, float a23, float a31, float a32, float a33)
	{
		return (a11*a22*a33 + a21*a32*a13 + a31*a12*a23 - a11*a23*a32 - a12*a21*a33 - a13*a22*a31);
	}
	
	static Vector3 solveCramer(float a11, float a12, float a13, float a21, float a22, float a23, float a31, float a32, float a33, float b1, float b2, float b3)
	{
		float denom = matrixDeterminant (a11, a12, a13, a21, a22, a23, a31, a32, a33);

		if (Mathf.Approximately(denom, 0)) 
		{
			return Vector3.zero;
		}

		return new Vector3 (matrixDeterminant(b1,  a12, a13, b2,  a22, a23, b3,  a32, a33) / denom,
		                    matrixDeterminant(a11, b1,  a13, a21, b2,  a23, a31, b3,  a33) / denom,
		                    matrixDeterminant(a11, a12, b1,  a21, a22, b2,  a31, a32, b3)  / denom);
	}
}
