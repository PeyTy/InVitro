using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFit : MonoBehaviour
{
	SpriteRenderer sr;
	int lastSize = -100500;

	void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
	}

	void FixedUpdate()
	{
		var s = Screen.height * Screen.width + (int)Screen.orientation;
		if (lastSize == s) return ;
		lastSize = s;
		float worldScreenHeight = Camera.main.orthographicSize * 2;
		float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

		transform.localScale = new Vector3(
			worldScreenWidth / sr.sprite.bounds.size.x,
			worldScreenHeight / sr.sprite.bounds.size.y, 1);
	}
}
