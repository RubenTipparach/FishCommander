using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Line
{
	public Vector3 direction;
	public Vector3 point;
}

public class RVOAgent : MonoBehaviour {

	float RVO_EPSILON = 0.00001f;

	public List<KeyValuePair<float, RVOAgent>> agentNeighbors = new List<KeyValuePair<float, RVOAgent>> ();
	public List<RVOPlane> orcaPlanes = new List<RVOPlane>();

	public Vector3 newVelocity;	
	public Vector3 position;
	public Vector3 preferedVelocity;
	public Vector3 velocity;

	public int id = 0;
	public int maxNeighbors = 10;

	public float maxSpeed = 2f;
	public float neighborDist = 15;
	public float radius = 1.5f;
	public float timeHorizon = 10;

	//RVOSimulator Simulator = RVOSimulator.Instance;

	public GameObject kdTreeObject;
	RVOKdTree kdTree;

	void Start()
	{
		velocity = Vector3.zero;
		
		position = transform.position;
	}

	//careful with this, this is actually updating irl.
	void Update()
	{
		//cool works with actual update method :)
		//note, we might not need siumulator class if this works. but will need away to find agents in 3d space.
		velocity = newVelocity;
		kdTree = kdTreeObject.GetComponent<RVOKdTree>();

		if (kdTree != null)
		{
			computeNeighbors();
			computeNewVelocity();
		}

		//position += velocity * Time.deltaTime;
	}

	public void computeNeighbors()
	{
		agentNeighbors.Clear ();

		if (maxNeighbors > 0) {
			float nDistSq = neighborDist * neighborDist;
	
			kdTree.computeAgentNeighbors(this, ref nDistSq);
		}
	}

	public void computeNewVelocity ()
	{
		orcaPlanes.Clear();
		float invTimeHorizon = 1.0f / timeHorizon;

		// Creates agen ORCA planes
		for (int i = 0; i < agentNeighbors.Count; i++)
		{
			RVOAgent other = agentNeighbors[i].Value;

			Vector3 relativePosition = other.position - this.position;
			Vector3 relativeVelocity = velocity - other.velocity;

			float distSquared = relativePosition.sqrMagnitude;

			float combinedRadius = radius + other.radius;
			float combinedRadiusSq = RVOKdTree.Squared(combinedRadius);

			RVOPlane plane = new RVOPlane();
			Vector3 u = Vector3.zero;

			if(distSquared > combinedRadiusSq)
			{
				// No collision.
				Vector3 w = relativeVelocity - invTimeHorizon * relativePosition;
				// Vector from cutoff center to relative velocity.
				float wLengthSq = w.sqrMagnitude;

				float dotProduct = Vector3.Dot(w, relativePosition);

				if(dotProduct < 0.0f && RVOKdTree.Squared(dotProduct) > combinedRadius * wLengthSq)
				{
					// Project on cut-off circle.
					float wLength = Mathf.Sqrt(wLengthSq);
					Vector3 unitW = w / wLengthSq;

					plane.normal = unitW;
					u = (combinedRadius * invTimeHorizon - wLength) * unitW;
				}
				else
				{
					// Project on cone.
					float a = distSquared;
					//always remember when they do v*v it means dot.
					float b = Vector3.Dot(relativePosition, relativeVelocity);
					float c = relativeVelocity.sqrMagnitude - (Vector3.Cross(relativePosition, relativeVelocity)).sqrMagnitude / (distSquared - combinedRadiusSq);
					float t = (b + Mathf.Sqrt((b * b) - a * c)) / a; // whoa, the quadratic formula!
					Vector3 wRelative = relativeVelocity - t * relativePosition;
					float wLength = wRelative.magnitude;
					Vector3 unitW = wRelative / wLength;

					plane.normal = unitW;
					u = (combinedRadius * t - wLength) * unitW;
				}
			}
			else
			{
				//collision with our mathematical body
				float invTimeStep = 1.0f / Time.deltaTime; //delta time.
				Vector3 w = relativeVelocity - invTimeStep * relativePosition;
				float wLength = w.magnitude;
				Vector3 unitW = w / wLength; // maybe I should replace this with w.normalized later.

				plane.normal = unitW;
				u = (combinedRadius * invTimeStep - wLength) * unitW;
			}

			plane.point = velocity + 0.5f * u;
			orcaPlanes.Add(plane);
        }

		int planeFail = linearProgram3(orcaPlanes, maxSpeed, preferedVelocity, false, ref newVelocity);

		if(planeFail < orcaPlanes.Count)
		{
			linearProgram4(orcaPlanes, planeFail, maxSpeed, ref newVelocity);
		}
	}

	/// <summary>
	/// Inserts the agent neighbor.
	/// </summary>
	/// <param name="agent">The agent.</param>
	/// <param name="rangeSquared">The range squared.</param>
	public void insertAgentNeighbor(RVOAgent agent, ref float rangeSquared)
	{
		if(this != agent)
		{
			float distSq = (position - agent.position).sqrMagnitude;
			
			if (distSq < rangeSquared)
			{
				if (agentNeighbors.Count < maxNeighbors)
				{
					agentNeighbors.Add(new KeyValuePair<float, RVOAgent>(distSq, agent));
				}

				int i = agentNeighbors.Count - 1;

				while (i != 0 && distSq < agentNeighbors[i - 1].Key)
				{
					agentNeighbors[i] = agentNeighbors[i - 1];
					--i;
				}

				agentNeighbors[i] = new KeyValuePair<float, RVOAgent>(distSq, agent);

				if(agentNeighbors.Count == maxNeighbors)
				{
					rangeSquared = agentNeighbors[agentNeighbors.Count - 1].Key;
				}
			}
		}
	}


	/// <summary>
	/// Linears the program1.
	/// </summary>
	/// <param name="planes">The planes.</param>
	/// <param name="planeNo">The plane no.</param>
	/// <param name="line">The line.</param>
	/// <param name="radius">The radius.</param>
	/// <param name="optVelocity">The opt velocity.</param>
	/// <param name="directionOpt">if set to <c>true</c> [direction opt].</param>
	/// <param name="result">The result.</param>
	/// <returns></returns>
	bool linearProgram1(List<RVOPlane> planes, int planeNo, Line line, float radius, Vector3 optVelocity, bool directionOpt, Vector3 result)
	{
		float dotProduct = Vector3.Dot(line.point, line.direction);
		float discriminant = RVOKdTree.Squared(dotProduct) + RVOKdTree.Squared(radius) - line.point.sqrMagnitude;

		if (discriminant < 0.0f)
		{
			/* Max speed sphere fully invalidates line. */
			return false;
		}

		float sqrtDiscriminant = Mathf.Sqrt(discriminant);
		float tLeft = -dotProduct - sqrtDiscriminant;
		float tRight = -dotProduct + sqrtDiscriminant;

		for (int i = 0; i < planeNo; ++i)
		{
			float numerator = Vector3.Dot(planes[i].point - line.point, planes[i].normal);
			float denominator = Vector3.Dot(line.direction , planes[i].normal);

			if (RVOKdTree.Squared(denominator) <= RVO_EPSILON)
			{
				/* Lines line is (almost) parallel to plane i. */
				if (numerator > 0.0f)
				{
					return false;
				}
				else
				{
					continue;
				}
			}

			float t = numerator / denominator;

			if (denominator >= 0.0f)
			{
				/* Plane i bounds line on the left. */
				tLeft = Mathf.Max(tLeft, t);
			}
			else
			{
				/* Plane i bounds line on the right. */
				tRight = Mathf.Min(tRight, t);
			}

			if (tLeft > tRight)
			{
				return false;
			}
		}

		if (directionOpt)
		{
			/* Optimize direction. */
			if (Vector3.Dot(optVelocity, line.direction) > 0.0f)
			{
				/* Take right extreme. */
				result = line.point + tRight * line.direction;
			}
			else
			{
				/* Take left extreme. */
				result = line.point + tLeft * line.direction;
			}
		}
		else
		{
			/* Optimize closest point. */
			float t = Vector3.Dot(line.direction , (optVelocity - line.point));

			if (t < tLeft)
			{
				result = line.point + tLeft * line.direction;
			}
			else if (t > tRight)
			{
				result = line.point + tRight * line.direction;
			}
			else
			{
				result = line.point + t * line.direction;
			}
		}

		return true;
	}

	bool linearProgram2(List<RVOPlane> planes, int planeNo, float radius, Vector3 optVelocity, bool directionOpt, ref Vector3 result)
	{
		float planeDist = Vector3.Dot(planes[planeNo].point, planes[planeNo].normal);
		float planeDistSq = RVOKdTree.Squared(planeDist);
		float radiusSq = RVOKdTree.Squared(radius);

		if (planeDistSq > radiusSq) {
			/* Max speed sphere fully invalidates plane planeNo. */
			return false;
		}

		float planeRadiusSq = radiusSq - planeDistSq;

		Vector3 planeCenter = planeDist * planes[planeNo].normal;

			if (directionOpt) {
				/* Project direction optVelocity on plane planeNo. */
				Vector3 planeOptVelocity = optVelocity - Vector3.Dot(optVelocity, planes[planeNo].normal) * planes[planeNo].normal;
				float planeOptVelocityLengthSq = planeOptVelocity.sqrMagnitude;

				if (planeOptVelocityLengthSq <= RVO_EPSILON)
				{
					result = planeCenter;
				}
				else
				{
					result = planeCenter + Mathf.Sqrt(planeRadiusSq / planeOptVelocityLengthSq) * planeOptVelocity;
				}
			}
			else {
				/* Project point optVelocity on plane planeNo. */
				result = optVelocity + Vector3.Dot((planes[planeNo].point - optVelocity), planes[planeNo].normal) * planes[planeNo].normal;

				/* If outside planeCircle, project on planeCircle. */
				if (result.sqrMagnitude > radiusSq) {
					Vector3 planeResult = result - planeCenter;
					float planeResultLengthSq = planeResult.sqrMagnitude;
					result = planeCenter + Mathf.Sqrt(planeRadiusSq / planeResultLengthSq) * planeResult;
				}
			}

			for (int i = 0; i<planeNo; ++i) {
				if (Vector3.Dot(planes[i].normal, (planes[i].point - result)) > 0.0f)
				{
					/* Result does not satisfy constraint i. Compute new optimal result. */
					/* Compute intersection line of plane i and plane planeNo. */
					Vector3 crossProduct = Vector3.Cross(planes[i].normal, planes[planeNo].normal);

					if (crossProduct.sqrMagnitude <= RVO_EPSILON) {
						/* Planes planeNo and i are (almost) parallel, and plane i fully invalidates plane planeNo. */
						return false;
					}

					Line line = new Line();
					line.direction = crossProduct.normalized;
					Vector3 lineNormal = Vector3.Cross(line.direction, planes[planeNo].normal);
				
					line.point = planes[planeNo].point
					+ (Vector3.Dot((planes[i].point - planes[planeNo].point), planes[i].normal) /
						Vector3.Dot(lineNormal, planes[i].normal)) * lineNormal;


				if (!linearProgram1(planes, i, line, radius, optVelocity, directionOpt, result)) {
					return false;
				}
			}
		}

		return true;
	}

	private int linearProgram3(List<RVOPlane> planes, float radius, Vector3 optVelocity, bool directionOpt, ref Vector3 result)
	{
		if (directionOpt)
		{
			/* Optimize direction. Note that the optimization velocity is of unit length in this case. */
			result = optVelocity * radius;
		}
		else if (optVelocity.sqrMagnitude > RVOKdTree.Squared(radius))
		{
			/* Optimize closest point and outside circle. */
			result = optVelocity.normalized * radius;
		}
		else
		{
			/* Optimize closest point and inside circle. */
			result = optVelocity;
		}

		for (int i = 0; i < planes.Count; ++i)
		{
			if (Vector3.Dot(planes[i].normal, (planes[i].point - result)) > 0.0f)
			{
				/* Result does not satisfy constraint i. Compute new optimal result. */
				Vector3 tempResult = result;

				if (!linearProgram2(planes, i, radius, optVelocity, directionOpt, ref  result))
				{
					result = tempResult;
					return i;
				}
			}
		}

		return planes.Count;
	}

	void linearProgram4(List<RVOPlane> planes, int beginPlane, float radius, ref Vector3 result)
	{
		float distance = 0.0f;

		for (int i = beginPlane; i < planes.Count; ++i)
		{
			if (Vector3.Dot(planes[i].normal, (planes[i].point - result)) > distance)
			{
				/* Result does not satisfy constraint of plane i. */
				List<RVOPlane> projPlanes = new List<RVOPlane>();

				for (int j = 0; j < i; ++j)
				{
					RVOPlane plane = new RVOPlane();

					Vector3 crossProduct = Vector3.Cross(planes[j].normal, planes[i].normal);

					if (crossProduct.sqrMagnitude <= RVO_EPSILON)
					{
						/* Plane i and plane j are (almost) parallel. */
						if (Vector3.Dot(planes[i].normal, planes[j].normal) > 0.0f)
						{
							/* Plane i and plane j point in the same direction. */
							continue;
						}
						else
						{
							/* Plane i and plane j point in opposite direction. */

							plane.point = 0.5f * (planes[i].point + planes[j].point);
						}
					}
					else
					{
						/* Plane.point is point on line of intersection between plane i and plane j. */
						Vector3 lineNormal = Vector3.Cross(crossProduct, planes[i].normal);
						plane.point = planes[i].point
							+ (
							Vector3.Dot((planes[j].point - planes[i].point) , planes[j].normal) /
							Vector3.Dot(lineNormal, planes[j].normal)) * lineNormal;
					}

					plane.normal = (planes[j].normal - planes[i].normal).normalized;
					projPlanes.Add(plane);
				}

				Vector3 tempResult = result;

				if (linearProgram3(projPlanes, radius, planes[i].normal, true, ref result) < projPlanes.Count)
				{
					/* This should in principle not happen.  The result is by definition already in the feasible region of this linear program. If it fails, it is due to small floating point error, and the current result is kept. */
					result = tempResult;
				}

				distance = Vector3.Dot(planes[i].normal, (planes[i].point - result));
			}
		}
	}


}

public class RVOPlane
{
	public Vector3 point;
	public Vector3 normal;
}
