using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RVOKdTree : MonoBehaviour {

	// This tree appears to only care about X and Y. Z axis should later be introduced.
	private struct AgentTreeNode
	{
		public int begin;
		public int end;
		public int left;
		public int right;

		public Vector3 maxCoord;
		public Vector3 minCoord;
	}

	private const int MAX_LEAF_SIZE = 10;

	private RVOAgent[] agents;
	private AgentTreeNode[] agentTree;
	////RVOSimulator simulator;

	//unity start
	void Start()
	{
		RVOAgent[] agentsObj = GameObject.FindObjectsOfType(typeof(RVOAgent)) as RVOAgent[];

		agents = agentsObj;
		for (int i = 0; i < agents.Length; i++)
		{
			agents[i].id = i;
		}

		buildAgentTree();
    }

	//unity update
	void Update()
	{

		buildAgentTree();
	
	}

	//public RVOKdTree(RVOSimulator sim)
	//{
	//	simulator = sim;
	//}

	public void buildAgentTree()
	{
		//start method handles this, we'll also need to handle removal of objects as well.
		//agents = new RVOAgent[simulator.agents.Count];

		//for (int i = 0; i< simulator.agents.Count; i++)
		//{
		//	agents[i] = simulator.agents[i];
		//}
		

		if (agents.Length > 0) {
			// there was a resize on agentTree here.
			agentTree = new AgentTreeNode[agents.Length * 2 - 1];
			buildAgentTreeRecursive (0, agents.Length, 0);
		}
	}

	void buildAgentTreeRecursive(int begin, int end, int node)
	{
		agentTree[node].begin = begin;
		agentTree[node].end = end;
		agentTree[node].minCoord = agents[begin].position;
		agentTree[node].maxCoord = agents[begin].position;

		for (int i = begin + 1; i < end; ++i)
		{
			agentTree[node].maxCoord[0] = Mathf.Max(agentTree[node].maxCoord[0], agents[i].position.x);
			agentTree[node].minCoord[0] = Mathf.Min(agentTree[node].minCoord[0], agents[i].position.x);
			agentTree[node].maxCoord[1] = Mathf.Max(agentTree[node].maxCoord[1], agents[i].position.y);
			agentTree[node].minCoord[1] = Mathf.Min(agentTree[node].minCoord[1], agents[i].position.y);
			agentTree[node].maxCoord[2] = Mathf.Max(agentTree[node].maxCoord[2], agents[i].position.z);
			agentTree[node].minCoord[2] = Mathf.Min(agentTree[node].minCoord[2], agents[i].position.z);
		}

		if (end - begin > MAX_LEAF_SIZE)
		{
			/* No leaf node. */
			int coord;

			if (agentTree[node].maxCoord[0] - agentTree[node].minCoord[0] > agentTree[node].maxCoord[1] - agentTree[node].minCoord[1] && agentTree[node].maxCoord[0] - agentTree[node].minCoord[0] > agentTree[node].maxCoord[2] - agentTree[node].minCoord[2])
			{
				coord = 0;
			}
			else if (agentTree[node].maxCoord[1] - agentTree[node].minCoord[1] > agentTree[node].maxCoord[2] - agentTree[node].minCoord[2])
			{
				coord = 1;
			}
			else
			{
				coord = 2;
			}

			float splitValue = 0.5f * (agentTree[node].maxCoord[coord] + agentTree[node].minCoord[coord]);

			int left = begin;

			int right = end;

			while (left < right)
			{
				while (left < right && agents[left].position[coord] < splitValue)
				{
					++left;
				}

				while (right > left && agents[right - 1].position[coord] >= splitValue)
				{
					--right;
				}

				if (left < right)
				{
					var temp = agents[left];
					agents[left] = agents[right - 1];
                    agents[right - 1] = temp;

					++left;
					--right;
				}
			}

			int leftSize = left - begin;

			if (leftSize == 0)
			{
				++leftSize;
				++left;
				++right;
			}

			agentTree[node].left = node + 1;
			agentTree[node].right = node + 2 * leftSize;

			buildAgentTreeRecursive(begin, left, agentTree[node].left);
			buildAgentTreeRecursive(left, end, agentTree[node].right);
		}
	}

	public void computeAgentNeighbors(RVOAgent agent, ref float rangeSquared)
	{
		queryAgentTreeRecursive(agent, ref rangeSquared, 0);
	}

	void queryAgentTreeRecursive(RVOAgent agent, ref float rangeSq, int node)
	{
		if (agentTree[node].end - agentTree[node].begin <= MAX_LEAF_SIZE)
		{
			for (int i = agentTree[node].begin; i < agentTree[node].end; ++i)
			{
				agent.insertAgentNeighbor(agents[i], ref rangeSq);
			}
		}
		else
		{
			float distSqLeft = Squared(Mathf.Max(0.0f, agentTree[agentTree[node].left].minCoord[0] - agent.position.x))
				+ Squared(Mathf.Max(0.0f, agent.position.x - agentTree[agentTree[node].left].maxCoord[0]))
				+ Squared(Mathf.Max(0.0f, agentTree[agentTree[node].left].minCoord[1] - agent.position.y))
				+ Squared(Mathf.Max(0.0f, agent.position.y - agentTree[agentTree[node].left].maxCoord[1]))
				+ Squared(Mathf.Max(0.0f, agentTree[agentTree[node].left].minCoord[2] - agent.position.z))
				+ Squared(Mathf.Max(0.0f, agent.position.z - agentTree[agentTree[node].left].maxCoord[2]));

			float distSqRight = Squared(Mathf.Max(0.0f, agentTree[agentTree[node].right].minCoord[0] - agent.position.x))
				+ Squared(Mathf.Max(0.0f, agent.position.x - agentTree[agentTree[node].right].maxCoord[0]))
				+ Squared(Mathf.Max(0.0f, agentTree[agentTree[node].right].minCoord[1] - agent.position.y))
				+ Squared(Mathf.Max(0.0f, agent.position.y - agentTree[agentTree[node].right].maxCoord[1]))
				+ Squared(Mathf.Max(0.0f, agentTree[agentTree[node].right].minCoord[2] - agent.position.z))
				+ Squared(Mathf.Max(0.0f, agent.position.z - agentTree[agentTree[node].right].maxCoord[2]));

			if (distSqLeft < distSqRight)
			{
				if (distSqLeft < rangeSq)
				{
					queryAgentTreeRecursive(agent, ref rangeSq, agentTree[node].left);

					if (distSqRight < rangeSq)
					{
						queryAgentTreeRecursive(agent, ref rangeSq, agentTree[node].right);
					}
				}
			}
			else
			{
				if (distSqRight < rangeSq)
				{
					queryAgentTreeRecursive(agent,ref rangeSq, agentTree[node].right);

					if (distSqLeft < rangeSq)
					{
						queryAgentTreeRecursive(agent,ref rangeSq, agentTree[node].left);
					}
				}
			}
		}
	}

	public static float Squared(float a)
	{
		return a * a;
	}

}
