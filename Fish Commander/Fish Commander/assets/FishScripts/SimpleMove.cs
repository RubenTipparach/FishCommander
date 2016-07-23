using UnityEngine;
using System.Collections;

public class SimpleMove : MonoBehaviour {

	private Vector3 target = Vector3.zero;
	bool inMove = false;

	public void Move(Vector3 _target)
	{
		inMove = true;
		this.target = _target;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//we'll take an average of this.
		//Plane movePlane = new Plane(Vector3.up, transform.position.y);

		if ((target - transform.position).magnitude < .1f) {
			inMove = false;
		}

		if (inMove) {
			Quaternion lookat = Quaternion.LookRotation (target - transform.position);

			transform.rotation = Quaternion.Slerp (transform.rotation, lookat, .1f);

			var agent = GetComponent<RVOAgent> ();
			if (agent != null) {
				agent.preferedVelocity = transform.transform.forward * 2f;
				transform.position += ( agent.velocity * Time.deltaTime);
				agent.position = transform.position;
			} else {
				transform.Translate (Vector3.forward * Time.deltaTime, Space.Self);
			}
		}
	}
}
