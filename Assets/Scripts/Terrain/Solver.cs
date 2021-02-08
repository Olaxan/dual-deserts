using System.Collections.Generic;
using UnityEngine;

public class Solver 
{
	static Vector4 empty = new Vector4(0,0,0,1);

	public static bool SolveMatrix3(Matrix4x4 mat, Vector4 b, out Vector3 vertex)
	{

		var det = mat.determinant;

		if (Mathf.Abs(det) <= 1e-12)
		{
			vertex = Vector3.positiveInfinity;
			return false;
		}

		vertex = new Vector3
			(
				new Matrix4x4(b, mat.GetColumn(1), mat.GetColumn(2), empty).determinant,
				new Matrix4x4(mat.GetColumn(0), b, mat.GetColumn(2), empty).determinant,
				new Matrix4x4(mat.GetColumn(0), mat.GetColumn(1), b, empty).determinant
			) / det;

		return true;
	}

	public static bool LeastSquares(List<Vector3> A, List<float> b, out Vector3 vertex)
	{
		int N = A.Count;

		if (N == 3)
		{
			var mat = new Matrix4x4(A[0], A[1], A[2], empty);
			var vec = new Vector4(b[0], b[1], b[2], 0);
			return SolveMatrix3(mat, vec, out vertex);
		}

		var At_A = Matrix4x4.identity;
		var At_b = Vector4.zero;

		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float sum = 0;

				for (int k = 0; k < N; k++)
					sum += A[k][i] * A[k][j];

				At_A[i,j] = sum;
			}
		}

		for (int i = 0; i < 3; i++)
		{
			float sum = 0;

			for (int k = 0; k < N; k++)
				sum += A[k][i] * b[k];

			At_b[i] = sum;
		}

		return SolveMatrix3(At_A, At_b, out vertex);
	}
}
