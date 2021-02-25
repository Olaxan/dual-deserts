using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeCubeScript : MonoBehaviour
{
	public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = player.transform.position + Vector3.down * 2;
    }
}
