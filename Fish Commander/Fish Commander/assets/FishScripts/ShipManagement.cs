using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FormationType
{
	// 2D formations.
	Broad,
	Delta,
	Box,
	SoldBox,
	Chevrons //\\ 

	// 3D formations.
}

public class ShipManagement : MonoBehaviour {


	protected FormationType _formationType;
	protected int _squadronNumber;
	public bool _formationMode;


	//protected List<Vector3> _waypoints; // If waypoints are availible, then we might use them.

	public int SquadronNumber
	{
		get
		{
			return _squadronNumber;
		}
	}


	public Vector3 GetTarget
	{
		get
		{
			return targetPosition;
        }
	}

	public void SetSquadLeader(ShipManagement thisGuy)
	{
		_squadLeader = thisGuy;
    }

	public float spacing = 20;

	bool move = false;

	protected ShipManagement _squadLeader;

	private Vector3 targetPosition;

	// Use this for initialization
	void Start () {

		targetPosition = transform.position;
		_formationMode = false;
		_formationType = FormationType.Delta;
    }

	Vector3 targetHeading = Vector3.zero;

	public GameObject testFireTarget = null;

	// Update is called once per frame
	void Update()
	{
		if (move && _formationMode)
		{
			//FollowLeader();
			//FormateAtTarget();
			//MoveOperation(targetPosition);
		}
		//targetPosition = targetHeading;
	}

	public void InputCommands(int i, ShipManagement squadLeader, bool formationMode)
	{
		Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		float distance;

		if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Z))
		{
			Plane p = new Plane(Vector3.up, -transform.position.y);
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);


			if (p.Raycast(ray, out distance))
			{
				targetHeading = ray.GetPoint(distance);
			}
		}

		if (Input.GetKey(KeyCode.Z))
		{
			var pl = Camera.main.transform.position - targetHeading;
			Plane zp = new Plane(pl.normalized, pl.magnitude);
			Ray zray = camera.ScreenPointToRay(Input.mousePosition);
			if (zp.Raycast(zray, out distance))
			{
				var ytarget = zray.GetPoint(distance);
				targetHeading = new Vector3(targetHeading.x, ytarget.y, targetHeading.z);
			}
		}

		if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
		{
			move = false;
			_formationMode = formationMode;

			//turning this into an abstract or interface will help.
			if (!_formationMode)
			{
				MoveOperation(targetHeading);
			}
			else
			{
				SetDestination(i, squadLeader);

				//FollowLeader();
				FormateAtTarget();
				MoveOperation(targetHeading, squadLeader);

				//targetHeading = targetPosition;
			}
			
			targetPosition = targetHeading;
		}
		else
		// its possible that there might be interference from the Update method.
		{
			move = true;
		}
	}

	public void SetDestination(int squadNum, ShipManagement squadLeader = null)
	{

		_squadronNumber = squadNum;

		if (squadLeader != null)
		{
			_squadLeader = squadLeader;
        }

		//idk why I'm doing this. kind of redundant.
		//targetPosition = targetHeading;
	}

	protected void FollowLeader()
	{

		// Default is broad formation since its easy.
		// Then delta, we'll try 3d stuff later.
		if (_formationMode && _squadLeader != null)
		{
			if (_formationType == FormationType.Broad)
			{
				Vector3 projectedToPlane = Vector3.ProjectOnPlane(_squadLeader.transform.right, Vector3.up);

                if (_squadronNumber % 2 == 1)
				{
					targetPosition = _squadLeader.transform.position + projectedToPlane * ((_squadronNumber + 1) / 2) * spacing;
				}
				else
				{
					targetPosition = _squadLeader.transform.position + projectedToPlane * -(_squadronNumber / 2) * spacing;
				}
			}

			if (_formationType == FormationType.Delta)
			{
				if (_squadronNumber % 2 == 1)
				{
					targetPosition = _squadLeader.transform.position
						+ _squadLeader.transform.right * ((_squadronNumber + 1) / 2) * spacing
						- _squadLeader.transform.forward * ((_squadronNumber + 1) / 2) * spacing;
				}
				else
				{
					targetPosition = _squadLeader.transform.position
						+ _squadLeader.transform.right * -(_squadronNumber / 2) * spacing
						- _squadLeader.transform.forward * (_squadronNumber / 2) * spacing;
				}
			}
		}
	}

	protected void FormateAtTarget()
	{

		// Default is broad formation since its easy.
		// Then delta, we'll try 3d stuff later.
		if (_formationMode && _squadLeader != null)
		{
			if (_formationType == FormationType.Broad)
			{
				Vector3 projectedToPlane = Vector3.ProjectOnPlane(_squadLeader.transform.right, Vector3.up);

				//Vector3 projectedToPlane =
				//	Vector3.ProjectOnPlane((targetPosition- _squadLeader.transform.position).normalized,Vector3.up);

				if (_squadronNumber % 2 == 1)
				{
					targetHeading = targetHeading + projectedToPlane * ((_squadronNumber + 1) / 2) * spacing;
				}
				else
				{
					targetHeading = targetHeading + projectedToPlane * -(_squadronNumber / 2) * spacing;
				}
			}

			if (_formationType == FormationType.Delta)
			{

				//Vector3 projectedToPlane =
				//	Vector3.ProjectOnPlane(
				//		Vector3.Cross((targetPosition - _squadLeader.transform.position).normalized,Vector3.up),
				//		Vector3.up);

				Vector3 projectedToPlane = Vector3.ProjectOnPlane(_squadLeader.transform.right, Vector3.up);

				Vector3 rigthRelative = Vector3.Cross((_squadLeader.GetTarget - _squadLeader.transform.position).normalized, Vector3.up);
				Vector3 forwardRelative = (_squadLeader.GetTarget - _squadLeader.transform.position).normalized;
				if (_squadronNumber % 2 == 1)
				{
					targetHeading = _squadLeader.GetTarget
						+ rigthRelative * ((_squadronNumber + 1) / 2) * spacing
						- forwardRelative * ((_squadronNumber + 1) / 2) * spacing;
				}
				else
				{
					targetHeading = _squadLeader.GetTarget
						+ rigthRelative * -(_squadronNumber / 2) * spacing
						- forwardRelative * (_squadronNumber / 2) * spacing;
				}
			}
		}
	}

	void MoveOperation(Vector3 targetPosition, ShipManagement squadLeader = null)
	{
		// Insert fish scripts here.
		var simpleMove = GetComponent<SimpleMove>();

		if (simpleMove != null) {
			simpleMove.Move (targetPosition);
		}
	}
}
