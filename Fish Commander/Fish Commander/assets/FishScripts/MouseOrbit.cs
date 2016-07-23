using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbit : MonoBehaviour
{

	public Transform target;
	public float distance = 40.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;

	public float upDownSpeed = 5;
	public float orientationThresh = 20;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	public float distanceMin = .5f;
	public float distanceMax = 15f;

	private Rigidbody rigidbody;

	float x = 0.0f;
	float y = 0.0f;

	public float zoomStepSize = 10;

	Quaternion _rotation;

	Material lineMat;

	public Transform lockTarget = null;

	// Use this for initialization
	void Start()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

		rigidbody = GetComponent<Rigidbody>();

		// Make the rigid body not change rotation
		if (rigidbody != null)
		{
			rigidbody.freezeRotation = true;
		}

		x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
		y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

		y = ClampAngle(y, yMinLimit, yMaxLimit);

		_rotation = Quaternion.Euler(y, x, 0);

		UpdateCamera();

		lineMat = Resources.Load("LineMat") as Material;
    }

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.F))
		{
			var guiOverlay = GetComponent<GUIOverlay>();
			if (guiOverlay != null)
			{
				var spaceObjs = guiOverlay.SpaceObjects;
				if (guiOverlay.SpaceObjects.Count > 0)
				{
					//consider if more than one object is availible in the list, we might want an average position of all those objects.
					lockTarget = guiOverlay.SpaceObjects[0].transform;
				}
			}
        }

		// This allows the camera to lock on a target.
		if(lockTarget!= null)
		{
			target.transform.position = lockTarget.transform.position;
		}

		Pan();
	}

	void LateUpdate()
	{
		if (target && Input.GetMouseButton(1))
		{
			x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

			y = ClampAngle(y, yMinLimit, yMaxLimit);
		}

		distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomStepSize, distanceMin, distanceMax);

		if(distance < orientationThresh && lockTarget != null)
		{
			_rotation = lockTarget.localRotation * Quaternion.Euler(y, x, 0);
		}
		else
		{
			_rotation = Quaternion.Euler(y, x, 0);
		}

		UpdateCamera();
	}

	void UpdateCamera()
	{
		//RaycastHit hit;
		//if (Physics.Linecast(target.position, transform.position, out hit))
		//{
		//	distance -= hit.distance;
		//}

		//this calculated position allows us to determing where the camera lies in orbit.
		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
		Vector3 position = _rotation * negDistance + target.position;

		transform.rotation = _rotation;
		transform.position = position;
	}

	void Pan()
	{
		//This operation figures out the relative area of the camera vector projected on to the XZ base plane.
		Vector3 facingDirection = target.position - transform.position;
		
		//This is the forward vector projected on the plane.
		Vector3 projectedDirection = Vector3.ProjectOnPlane(facingDirection, Vector3.up);

		//This is the right vector on the plane. 
		//Note that the vector are not normalized, thus speed of pan is tied to distance from camera to target. ;)
		Vector3 rightProjectedDir = Vector3.Cross(projectedDirection, Vector3.up);

		if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0 )
		{
			lockTarget = null;
		}

		float forwardVal = Input.GetAxis("Vertical") * Time.deltaTime;
		float rightVal = -Input.GetAxis("Horizontal") * Time.deltaTime;


		float upVal = Input.GetAxis("Lateral") * Time.deltaTime;

		//Debug.Log("movingRight " + forwardVal + projectedDirection);
		//Vector3.Cross(projectedDirection, Vector3.right)
		target.transform.Translate(projectedDirection * forwardVal);
		target.transform.Translate(rightProjectedDir * rightVal);

		target.transform.Translate(Vector3.up * upDownSpeed * upVal);

		//Debug.DrawLine(target.position + target.forward, target.position - target.forward, Color.green);
		//Debug.DrawLine(target.position + target.right, target.position - target.right, Color.green);
	}


	private void OnPostRender()
	{
		//DrawLine(target.position + target.forward, target.position - target.forward);
		//DrawLine(target.position + target.right, target.position - target.right);
	}

    void DrawLine(Vector3 p1, Vector3 p2)
	{
		lineMat.SetPass(0);
		GL.Begin(GL.LINES);

		GL.Color(Color.red);

		GL.Vertex3(p1.x, p1.y, p1.z);
		GL.Vertex3(p2.x, p2.y, p2.z);

		GL.End();
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
}