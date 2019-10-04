using UnityEngine;
using System.Collections;

public class FunFalling : MonoBehaviour
{
	void Update()
	{
		transform.Rotate(0, 0, Time.deltaTime * 10);
	}
}
