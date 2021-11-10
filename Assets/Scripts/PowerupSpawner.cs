using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Audio;

public class PowerupSpawner : MonoBehaviour
{
	[SerializeField] private int labyrinthWidth, labyrinthHeight;
	[SerializeField] [Range(0f, 1f)] private float powerupSpawnChance;
	[SerializeField] private AudioSource soundSource;
	[SerializeField] private AudioClip powerupPickupClip;
	[SerializeField] private ParticleSystem pickupParticle;

	[SerializeField] private Tilemap powerups;
	[SerializeField] private TileBase powerupTile;

	List<Vector3Int> powerupTiles = new List<Vector3Int>();

	System.Random rand;

	[HideInInspector] public Tilemap POWERUPS { get { return powerups; } }

	[HideInInspector] public TileBase POWERUP { get { return powerupTile; } }

	[HideInInspector] public List<Vector3Int> POWERUPTILES { get { return powerupTiles; } }

	[HideInInspector] public bool DONE_GENERATING { get { return doneGenerating; } }

	private bool doneGenerating = false;
	private Labyrinth labyrinth;

	// Start is called before the first frame update
	void Awake()
	{
		rand = new System.Random(System.DateTime.Now.Second);
		labyrinth = FindObjectOfType<Labyrinth>();
		PlacePowerups();
	}

	public void UpdatePowerupMap(Vector3Int pos, TileBase tileType = null)
	{
		powerups.SetTile(pos, tileType);
		powerupTiles.Remove(pos);
		if (pickupParticle != null)
			Instantiate(pickupParticle, new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0), Quaternion.identity);
		if (soundSource != null && powerupPickupClip != null)
			soundSource.PlayOneShot(powerupPickupClip);
		PlacePowerup();
	}

	Coroutine PlacePowerups()
	{
		return StartCoroutine(IPlacePowerups());
	}

	IEnumerator IPlacePowerups()
	{
		yield return new WaitUntil(() => labyrinth.DONE_GENERATING);
		foreach (Vector3Int pos in labyrinth.WALKABLETILES)
		{
			if (rand.NextDouble() > 1f - powerupSpawnChance && powerups.GetTile(pos) != powerupTile)
			{
				BSPTile tile = BSPTile.CreateTileInstance(pos, powerupTile);
				powerups.SetTile(pos, tile.tile);
				powerupTiles.Add(pos);
			}
		}
		doneGenerating = true;
	}

	void PlacePowerup()
	{
		foreach (Vector3Int pos in labyrinth.WALKABLETILES)
		{
			if (rand.NextDouble() > 1f - powerupSpawnChance && powerups.GetTile(pos) != powerupTile)
			{
				BSPTile tile = BSPTile.CreateTileInstance(pos, powerupTile);
				powerups.SetTile(pos, tile.tile);
				powerupTiles.Add(pos);
				return;
			}
		}
	}
}
