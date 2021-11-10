using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private Labyrinth labyrinth;
	[SerializeField] private PowerupSpawner powerupSpawner;
	[SerializeField] private float moveRate, invulnerabilityDuration;
	[SerializeField] private PlayerScore playerScore = new PlayerScore();
	[SerializeField] private AudioClip deathClip, powerupClip;
	[SerializeField] private AudioSource soundSource;
	[SerializeField] private ParticleSystem deathParticle;

	private float timeDelta, invulnerabilityTimer;
	private TileBase floor, wall, powerup, corridor;
	private bool isDead;
	private SpriteRenderer spriteRend;

	System.Random rand;
	Vector3Int posToGridVector = new Vector3Int(1, 1, 0);
	Color blue = new Color(0, 0.4465573f, 1);

	// Start is called before the first frame update
	void Awake()
	{
		rand = new System.Random(System.DateTime.Now.Second);
		floor = labyrinth.FLOOR;
		wall = labyrinth.WALL;
		corridor = labyrinth.CORRIDOR;
		powerup = powerupSpawner.POWERUP;
		spriteRend = GetComponent<SpriteRenderer>();

		FindStartPosition();

		playerScore.Initialize();
	}

	// Update is called once per frame
	void Update()
	{
		if (isDead)
			return;
		Movement();
		OnPickup();
	}

	void FindStartPosition()
	{
		transform.position = new Vector3(rand.Next(0, labyrinth.LABYRINTHSIZE.x) + .5f, rand.Next(0, labyrinth.LABYRINTHSIZE.y) + .5f, -1);
		int repositionAttempts = 50;
		while (labyrinth.LABYRINTH.GetTile(GridPosition) == wall && repositionAttempts > 0)
		{
			transform.position = new Vector3(rand.Next(0, labyrinth.LABYRINTHSIZE.x) + .5f, rand.Next(0, labyrinth.LABYRINTHSIZE.y) + .5f, -1);
			repositionAttempts--;
		}
	}

	void Movement()
	{
		timeDelta += Time.deltaTime;
		if (timeDelta > moveRate)
		{
			if (Input.GetAxisRaw("Vertical") > 0.0f)
			{
				if (labyrinth.LABYRINTH.GetTile(GridPosition + Vector3Int.up) == wall)
					return;
				transform.position += Vector3.up;
				timeDelta = 0.0f;
			}
			if (Input.GetAxisRaw("Vertical") < 0.0f)
			{
				if (labyrinth.LABYRINTH.GetTile(GridPosition + Vector3Int.down) == wall)
					return;
				transform.position += Vector3.down;
				timeDelta = 0.0f;
			}
			if (Input.GetAxisRaw("Horizontal") < 0.0f)
			{
				if (labyrinth.LABYRINTH.GetTile(GridPosition + Vector3Int.left) == wall)
					return;
				transform.position += Vector3.left;
				timeDelta = 0.0f;
			}
			if (Input.GetAxisRaw("Horizontal") > 0.0f)
			{
				if (labyrinth.LABYRINTH.GetTile(GridPosition + Vector3Int.right) == wall)
					return;
				transform.position += Vector3.right;
				timeDelta = 0.0f;
			}
		}
	}

	void OnPickup()
	{
		if (powerupSpawner.POWERUPS.GetTile(GridPosition) == powerup)
		{
			powerupSpawner.UpdatePowerupMap(GridPosition);
			invulnerabilityTimer = invulnerabilityDuration;
			playerScore.Update();
			HandleInvulnerability();
		}
		if (invulnerabilityTimer > 0.0f)
		{
			invulnerabilityTimer -= Time.deltaTime;
		}
	}

	Coroutine HandleInvulnerability()
	{
		return StartCoroutine(IHandleInvulnerability());
	}

	IEnumerator IHandleInvulnerability()
	{
		float t = 0.0f;
		while (invulnerabilityTimer > 1.0f)
		{
			while (t < 1.0f)
			{
				t += Time.deltaTime;
				spriteRend.color = Color.Lerp(Color.yellow, Color.magenta, t);
				if (soundSource != null && powerupClip != null)
					soundSource.PlayOneShot(powerupClip);
				yield return null;
			}
			while (t > 0.0f)
			{
				t -= Time.deltaTime;
				spriteRend.color = Color.Lerp(Color.yellow, Color.magenta, t);
				if (soundSource != null && powerupClip != null)
					soundSource.PlayOneShot(powerupClip);
				yield return null;
			}
		}
		t = 0.0f;
		while (t < 1.0f)
		{
			t += Time.deltaTime;
			spriteRend.color = Color.Lerp(spriteRend.color, blue, t);
			if (soundSource != null && powerupClip != null)
				soundSource.PlayOneShot(powerupClip);
			yield return null;
		}
	}

	Vector3Int GridPosition => transform.position.ToVector3Int() * posToGridVector;

	public bool IsInvulnerable() => invulnerabilityTimer > 0.0f;
	public void KillPlayer()
	{
		if (!isDead)
		{
			if (soundSource != null && deathClip != null)
				soundSource.PlayOneShot(deathClip);
			if (deathParticle != null)
				Instantiate(deathParticle, transform.position, Quaternion.identity);
			playerScore.Save();
			isDead = true;
		}
	}
	public bool IsDead => isDead;
	public Vector3Int GetGridPosition() => GridPosition;

	[System.Serializable]
	class PlayerScore
	{
		[SerializeField] private Text currentScore, topScore;

		private const string HIGHSCORE_STRING = "Labyrinth_Highscore";
		private int score, highscore;

		public void Initialize()
		{
#if UNITY_EDITOR
			highscore = 0;
#elif UNITY_STANDALONE
			highscore = PlayerPrefs.GetInt(HIGHSCORE_STRING);
#endif
			topScore.text = string.Format("Highscore: {000}", highscore);
			currentScore.text = string.Format("Score: {000}", score);
		}

		public void Update()
		{
			score++;
			currentScore.text = string.Format("Score: {000}", score);
		}

		public void Save()
		{
			if (score > highscore)
			{
				PlayerPrefs.SetInt(HIGHSCORE_STRING, score);
			}
		}
	}
}
