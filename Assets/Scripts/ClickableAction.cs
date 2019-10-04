using UnityEngine;
using System.Collections;

namespace Game {
public enum UIAction
{
	SoundON = 0,
	SoundOFF = 1,
	Nope = 2,
	Fullscreen = 4,
	Pause = 5,
	PlayGame = 6,
	OpenSponsorLink = 7,
	TryAgain = 8,
	Leaderboard = 9,
	NextLevel = 10,
	MusicON = 11,
	MusicOFF = 12,
}

[RequireComponent(typeof(BoxCollider2D))]
public class ClickableAction : MonoBehaviour
{
	public UIAction action;
	void OnMouseUpAsButton()
	{
		OnMouseExit();
		GamePlay.lastAction = action;
	}
	void Awake()
	{
		sx = transform.localScale.x;
		sy = transform.localScale.y;
	}
	float sx;
	float sy;
	void OnMouseEnter()
	{
		GamePlay.hold = true;
		transform.localScale = new Vector3(sx * 1.11f, sy * 1.11f, 1);
	}
	void OnMouseExit()
	{
		GamePlay.hold = false;
		transform.localScale = new Vector3(sx, sy, 1f);
	}
}
}
