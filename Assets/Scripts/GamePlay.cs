#pragma warning disable 618, 108

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Game;

using Void = System.Boolean;

class Tile {
	public int x;
	public int y;
	public GameObject type;
	public GameObject obj;
	public bool pillInverted = false;
}

enum TileColor
{
	Blue = 0,
	Yellow = 1,
	Red = 2,
	Nope = 3,
}

enum Direction
{
	Any = 0,
	Horizontal = 1,
	Vertical = 2,
}

enum PillSpawnState
{
	Up = 0,
	Down = 1,
	Done = 2,
}

enum MouseState
{
	None = 0,
	MoveLeft = 1,
	MoveRight = 2,
	MoveDown = 3,
	Rotate = 4,
}

public class GamePlay : MonoBehaviour
{
	// List of sprites:
	public GameObject pillHorRedRed;
	public GameObject pillHorBlueBlue;
	public GameObject pillHorYellowYellow;

	public GameObject pillHorRedYellow;
	public GameObject pillHorRedBlue;
	public GameObject pillHorYellowBlue;

	public GameObject pillVertRedRed;
	public GameObject pillVertBlueBlue;
	public GameObject pillVertYellowYellow;

	public GameObject pillVertRedYellow;
	public GameObject pillVertRedBlue;
	public GameObject pillVertYellowBlue;

	public GameObject pillRed;
	public GameObject pillBlue;
	public GameObject pillYellow;

	public GameObject virusRed;
	public GameObject virusBlue;
	public GameObject virusYellow;
	const float step = 1.29f * 0.35f;

	// Audio
	public AudioClip soundMove;
	public AudioClip soundGlass;
	public AudioClip soundClick;
	private AudioSource audio;
	private AudioSource music;

	public static UIAction lastAction = UIAction.Nope;

	// Scene:
	GameObject bottle;
	const int gridWidth = 8;
	const int gridHeight = 16;
	Tile pill = null;
	Tile pillNext = null;
	PillSpawnState pillSpawnState = PillSpawnState.Done;

	Tile[,] room = new Tile[gridWidth, gridHeight];

	int score = 0;
	int best = 0;
	bool paused = false;
	int chmod = 644;
	int level = 0;
	float popupScale = 1f;

	const Void end = false;

	GameObject mmSoundON;
	GameObject mmSoundOFF;
	GameObject mmMusicON;
	GameObject mmMusicOFF;
	GameObject Pause;
	GameObject Pauser;
	GameObject mmTR;
	GameObject mmTL;
	GameObject TextLevel;
	GameObject TextScore;
	GameObject SpawnPoint;
	GameObject Hand;

	// Stages
	public GameObject MainMenu;
	public GameObject Looser;
	public GameObject Winner;
	public GameObject Room;

	Tile SpawnTile(GameObject type, int x, int y) {
		var tile = new Tile();
		tile.x = x;
		tile.y = y;
		tile.type = type;
		tile.obj = (GameObject)Object.Instantiate(type, new Vector3(bottle.transform.position.x, bottle.transform.position.y, 0), Quaternion.identity);
		tile.obj.GetComponent<SpriteRenderer>().sortingOrder = 1;
		tile.obj.name = "Spawned";
		room[x, y] = tile;
		tile.obj.transform.position = Repos(tile.x, tile.y);
		return tile;
	}

	void Awake()
	{
		Input.simulateMouseWithTouches = true;

		Looser.active = true;
		Winner.active = true;
		Room.active = true;

		GameObject.Find("GameSprites").active = false;
		Debug.Log("Awake!");
		audio = gameObject.GetComponents<AudioSource>()[0];
		music = gameObject.GetComponents<AudioSource>()[1];

		bottle = GameObject.Find("bottle");
		mmSoundON = GameObject.Find("mmSoundON");
		mmSoundOFF = GameObject.Find("mmSoundOFF");
		mmMusicON = GameObject.Find("mmMusicON");
		mmMusicOFF = GameObject.Find("mmMusicOFF");
		Pause = GameObject.Find("Pause");
		Pauser = GameObject.Find("Pauser");
		Hand = GameObject.Find("Hand");

		TextLevel = GameObject.Find("TextLevel");
		TextScore = GameObject.Find("TextScore");

		mmTR = GameObject.Find("mmTR");
		mmTL = GameObject.Find("mmTL");

		SpawnPoint = GameObject.Find("SpawnPoint");

		popupScale = Winner.transform.localScale.x;

		Looser.active = false;
		Winner.active = false;
		Room.active = false;
		SpawnPoint.active = false;

		MainMenu.active = true;

		chmod = 777;
	}

	int CheckHost() {
		var hosts = new string[] {
			"file://",
			"http://localhost/",
			"http://localhost:",
			"https://localhost/",
			"https://localhost:",

			"https://www.fgl.com",
			"https://fgl.com",
			"http://fgl.com",
			"http://www.fgl.com",

			"https://the-demigodaarts.rhcloud.com",
			"https//the-demigodaarts.rhcloud.com",
		};

		// print out list of hosts in debugging build
		if (Debug.isDebugBuild)
		{
			StringBuilder msg = new StringBuilder();
			msg.Append("Checking against list of hosts: ");
			foreach (string url in hosts)
			{
				msg.Append(url);
				msg.Append(",");
			}

			Debug.Log(msg.ToString());

			if (Application.isEditor) {
				Debug.Log(("Big Brother Is Watching You").ToString());
				return 777;
			}
		}

		// check current host against each of the given hosts
		foreach (string host in hosts)
		{
			if (Application.absoluteURL.IndexOf(host) == 0)
			{
				return 777;
			}
		}

		return 502;
	}

	int DesiredVirusesAmount() {
		if ((26 + level) > 64) return 64;
		return 26 + level;
	}

	int VirusesCeil() {
		if (level == 0) return 9;
		if (level == 1) return 8;
		if (level == 2) return 7;
		return 6;
	}

	void FillBottleWithViruses() {
		var viruses = 0;
		var desired = Random.Range(DesiredVirusesAmount() - 10, DesiredVirusesAmount() + 2);
		while (viruses < desired)
		{
			int x = Random.Range(0, 8);
			int y = Random.Range(VirusesCeil(), 16);

			if (room[x, y] != null) continue ;

			GameObject cur = null;
			switch (Random.Range(1, 4)) {
			case 1: cur = virusRed; break ;
			case 2: cur = virusBlue; break ;
			default: cur = virusYellow; break ;
			}

			var tile = new Tile();
			tile.x = x;
			tile.y = y;
			tile.type = cur;
			tile.obj = (GameObject)Object.Instantiate(cur, new Vector3(bottle.transform.position.x, bottle.transform.position.y, 0), Quaternion.identity);
			tile.obj.GetComponent<SpriteRenderer>().sortingOrder = 1;
			tile.obj.name = "Spawned";
			room[x, y] = tile;

			viruses++;
		}
	}

	float stepDelay = 0.5f;
	float moveDelay = 0.5f;
	public static bool hold = false;

	MouseState mouseState = MouseState.None;
	void GlobalMouseState() {
		if (hold) return ;
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
			var mpos = Input.mousePosition;
			var x = mpos.x;
			var y = mpos.y;

			var w = Screen.width;
			var h = Screen.height;

			if (y > h / 2) mouseState = MouseState.Rotate;
			else if (x < w / 4) mouseState = MouseState.MoveLeft;
			else if (x > (w - w / 4)) mouseState = MouseState.MoveRight;
			else mouseState = MouseState.MoveDown;
		} else mouseState = MouseState.None;
	}

	void MoveHorizontal() {
		moveDelay -= Time.deltaTime;
		if (moveDelay > 0) return ;
		if (pill == null) return ;

		var move = 0;
		if (Input.GetAxis("Horizontal") > +0.001) move = +1;
		if (Input.GetAxis("Horizontal") < -0.001) move = -1;

		if (mouseState == MouseState.MoveRight) move = +1;
		if (mouseState == MouseState.MoveLeft)  move = -1;

		if (move == 0) return ;

		// Hor
		if (PillIsHor(pill))
			if (IsFree(pill.x + move, pill.y))
				if (IsFree(pill.x + 1 + move, pill.y))
				{
					pill.x += move;
					moveDelay = 0.15f;
					if (!WillPillFall()) stepDelay += 0.15f;
					if (pill.x == 0 || pill.x == 6)
						audio.PlayOneShot(soundGlass, 0.20F);
					else
						MoveSound();
					return ;
				}

		// Vert
		if (PillIsVert(pill))
			if (IsFree(pill.x + move, pill.y))
				if (IsFree(pill.x + move, pill.y - 1))
				{
					pill.x += move;
					moveDelay = 0.15f;
					if (!WillPillFall()) stepDelay += 0.15f;
					if (pill.x == 0 || pill.x == 7)
						audio.PlayOneShot(soundGlass, 0.20F);
					else
						MoveSound();
					return ;
				}
	}

	int pressedRotate = 0;
	void PillRotate()
	{
		if (moveDelay > 0) return ;
		if (pill == null) return ;
		var move = 0;
		if (Input.GetAxis("Vertical") > +0.001 || mouseState == MouseState.Rotate) move = +1;
		if (Input.GetAxis("Vertical") < -0.001 || mouseState == MouseState.MoveDown) {
			move = -1;
			moveDelay = 0.1f;
			var y = pill.y;
			PillCheck();
			if (pill != null && y != pill.y) {
				MoveSound();
				stepDelay = WillPillFall() ? 0.75f : 0.35f;
			}
			return ;
		}

		if (pressedRotate == move) return ;
		pressedRotate = move;
		if (move == 0) return ;

		if (PillIsHor(pill))
			if (!IsFree(pill.x, pill.y - 1) || pill.y < 2)
				return ;

		if (PillIsVert(pill))
			if (!IsFree(pill.x + 1, pill.y) && !IsFree(pill.x - 1, pill.y))
				return ;

		if (PillIsVert(pill))
			if (!IsFree(pill.x + 1, pill.y))
				pill.x -= 1;

		moveDelay = 0.1f;

		GameObject cur = null;

		if (cur == null && pill.type == pillVertRedRed)       cur = pillHorRedRed;
		if (cur == null && pill.type == pillVertBlueBlue)     cur = pillHorBlueBlue;
		if (cur == null && pill.type == pillVertYellowYellow) cur = pillHorYellowYellow;
		if (cur == null && pill.type == pillVertRedYellow)    cur = pillHorRedYellow;
		if (cur == null && pill.type == pillVertRedBlue)      cur = pillHorRedBlue;
		if (cur == null && pill.type == pillVertYellowBlue)   cur = pillHorYellowBlue;

		if (cur == null && pill.type == pillHorRedRed)        cur = pillVertRedRed;
		if (cur == null && pill.type == pillHorBlueBlue)      cur = pillVertBlueBlue;
		if (cur == null && pill.type == pillHorYellowYellow)  cur = pillVertYellowYellow;
		if (cur == null && pill.type == pillHorRedYellow)     cur = pillVertRedYellow;
		if (cur == null && pill.type == pillHorRedBlue)       cur = pillVertRedBlue;
		if (cur == null && pill.type == pillHorYellowBlue)    cur = pillVertYellowBlue;

		if (PillIsHor(pill)) pill.pillInverted = !pill.pillInverted;

		var vec = pill.obj.transform.position;
		Destroy(pill.obj);

		pill.obj = (GameObject)Object.Instantiate(cur, new Vector3(bottle.transform.position.x, bottle.transform.position.y, 0), Quaternion.identity);
		pill.type = cur;
		pill.obj.transform.position = vec;
		pill.obj.GetComponent<SpriteRenderer>().sortingOrder = 1;
		pill.obj.name = "Spawned";

		MoveSound();
	}

	void SpawnNextPill()
	{
		Tile pill;
		GameObject cur = null;
		switch (Random.Range(1, 7)) {
		case 1: cur = pillHorRedRed; break ;
		case 2: cur = pillHorBlueBlue; break ;
		case 3: cur = pillHorYellowYellow; break ;
		case 4: cur = pillHorRedYellow; break ;
		case 5: cur = pillHorRedBlue; break ;
		default: cur = pillHorYellowBlue; break ;
		}

		var i = 3;
		var j = 0;

		pill = new Tile();
		pill.x = i;
		pill.y = j;
		pill.type = cur;
		pill.obj = (GameObject)Object.Instantiate(cur, new Vector3(SpawnPoint.transform.position.x, SpawnPoint.transform.position.y, 0), Quaternion.identity);
		pill.obj.GetComponent<SpriteRenderer>().sortingOrder = 2;
		pill.obj.name = "Spawned";

		pillNext = pill;
	}

	bool PlacePill() {
		room[pill.x, pill.y] = pill;
		pill = null;
		return false;
	}

	bool PillIsHor(Tile tile) {
		if (tile == null) return false;
		if (tile.type == pillHorRedRed) return true;
		if (tile.type == pillHorBlueBlue) return true;
		if (tile.type == pillHorYellowYellow) return true;
		if (tile.type == pillHorRedYellow) return true;
		if (tile.type == pillHorRedBlue) return true;
		if (tile.type == pillHorYellowBlue) return true;
		return false;
	}

	bool PillIsVert(Tile tile) {
		if (tile == null) return false;
		if (tile.type == pillVertRedRed) return true;
		if (tile.type == pillVertBlueBlue) return true;
		if (tile.type == pillVertYellowYellow) return true;
		if (tile.type == pillVertRedYellow) return true;
		if (tile.type == pillVertRedBlue) return true;
		if (tile.type == pillVertYellowBlue) return true;
		return false;
	}

	bool TileIsAnyPill(Tile tile) {
		if (tile == null) return false;
		if (tileIsPill(tile)) return true;
		if (tile.type == pillRed) return true;
		if (tile.type == pillBlue) return true;
		if (tile.type == pillYellow) return true;
		return false;
	}

	bool TileIsAnyPillAtPosition(int x, int y) {
		if (TileIsAnyPill(room[x, y])) return true;
		if (x > 0 && !IsFree(x - 1, y) && TileIsAnyPill(room[x - 1, y]) && PillIsHor(room[x - 1, y])) return true;
		if (y < gridHeight - 1 && !IsFree(x, y + 1) && TileIsAnyPill(room[x, y + 1]) && PillIsVert(room[x, y + 1])) return true;
		return false;
	}

	bool tileIsPill(Tile tile) {
		if (tile == null) return false;
		if (tile.type == pillHorRedRed) return true;
		if (tile.type == pillHorBlueBlue) return true;
		if (tile.type == pillHorYellowYellow) return true;
		if (tile.type == pillHorRedYellow) return true;
		if (tile.type == pillHorRedBlue) return true;
		if (tile.type == pillHorYellowBlue) return true;

		if (tile.type == pillVertRedRed) return true;
		if (tile.type == pillVertBlueBlue) return true;
		if (tile.type == pillVertYellowYellow) return true;
		if (tile.type == pillVertRedYellow) return true;
		if (tile.type == pillVertRedBlue) return true;
		if (tile.type == pillVertYellowBlue) return true;
		return false;
	}

	bool IsFree(int x, int y) {
		Tile tile = null;
		// Out of grid
		if (x < 0) return false;
		if (x > gridWidth - 1) return false;
		if (y < 0) return false;
		if (y > gridHeight - 1) return false;
		tile = room[x, y];
		// 1-cell
		if (tile != null) return false;
		// 2-cell -> right
		if (x >= 1) {
			tile = room[x - 1, y];
			if (PillIsHor(tile))
				return false;
		}
		// 2-cell -> top
		if ((y + 1) <= (gridHeight - 1)) {
			tile = room[x, y + 1];
			if (PillIsVert(tile))
				return false;
		}
		return true;
	}

	bool WillPillFall()
	{
		if (pill == null) return false;
		// Floor
		if (pill.y == gridHeight - 1)
			return false;

		// Hor
		if (PillIsHor(pill))
			if (IsFree(pill.x, pill.y + 1))
				if (IsFree(pill.x + 1, pill.y + 1))
				{
					return true;
				}

		// Vert
		if (PillIsVert(pill))
			if (IsFree(pill.x, pill.y + 1))
			{
				return true;
			}

		return false;
	}

	Void PillCheck()
	{
		if (pill == null) return end;
		// Floor
		if (pill.y == gridHeight - 1)
			return PlacePill();

		// Hor
		if (PillIsHor(pill))
			if (IsFree(pill.x, pill.y + 1))
				if (IsFree(pill.x + 1, pill.y + 1))
				{
					pill.y += 1;
					return end;
				}

		// Vert
		if (PillIsVert(pill))
			if (IsFree(pill.x, pill.y + 1))
			{
				pill.y += 1;
				return end;
			}

		return PlacePill();
	}

	TileColor GetColorOfTile(Tile tile) {
		if (tile == null) return TileColor.Nope;
		var ok = !tile.pillInverted;

		if (tile.type == pillHorRedRed) return TileColor.Red;
		if (tile.type == pillHorBlueBlue) return TileColor.Blue;
		if (tile.type == pillHorYellowYellow) return TileColor.Yellow;

		if (tile.type == pillHorRedYellow) return ok ? TileColor.Red : TileColor.Yellow;
		if (tile.type == pillHorRedBlue) return ok ? TileColor.Red : TileColor.Blue;
		if (tile.type == pillHorYellowBlue) return ok ? TileColor.Yellow : TileColor.Blue;

		if (tile.type == pillVertRedRed) return TileColor.Red;
		if (tile.type == pillVertBlueBlue) return TileColor.Blue;
		if (tile.type == pillVertYellowYellow) return TileColor.Yellow;

		if (tile.type == pillVertRedYellow) return ok ? TileColor.Red : TileColor.Yellow;
		if (tile.type == pillVertRedBlue) return ok ? TileColor.Red : TileColor.Blue;
		if (tile.type == pillVertYellowBlue) return ok ? TileColor.Yellow : TileColor.Blue;

		if (tile.type == pillRed) return TileColor.Red;
		if (tile.type == pillBlue) return TileColor.Blue;
		if (tile.type == pillYellow) return TileColor.Yellow;
		if (tile.type == virusRed) return TileColor.Red;
		if (tile.type == virusBlue) return TileColor.Blue;
		if (tile.type == virusYellow) return TileColor.Yellow;
		return TileColor.Nope;
	}

	TileColor GetColorOfPosition(int x, int y) {
		var tile = GetColorOfTile(room[x, y]);
		if (TileColor.Nope != tile)
			return tile;

		// HOR x - 1
		if (x > 0 && !IsFree(x - 1, y))
		{
			var t = room[x - 1, y];
			if (PillIsHor(t)) {
				var ok = t.pillInverted;

				if (t.type == pillHorRedRed) return TileColor.Red;
				if (t.type == pillHorBlueBlue) return TileColor.Blue;
				if (t.type == pillHorYellowYellow) return TileColor.Yellow;

				if (t.type == pillHorRedYellow) return ok ? TileColor.Red : TileColor.Yellow;
				if (t.type == pillHorRedBlue) return ok ? TileColor.Red : TileColor.Blue;
				if (t.type == pillHorYellowBlue) return ok ? TileColor.Yellow : TileColor.Blue;
			}
		}

		// VERT y + 1
		if (y < gridHeight - 1 && !IsFree(x, y + 1))
		{
			var t = room[x, y + 1];
			if (PillIsVert(t)) {
				var ok = t.pillInverted;

				if (t.type == pillVertRedRed) return TileColor.Red;
				if (t.type == pillVertBlueBlue) return TileColor.Blue;
				if (t.type == pillVertYellowYellow) return TileColor.Yellow;

				if (t.type == pillVertRedYellow) return ok ? TileColor.Red : TileColor.Yellow;
				if (t.type == pillVertRedBlue) return ok ? TileColor.Red : TileColor.Blue;
				if (t.type == pillVertYellowBlue) return ok ? TileColor.Yellow : TileColor.Blue;
			}
		}

		return TileColor.Nope;
	}

	bool MatchTile(int x, int y, Direction limit = Direction.Any) {
		var color = GetColorOfPosition(x, y);
		if (color == TileColor.Nope) return false;
		var hasPills = TileIsAnyPillAtPosition(x, y);
		// HOR
		var left = x;
		while (left > 0 && color == GetColorOfPosition(left - 1, y)) {
			left--;
			hasPills = hasPills || TileIsAnyPillAtPosition(left, y);
		}
		var right = left;
		while (right < gridWidth && color == GetColorOfPosition(right, y))
		{
			hasPills = hasPills || TileIsAnyPillAtPosition(right, y);
			right++;
		}

		if (limit == Direction.Vertical) hasPills = false;
		if (right - left >= 4 && hasPills) {
			while (left < right) {
				if (color != GetColorOfPosition(left, y)) {
					left++;
					continue ;
				}

				dissectPill(left, y);

				if (limit != Direction.Horizontal)
					MatchTile(left, y, Direction.Vertical);

				if (room[left, y] != null)
					AnimateDestroyAdd(room[left, y]);

				if (TileIsAnyPillAtPosition(left, y)) score += 50;
				else score += 100;

				AdvScore();

				room[left, y] = null;
				left++;

			}
			Debug.Log("Scored hor!");
			return true;
		}

		// VERT
		hasPills = TileIsAnyPillAtPosition(x, y);
		left = y;
		while (left > 0 && color == GetColorOfPosition(x, left - 1)) {
			left--;
			hasPills = hasPills || TileIsAnyPillAtPosition(x, left);
		}
		right = left;
		while (right < gridHeight && color == GetColorOfPosition(x, right))
		{
			hasPills = hasPills || TileIsAnyPillAtPosition(x, right);
			right++;
		}

		if (limit == Direction.Horizontal) hasPills = false;
		if (right - left >= 4 && hasPills) {
			while (left < right) {
				if (color != GetColorOfPosition(x, left)) {
					left++;
					continue ;
				}

				dissectPill(x, left);

				if (limit != Direction.Vertical)
					MatchTile(x, left, Direction.Horizontal);

				if (room[x, left] != null)
					AnimateDestroyAdd(room[x, left]);

				if (TileIsAnyPillAtPosition(x, left)) score += 50;
				else score += 100;

				AdvScore();

				room[x, left] = null;
				left++;
			}
			Debug.Log("Scored vert!");
			return true;
		}

		return false;
	}

	void dissectPill(int x, int y) {
		// Dissect doubles
		// Check direction
		// HOR
		if (
			PillIsHor(room[x, y])
		) {
			x++;
			var color_l = GetColorOfPosition(x - 1, y);
			var color_r = GetColorOfPosition(x, y);

			room[x - 1, y].obj.transform.localScale  = new Vector3
			(0.0f, -0.0f, 1);

			GameObject type = null;
			switch (color_l) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x - 1, y);

			type = null;
			switch (color_r) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x, y);

			return ;
		}

		if (
			x > 0
			&& !IsFree(x - 1, y)
			&& PillIsHor(room[x - 1, y])
		) {
			var color_l = GetColorOfPosition(x - 1, y);
			var color_r = GetColorOfPosition(x, y);

			room[x - 1, y].obj.transform.localScale  = new Vector3
			(0.0f, -0.0f, 1);

			GameObject type = null;
			switch (color_l) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x - 1, y);

			type = null;
			switch (color_r) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x, y);

			return ;
		}
		// VERT
		if (
			PillIsVert(room[x, y])
		) {
			y--;
			var color_l = GetColorOfPosition(x, y + 1);
			var color_r = GetColorOfPosition(x, y);

			room[x, y + 1].obj.transform.localScale  = new Vector3
			(0.0f, -0.0f, 1);

			GameObject type = null;
			switch (color_l) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x, y + 1);

			type = null;
			switch (color_r) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x, y);

			return ;
		}

		if (
			y < gridHeight - 1
			&& !IsFree(x, y + 1)
			&& PillIsVert(room[x, y + 1])
		) {
			var color_l = GetColorOfPosition(x, y + 1);
			var color_r = GetColorOfPosition(x, y);

			room[x, y + 1].obj.transform.localScale  = new Vector3
			(0.0f, -0.0f, 1);

			GameObject type = null;
			switch (color_l) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x, y + 1);

			type = null;
			switch (color_r) {
			case TileColor.Blue: type = pillBlue; break ;
			case TileColor.Yellow: type = pillYellow; break ;
			case TileColor.Red: type = pillRed; break ;
			}
			SpawnTile(type, x, y);

			return ;
		}
	}

	void Match() {
		int i = 0;
		int j = 0;
		while (i < gridHeight) { // Top -> down
			j = 0;
			while (j < gridWidth) {
				if (MatchTile(j, i))
				{
					singlesWait = 0.21f;
					Match();
					return ;
				}
				j++;
			}
			i++;
		}
	}

	bool IsLevelComplete() {
		int i = 0;
		int j = 0;
		while (i < gridHeight) {
			j = 0;
			while (j < gridWidth) {
				if (room[j, i] != null)
					if (!TileIsAnyPillAtPosition(j, i))
						return false;
				j++;
			}
			i++;
		}
		return true;
	}

	bool TileIsSinglePill(Tile tile) {
		if (tile == null) return false;
		if (tile.type == pillRed) return true;
		if (tile.type == pillBlue) return true;
		if (tile.type == pillYellow) return true;
		return false;
	}

	bool FallSingles() {
		// From FLOOR to TOP!
		bool fall = false;
		int i = gridHeight - 1;
		int j = 0;
		while (i > -1) { // Top <- down
			j = 0;
			while (j < gridWidth) {
				var tile = room[j, i];
				if (TileIsSinglePill(tile))
					if (IsFree(j, i + 1))
					{
						room[j, i + 1] = tile;
						tile.y += 1;
						room[j, i] = null;
						fall = true;
					}
				j++;
			}
			i--;
		}
		return fall;
	}

	bool FallDoubles() {
		// From FLOOR to TOP!
		bool fall = false;
		int i = gridHeight - 1;
		int j = 0;
		while (i > -1) { // Top <- down
			j = 0;
			while (j < gridWidth) {
				var tile = room[j, i];
				if (tile == null) {
					j++;
					continue ;
				}
				var type = tile.type;
				var tileIsDoublePillLeft = false;
				var isVert = false;

				if (type == pillHorRedRed) tileIsDoublePillLeft = true;
				if (type == pillHorBlueBlue) tileIsDoublePillLeft = true;
				if (type == pillHorYellowYellow) tileIsDoublePillLeft = true;
				if (type == pillHorRedYellow) tileIsDoublePillLeft = true;
				if (type == pillHorRedBlue) tileIsDoublePillLeft = true;
				if (type == pillHorYellowBlue) tileIsDoublePillLeft = true;

				if (type == pillVertRedRed) { tileIsDoublePillLeft = true; isVert = true; }
				if (type == pillVertBlueBlue) { tileIsDoublePillLeft = true; isVert = true; }
				if (type == pillVertYellowYellow) { tileIsDoublePillLeft = true; isVert = true; }
				if (type == pillVertRedYellow) { tileIsDoublePillLeft = true; isVert = true; }
				if (type == pillVertRedBlue) { tileIsDoublePillLeft = true; isVert = true; }
				if (type == pillVertYellowBlue) { tileIsDoublePillLeft = true; isVert = true; }

				if (tileIsDoublePillLeft && !isVert)
					if (IsFree(j, i + 1) && IsFree(j + 1, i + 1))
					{
						room[j, i + 1] = tile;
						tile.y += 1;
						room[j, i] = null;
						fall = true;
					}

				if (tileIsDoublePillLeft && isVert)
					if (IsFree(j, i + 1))
					{
						room[j, i + 1] = tile;
						tile.y += 1;
						room[j, i] = null;
						fall = true;
					}

				j++;
			}
			i--;
		}
		return fall;
	}

	float singlesWait = 0.0f;

	bool keyDown = false;
	void FixedUpdate()
	{
		UpdateActions();

		Pause.active = paused
					   && Room.active
					   && !Winner.active
					   && !Looser.active
					   && !MainMenu.active;

		if (Room.active) {
			if (Winner.active) return ;
			if (Looser.active) return ;
			GameStep();
			TextLevel.GetComponent<Text>().text = "Level " + (level + 1).ToString();
			TextScore.GetComponent<Text>().text = "Score " + score.ToString();
		}
	}

	void GameStep() {
		if (Input.GetKeyDown("space") && keyDown == false) {
			paused = !paused;
			keyDown = true;
		}

		if (!Input.GetKeyDown("space") && keyDown == true) {
			keyDown = false;
		}

		if (paused) return ;

		if (pillNext != null && pillNext.obj != null)
			pillNext.obj.transform.position =
				SpawnPoint.transform.position;

		var canControl = !(pill != null && pillSpawnState != PillSpawnState.Done);

		if (canControl) GlobalMouseState();

		if (canControl) if (pillNext == null) SpawnNextPill();

		singlesWait -= Time.deltaTime;
		if (singlesWait > 0f) {
			stepDelay = 0.1f;
			return ;
		}

		var falls = FallSingles();
		if (FallDoubles() || falls) {
			singlesWait = 0.15f;
			return ;
		}

		if (!canControl) return ;

		MoveHorizontal();
		PillRotate();

		stepDelay -= Time.deltaTime;
		if (stepDelay > 0f) return ;
		stepDelay = 0.75f;

		if (pill == null && !IsFree(3, 0)) {
			Debug.Log("Wasted!");
			Looser.active = true;
			Looser.transform.localScale = new Vector3(popupScale*0.7f, popupScale*0.7f, 1);
			GameObject.Find("TextLooserScore").GetComponent<Text>().text = score.ToString() +
			(best > 0? "\nBest: " + best.ToString() : "");
		}

		if (pill == null) {
			pill = pillNext;
			pillNext = null;
			pillSpawnState = PillSpawnState.Up;
			pill.obj.transform.position =
				SpawnPoint.transform.position;
		}

		PillCheck();

		if (!WillPillFall()) stepDelay = 0.35f;

		Match();

		if (IsLevelComplete()) {
			Debug.Log("Well Done!");
			Winner.active = true;
			Winner.transform.localScale = new Vector3(popupScale*0.7f, popupScale*0.7f, 1);
			GameObject.Find("TextWinnerScore").GetComponent<Text>().text = score.ToString();
		}
	}

	Vector3 Repos(float i, float j) {
		return new Vector3(bottle.transform.position.x + (0.5f + i - gridWidth / 2) * step, bottle.transform.position.y - 0.15f - (0.5f + j - gridHeight / 2) * step, 0);
	}

	void ReposPill(Tile tile) {
		if (PillIsHor(tile)) {
			var to = Repos(0.5f + tile.x, tile.y);
			tile.obj.transform.position = Towards(to, tile.obj.transform.position);
			tile.obj.transform.localScale  = new Vector3
			(tile.pillInverted ? -1 : 1, 1, 1);
		} else {
			var to = Repos(tile.x, -0.5f + tile.y);
			tile.obj.transform.position = Towards(to, tile.obj.transform.position);
			tile.obj.transform.localScale  = new Vector3
			(1, tile.pillInverted ? -1 : 1, 1);
		}
	}

	void Update()
	{
		UpdateRoom();
		UpdateMainMenu();
		UpdateEscape();
	}

	void UpdateMainMenu()
	{
		float OrthoWidth = Camera.main.orthographicSize * Camera.main.aspect;
		float OrthoHeight = Camera.main.orthographicSize;

		mmTR.transform.position = new Vector3(OrthoWidth, OrthoHeight, 0f);
		mmTL.transform.position = new Vector3(-OrthoWidth, OrthoHeight, 0f);

		if (Winner.active)
			if (Winner.transform.localScale.x < popupScale)
				Winner.transform.localScale *= 1 + Time.deltaTime * 3.5f;

		if (Looser.active)
			if (Looser.transform.localScale.x < popupScale)
				Looser.transform.localScale *= 1 + Time.deltaTime * 3.5f;
	}

	void MoveSound()
	{
		audio.PlayOneShot(soundMove, 0.80F);
	}

	void ClickSound()
	{
		audio.PlayOneShot(soundClick, 0.90F);
	}

	void UpdateActions()
	{
		if (chmod < 700 && lastAction != UIAction.Nope) {
			Debug.Log("Stop you criminal scum!");
			lastAction = UIAction.Nope;
			return ;
		}
		switch (lastAction) {
		case UIAction.SoundON:
			audio.mute = false;
			ClickSound();
			break;
		case UIAction.SoundOFF:
			audio.mute = true;
			break;
		case UIAction.MusicON:
			music.mute = false;
			ClickSound();
			break;
		case UIAction.MusicOFF:
			music.mute = true;
			ClickSound();
			break;
		case UIAction.Fullscreen:
			Screen.fullScreen = !Screen.fullScreen;
			ClickSound();
			break;
		case UIAction.Pause:
			paused = !paused;
			ClickSound();
			break;
		case UIAction.OpenSponsorLink:
			// TODO
			ClickSound();
			break;
		case UIAction.Leaderboard:
			// TODO
			ClickSound();
			break;
		case UIAction.NextLevel:
			StartNewLevel();
			Winner.active = false;
			ClickSound();
			break;
		case UIAction.TryAgain:
			TryAgain();
			Looser.active = false;
			ClickSound();
			break;
		case UIAction.PlayGame:
			MainMenu.active = false;
			Room.active = true;
			TryAgain();
			ClickSound();
			break;
		}

		mmSoundON.active = !audio.mute;
		mmSoundOFF.active = audio.mute;

		mmMusicON.active = !music.mute;
		mmMusicOFF.active = music.mute;

		mmSoundOFF.transform.position =
			mmSoundON.transform.position;

		mmMusicOFF.transform.position =
			mmMusicON.transform.position;
		mmMusicOFF.transform.Translate(0.05f, 0.04f, 0);

		Pauser.active = Room.active
						&& !Winner.active
						&& !Looser.active
						&& !MainMenu.active;

		lastAction = UIAction.Nope;
	}

	Vector3 Towards(Vector3 fromxy, Vector3 toxy)
	{
		return new Vector3((fromxy.x + toxy.x) * 0.5f, (fromxy.y + toxy.y) * 0.5f, 0);
	}

	void UpdateRoom()
	{
		if (pill != null && pillSpawnState == PillSpawnState.Done) ReposPill(pill);

		AnimateDestroy();

		if (pillSpawnState == PillSpawnState.Up && pill != null) {
			pill.obj.transform.Translate(0, Time.deltaTime * 10f, 0);
			Hand.transform.rotation = Quaternion.Euler(0, 0, 45);
			if (pill.obj.transform.position.y >= 5.05f) {
				pillSpawnState = PillSpawnState.Down;
				pill.obj.transform.position =
					new Vector3(bottle.transform.position.x, 5.1f, 0);
			}
		}

		if (pillSpawnState == PillSpawnState.Down && pill != null) {
			pill.obj.transform.position =
				new Vector3(
				pill.obj.transform.position.x,
				pill.obj.transform.position.y * 0.8f
				+
				Repos(3, 1).y * 0.2f
				, 0);
			if (Hand.transform.rotation.z > -13)
				Hand.transform.Rotate (0, 0, -Time.deltaTime * 60f);
			if (pill.obj.transform.position.y <= Repos(3, 1).y + 0.05f) {
				pillSpawnState = PillSpawnState.Done;
				ReposPill(pill);
			}
		}

		int i = 0;
		int j = 0;
		while (i < gridWidth) {
			j = 0;
			while (j < gridHeight) {
				var tile = room[i, j];
				j++;
				if (tile == null) continue ;
				if (!tile.obj.active) tile.obj.active = true;
				if (!tileIsPill(tile))
					tile.obj.transform.position =
						Towards(tile.obj.transform.position, Repos(tile.x, tile.y));
				else ReposPill(tile);
			}
			i++;
		}
	}

	List<Tile> animateDestroy = new List<Tile>();
	void AnimateDestroyAdd(Tile tile)
	{
		animateDestroy.Add(tile);
	}

	void AnimateDestroy()
	{
		foreach (Tile tile in animateDestroy)
		{
			if (!tile.obj.active) continue ;
			tile.obj.transform.position = Repos(tile.x, tile.y);
			tile.obj.transform.localScale *= 0.91f;
			if (tile.obj.transform.localScale.y < 0.05f)
				tile.obj.active = false;
		}
	}

	void StartNewLevel()
	{
		pillSpawnState = PillSpawnState.Done;
		level++;
		if (pill != null && pill.obj != null) pill.obj.active = false;
		if (pillNext != null && pillNext.obj != null) pillNext.obj.active = false;
		pill = null;
		pillNext = null;
		int i = 0;
		int j = 0;
		while (i < gridHeight) {
			j = 0;
			while (j < gridWidth) {
				if (room[j, i] != null)
				{
					if (room[j, i].obj != null)
						room[j, i].obj.active = false;
					room[j, i] = null;
				}
				j++;
			}
			i++;
		}
		foreach (Tile tile in animateDestroy) tile.obj.active = false;
		animateDestroy = new List<Tile>();
		while (GameObject.Find("Spawned") != null)
			GameObject.Find("Spawned").active = false;
		FillBottleWithViruses();
	}

	void TryAgain()
	{
		level = -1;
		score = 0;
		StartNewLevel();
	}

	void AdvScore()
	{
		if (score > best) {
			best = score;
		}
	}

	void UpdateEscape()
	{
#if UNITY_ANDROID
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			MobileNativeDialog dialog = new MobileNativeDialog("Quit Game", "Do you want to quit?");
			dialog.OnComplete += OnDialogClose;
		}
#endif
	}

	private void OnDialogClose(MNDialogResult result)
	{
		switch (result)
		{
		case MNDialogResult.YES:
			Application.Quit();
			break;
		case MNDialogResult.NO:
			Debug.Log("Escape cancelled");
			break;
		}
	}
}
