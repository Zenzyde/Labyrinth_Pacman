using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class AiGhostBase : MonoBehaviour
{
	[SerializeField] protected PlayerController player;
	[SerializeField] protected Labyrinth labyrinth;
	[SerializeField] protected PowerupSpawner powerupSpawner;
	[SerializeField] protected float moveRate;
	[SerializeField] protected AudioClip deathClip;
	[SerializeField] protected AudioSource soundsSource;
	[SerializeField] protected ParticleSystem deathParticle;

	protected float timeDelta;
	protected TileBase floor, wall, powerup, corridor;
	protected System.Random rand;
	protected Vector3Int[] moveDirections = new Vector3Int[]
	{
		Vector3Int.up,
		Vector3Int.down,
		Vector3Int.left,
		Vector3Int.right
	};

	Vector3Int posToGridVector = new Vector3Int(1, 1, 0);

	// Start is called before the first frame update
	protected virtual void Start()
	{
		rand = new System.Random(System.DateTime.Now.Millisecond);
		player = FindObjectOfType<PlayerController>();
		labyrinth = FindObjectOfType<Labyrinth>();
		powerupSpawner = FindObjectOfType<PowerupSpawner>();
		floor = labyrinth.FLOOR;
		wall = labyrinth.WALL;
		corridor = labyrinth.CORRIDOR;
		powerup = powerupSpawner.POWERUP;
		FindStartPosition();
	}

	// Update is called once per frame
	protected virtual void Update()
	{
		Move();
		CheckOverlap();
	}

	protected abstract void Move();
	protected virtual bool CheckOverlap()
	{
		if (player.GetGridPosition() == GridPosition)
		{
			if (!player.IsInvulnerable())
				player.KillPlayer();
			else
			{
				if (deathParticle != null)
					Instantiate(deathParticle, transform.position, Quaternion.identity);
				if (soundsSource != null && deathClip != null)
					soundsSource.PlayOneShot(deathClip);
				FindStartPosition();
			}
			return true;
		}
		return false;
	}

	void FindStartPosition()
	{
		transform.position = new Vector3(rand.Next(0, labyrinth.LABYRINTHSIZE.x) + .5f, rand.Next(0, labyrinth.LABYRINTHSIZE.y) + .5f, -1);
		int repositionAttempts = 50;
		while ((labyrinth.LABYRINTH.GetTile(GridPosition) == wall || GridPosition == player.GetGridPosition()) && repositionAttempts > 0)
		{
			transform.position = new Vector3(rand.Next(0, labyrinth.LABYRINTHSIZE.x) + .5f, rand.Next(0, labyrinth.LABYRINTHSIZE.y) + .5f, -1);
			repositionAttempts--;
		}
	}

	protected Vector3Int GridPosition => transform.position.ToVector3Int() * posToGridVector;
	public Vector3Int GetGridPosition() => GridPosition;
}
