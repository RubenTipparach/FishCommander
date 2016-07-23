using UnityEngine;
using System.Collections;

public abstract class BasicLine : MonoBehaviour
{

	public Material lineMaterial;

	void OnPostRender()
	{
		Overlay();
	}


	private void Overlay()
	{
		lineMaterial.SetPass(0);
		GL.Color(Color.red);

		GL.Begin(GL.LINES);
		DrawLines();
		GL.End();
	}

	protected void DrawLine(Vector3 source, Vector3 destination)
	{
		GL.Vertex3(source.x, source.y, source.z);
		GL.Vertex3(destination.x, destination.y, destination.z);
	}

	public abstract void DrawLines();
}
