using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sc_target : MonoBehaviour {
    public static Vector3 pos;
	void Start ()
    {
    }
	// Update is called once per frame
	void Update ()
    {
        pos = transform.position;
    }
}
