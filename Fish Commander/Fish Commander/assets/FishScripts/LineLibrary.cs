using UnityEngine;
using System.Collections;

public static class LineLibrary
{

	//public Material lineMaterial;

	//example call
	//void OnPostRender()
	//{
	//	LineDrawing();
	//}


	public static void LineDrawing(Material lineMaterial, Color color, DrawLines lineDrawer)
	{
		lineMaterial.SetPass(0);
		GL.Color(color);

		GL.Begin(GL.LINES);

		// Implementation happens here.
		lineDrawer();

		GL.End();
	}


	public static void LineDrawing(Material lineMaterial, Color color, Vector3 source, Vector3 destination)
	{
		//lineMaterial.color = Color.red;
		lineMaterial.SetColor("_TintColor", color);
		lineMaterial.SetPass(0);
		//GL.Color(Color.yellow);

		GL.Begin(GL.LINES);

		// Implementation happens here.
		GL.Vertex3(source.x, source.y, source.z);
		GL.Vertex3(destination.x, destination.y, destination.z);

		GL.End();
	}

	/// <summary>
	/// Not done yet.
	/// </summary>
	/// <param name="lineMaterial"></param>
	/// <param name="color"></param>
	/// <param name="source"></param>
	/// <param name="destination"></param>
	public static void PathDrawing(Material lineMaterial, Color color, Vector3 source, Vector3 destination)
	{
	}


	/// <summary>
	/// Not done yet.
	/// </summary>
	/// <param name="lineMaterial"></param>
	/// <param name="color"></param>
	/// <param name="source"></param>
	/// <param name="destination"></param>
	public static void CircleDrawing(Material lineMaterial, Color color, int iteration, float radius, Vector3 s, Quaternion q)
	{
		lineMaterial.SetColor("_TintColor", color);
		lineMaterial.SetPass(0);

		GL.Begin(GL.LINES);
		for (int i = 1; i <= iteration + 1; i++)
		{
			float dg = i;
			float n = iteration;

			Vector3 v = Vector3.zero;

			v.x = radius * Mathf.Cos((dg / n) * Mathf.PI * 2);
			v.z = radius * Mathf.Sin((dg / n) * Mathf.PI * 2);

			v = q * v;

			Vector3 v1 = new Vector3(v.x + s.x, v.y + s.y, v.z + s.z);

			GL.Vertex3(v1.x, v1.y, v1.z);

			if (i != 1 && i != iteration + 1)
				GL.Vertex3(v.x + s.x, v.y + s.y, v.z + s.z);

		}
		GL.End();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="lineMaterial"></param>
	/// <param name="color"></param>
	/// <param name="source"></param>
	/// <param name="destination"></param>
	public static void BoxDrawing(Material lineMaterial, Color color, float width, float height, Vector3 position)
	{
		lineMaterial.SetColor("_TintColor", color);
		lineMaterial.SetPass(0);

		GL.Begin(GL.LINES);

		// upper width line
		GL.Vertex3(position.x - width / 2, position.y, position.z + height / 2);
		GL.Vertex3(position.x + width / 2, position.y, position.z + height / 2);

		// right height line
		GL.Vertex3(position.x + width / 2, position.y, position.z + height / 2);
		GL.Vertex3(position.x + width / 2, position.y, position.z - height / 2);

		// lower width line
		GL.Vertex3(position.x + width / 2, position.y, position.z - height / 2);
		GL.Vertex3(position.x - width / 2, position.y, position.z - height / 2);

		//height lines
		GL.Vertex3(position.x - width / 2, position.y, position.z - height / 2);
		GL.Vertex3(position.x - width / 2, position.y, position.z + height / 2);

		GL.End();
	}


	public static void DrawRelativeHorizontalCircle(int iteration, float radius, Vector3 s, Quaternion q)
	{
		GL.Color(new Color(1f, .1f, .5f, .5f));
		for (int i = 1; i <= iteration + 1; i++)
		{
			float dg = i;
			float n = iteration;

			Vector3 v = Vector3.zero;

			v.x = radius * Mathf.Cos((dg / n) * Mathf.PI * 2);
			v.z = radius * Mathf.Sin((dg / n) * Mathf.PI * 2);

			v = q * v;

			Vector3 v1 = new Vector3(v.x + s.x, v.y + s.y, v.z + s.z);

			GL.Vertex3(v1.x, v1.y, v1.z);

			if (i != 1 && i != iteration + 1)
				GL.Vertex3(v.x + s.x, v.y + s.y, v.z + s.z);

		}
	}

	public delegate void DrawLines();
	//public abstract void DrawLines();
}
