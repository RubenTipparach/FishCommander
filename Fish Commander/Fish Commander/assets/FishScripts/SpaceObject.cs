using UnityEngine;
using System.Collections;

public class SpaceObject : MonoBehaviour {

	[SerializeField]
	private string Name = "";

	[SerializeField]
	private TransponderType transponderType = TransponderType.Nuetral;

	public float health = 10;

	public string ObjName
	{
		get
		{
			return name;
		}
	}

	public TransponderType Transponder
	{
		get
		{
			return transponderType;
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public enum TransponderType
{
	Enemy,
	Nuetral,
	Friendly,
	Own
}
