using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AveragePosition : MonoBehaviour {

    public Transform p1;
    public Transform p2;

	private void LateUpdate()
	{

        transform.position = (p1.position + p2.position) / 2;

	}
}
