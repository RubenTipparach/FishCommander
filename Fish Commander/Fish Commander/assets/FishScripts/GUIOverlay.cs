using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIOverlay : MonoBehaviour {
	bool InSelectState;
	Vector2 startRect;

	List<SpaceObject> shipsSelected;
	List<ScreenInfo> screenInfos;

	public GUISkin customGuiSkin;

	Material lineMat;
	bool autoShowHeight = false;
	bool toggleTimeFreeze;


	public List<SpaceObject> SpaceObjects
	{
		get
		{
			return shipsSelected;
		}
	}
	
	// Use this for initialization
	void Start () {
		shipsSelected = new List<SpaceObject>();
		screenInfos = new List<ScreenInfo>();
		lineMat = Resources.Load("LineMat") as Material;
		toggleTimeFreeze = false;
    }
	
	// Update is called once per frame
	void Update () {

		//Toggle Height overlay
		if (Input.GetKeyDown(KeyCode.H))
		{
			if (!autoShowHeight)
			{
				autoShowHeight = true;
			}
			else
			{
				autoShowHeight = false;
			}
		}

		if (Input.GetKeyDown(KeyCode.P))
		{
			if(toggleTimeFreeze)
			{
				toggleTimeFreeze = false;
			}
			else
			{
				toggleTimeFreeze = true;
			}
		}

		HandleShipOrders();
	}
	
	void OnGUI()
	{
		GUI.skin = customGuiSkin;

		MouseSelectProc();

		//Moved to postrender
		DisplaySelectedObjects();
    }

	// This procedure is to be used to select objects in game.
	private void MouseSelectProc()
	{
		Rect t = new Rect(0, 0, Screen.width, Screen.height);
		GUI.BeginGroup(t);
		//GUI.contentColor = Color.green;

		Vector2 currentMousePosition = Input.mousePosition;
		List<SpaceObject> tempSelected = new List<SpaceObject>();

		//Cast ray to draw ship's names.
		Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			var spaceObj = hit.transform.gameObject.GetComponent<SpaceObject>();
            if (spaceObj != null)
			{
				SwitchGUIColor(spaceObj.Transponder);
				Vector2 screenPos = GetComponent<Camera>().WorldToScreenPoint(hit.transform.position);
                GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, spaceObj.ObjName.Length * 10, 20), spaceObj.ObjName);
            }
		}

		// Step 1 mouse is pressed.
		if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift))
		{
			InSelectState = true;
			startRect = Input.mousePosition;

			//Test ray to intersect friendly ships.
			Ray ray2 = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			RaycastHit hit2;
			if (Physics.Raycast(ray, out hit2))
			{
				//if (hit.collider.gameObject.GetComponent<SpaceObject>().ShipTransponder == ShipTag.Friendly)
				//{
				//	InSelectState = false;
				//	shipsSelected.Clear();
				//	shipsSelected.Add(hit.collider.gameObject);
				//}
				if (hit2.transform.gameObject.GetComponent<SpaceObject>() != null)
				{
					InSelectState = false;
					shipsSelected.Clear();
					shipsSelected.Add(hit2.transform.GetComponent< SpaceObject>());
				}
			}
		}

		// Step 2 mouse is being held down.
		if (Input.GetMouseButton(0) && InSelectState)
		{
			Rect selection = GetSelectionRect(startRect, currentMousePosition);
			//GUI.color = Color.cyan;
			GUI.Box(selection, string.Empty);

			// Do the selection logic here
			Object[] gb = GameObject.FindObjectsOfType(typeof(SpaceObject));
			foreach (Object o in gb)
			{
				
				GameObject obj = GameObject.Find(o.name);
				Vector2 screenPos = GetComponent<Camera>().WorldToScreenPoint(obj.transform.position);
				//Debug.Log(screenPos);

				if (obj.name != "Main Camera")
				{
					screenPos.y = Screen.height - screenPos.y;
					//If we hit the plane, set move to that location.
                    if (obj.GetComponent<Renderer>().isVisible
						//&& (Screen.width - screenPos.x) < Screen.width 
						//&& (Screen.height - screenPos.y)< Screen.height
						&& selection.Contains(screenPos)
						//&& (obj.GetComponent<SpaceObject>().Transponder == TransponderType.Friendly || obj.GetComponent<SpaceObject>().Transponder == TransponderType.Own))
						&& obj.GetComponent<SpaceObject>().Transponder == TransponderType.Own)
					{
						tempSelected.Add(obj.GetComponent<SpaceObject>());
						//GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, obj.name.Length * 10, 20), obj.name);
					}
				}
			}

			shipsSelected = tempSelected;
		}

		// Step 3 on mouse up select objects in rectangle set the select state to false.
		if (Input.GetMouseButtonUp(0))
		{
			InSelectState = false;
		}

		GUI.EndGroup();
	}

	private void HandleShipOrders()
	{
		int i = 0;
		ShipManagement squadLeader = null;

		// Do the selection logic here
		SpaceObject[] gb = shipsSelected.ToArray();
		bool formationMode = false;

		if (gb.Length > 1)
		{
			formationMode = true;
		}

		foreach (SpaceObject o in gb)
		{
			if (o != null)
			{
				var sm = o.transform.GetComponent<ShipManagement>();

				if (sm != null)
				{
					// Each ship selected has a method that listens for commands.
					// Basic commands are move and fire.
					sm.InputCommands(i, squadLeader, formationMode);
					//sm.SetDestination(i, squadLeader);
					
                    if (i == 0)
					{
						squadLeader = sm;
					}

					i++;
				}
			}
		}
	}

	private void OnPostRender()
	{
		DisplayLinesSelectedObjects();
		DrawObjectHeights();
    }

	/// <summary>
	/// Displays the selected space objects.
	/// </summary>
	void DisplaySelectedObjects()
	{
		// Do the selection logic here
		SpaceObject[] gb = shipsSelected.ToArray();
		foreach (SpaceObject o in gb)
		{
			if (o != null)
			{
				Vector2 screenPos = this.GetComponent<Camera>().WorldToScreenPoint(o.transform.position);

				if (o.transform.GetComponent<Renderer>().isVisible
					&& screenPos.x < Screen.width
					&& Screen.height - screenPos.y < Screen.height)
				{
					SwitchGUIColor(o.Transponder);
                    GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, o.ObjName.Length * 10, 20), o.ObjName);
				}
			}
		}
	}

	void DisplayLinesSelectedObjects()
	{
		// Do the selection logic here
		SpaceObject[] gb = shipsSelected.ToArray();
		foreach (SpaceObject o in gb)
		{
			if (o != null)
			{			
				// Display heading.
				/*
				var propulsionSystem = o.transform.GetComponent<ShipImpulseForcePID>();
				if (propulsionSystem != null)
				{
					LineLibrary.LineDrawing(lineMat, Color.green, o.transform.position, propulsionSystem.GetTargetPosition);
				}
				*/
			}
		}
	}

	void DrawObjectHeights()
	{
		SpaceObject[] gb = shipsSelected.ToArray();

		SpaceObject so = null;

		if(gb.Length > 0)
		{
			so = gb[0];
		}

        if (autoShowHeight)
		{
			gb = GameObject.FindObjectsOfType<SpaceObject>();
			foreach (var g in gb)
			{
				var zxPlaneProject = new Vector3(g.transform.position.x, 0, g.transform.position.z);

				LineLibrary.LineDrawing(lineMat, Color.green, g.transform.position, zxPlaneProject);
				LineLibrary.CircleDrawing(lineMat, Color.cyan, 16, .5f, zxPlaneProject, Quaternion.Euler(Vector3.up));

				if (g.transform.GetComponent<ShipManagement>() != null)
				{
					var sm = g.transform.GetComponent<ShipManagement>();
					zxPlaneProject = new Vector3(sm.GetTarget.x, 0, sm.GetTarget.z);

					LineLibrary.LineDrawing(lineMat, Color.green, sm.transform.position, sm.GetTarget);
					LineLibrary.LineDrawing(lineMat, Color.blue, zxPlaneProject, sm.GetTarget);
					LineLibrary.CircleDrawing(lineMat, Color.blue, 16, .5f, zxPlaneProject, Quaternion.Euler(Vector3.up));
				}
			}
		}
		else
		{
			foreach (SpaceObject o in gb)
			{
				so = o;
				if (o != null)
				{
					// Display heading.
					var zxPlaneProject = new Vector3(o.transform.position.x, 0, o.transform.position.z);

					LineLibrary.LineDrawing(lineMat, Color.green, o.transform.position, zxPlaneProject);
					LineLibrary.CircleDrawing(lineMat, Color.cyan, 16,.5f, zxPlaneProject, Quaternion.Euler(Vector3.up));

					if (o.transform.GetComponent<ShipManagement>() != null)
					{
						var sm = o.transform.GetComponent<ShipManagement>().GetTarget;
						zxPlaneProject = new Vector3(sm.x, 0, sm.z);

						LineLibrary.LineDrawing(lineMat, Color.blue, zxPlaneProject, sm);
						LineLibrary.CircleDrawing(lineMat, Color.gray, 16, .5f, zxPlaneProject, Quaternion.Euler(Vector3.up));
					}
				}
			}
		}

		//as before maybe do some logic to average this.
		if (so != null)
		{
			DrawDestinations(so.transform.position);
        }
	}

	Vector3 targetHeading = Vector3.zero;
	//bool moveInYAxis = false;

	void DrawDestinations(Vector3 preHeight)
	{
		Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		float distance;

		if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Z))
		{
			Plane p = new Plane(Vector3.up, -preHeight.y);
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

		if (Input.GetKey(KeyCode.LeftShift))
		{
			RayCastOperation(targetHeading, preHeight);
		}
	}

	void RayCastOperation(Vector3 targetHeading, Vector3 avgShipsPos)
	{
		var zxPlaneProject = new Vector3(targetHeading.x, 0, targetHeading.z);

		LineLibrary.LineDrawing(lineMat, Color.cyan, targetHeading + Vector3.forward, targetHeading - Vector3.forward);
		LineLibrary.LineDrawing(lineMat, Color.cyan, targetHeading + Vector3.right, targetHeading - Vector3.right);

		LineLibrary.LineDrawing(lineMat, Color.green, targetHeading, zxPlaneProject);
		LineLibrary.CircleDrawing(lineMat, Color.cyan, 16, 2, zxPlaneProject, Quaternion.Euler(Vector3.up));

		LineLibrary.LineDrawing(lineMat, Color.yellow, targetHeading, avgShipsPos);
		Vector3 xzPlaneCoord = new Vector3(targetHeading.x, avgShipsPos.y, targetHeading.z);
        LineLibrary.CircleDrawing(lineMat, Color.blue, 32, (avgShipsPos - xzPlaneCoord).magnitude
			, avgShipsPos, Quaternion.Euler(Vector3.up));
	}

	private void SwitchGUIColor(TransponderType t)
	{
		switch (t)
		{
			case TransponderType.Enemy:
				GUI.contentColor = Color.red;
				break;
			case TransponderType.Own:
				GUI.contentColor = Color.green;
				break;
			case TransponderType.Nuetral:
				GUI.contentColor = Color.cyan;
				break;
			case TransponderType.Friendly:
				GUI.contentColor = Color.yellow;
				break;
			default:
				GUI.contentColor = Color.cyan;
				break;
		}
	}

	private Rect GetSelectionRect(Vector2 start, Vector2 end)
	{
		int width = (int)(end.x - start.x);
		int height = (int)((Screen.height - end.y) - (Screen.height - start.y));

		if (width < 0 && height < 0)
		{
			return (new Rect(end.x, Screen.height - end.y, Mathf.Abs(width), Mathf.Abs(height)));
		}
		else if (width < 0)
		{
			return (new Rect(end.x, Screen.height - start.y, Mathf.Abs(width), height));
		}
		else if (height < 0)
		{
			return (new Rect(start.x, Screen.height - end.y, width, Mathf.Abs(height)));
		}
		else
		{
			return (new Rect(start.x, Screen.height - start.y, width, height));
		}
	}

	public struct ScreenInfo
	{
		public string Info;

		public Vector2 ScreenPos;

		public ScreenInfo(string info, Vector2 screenPos)
		{
			Info = info;
			ScreenPos = screenPos;
		}
	}
}
