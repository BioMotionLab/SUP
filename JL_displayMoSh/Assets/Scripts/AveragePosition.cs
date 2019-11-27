using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AveragePosition : MonoBehaviour {

    public Transform p1;
    public Transform p2;

	void LateUpdate()
	{
		//TODO Not super sure what this is used for. Maybe the FreeLookCamera? so it has something to target?
        //transform.position = (p1.position + p2.position) / 2;

	}
}
