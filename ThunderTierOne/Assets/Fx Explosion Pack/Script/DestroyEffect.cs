using UnityEngine;
using System.Collections;

public class DestroyEffect : MonoBehaviour {

	void Update ()
	{

		
		   Destroy(transform.gameObject,2.5f);
	
	}
}
