using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LabyrinthGameManager : MonoBehaviour
{
	[SerializeField] private Text endText;
	[SerializeField] private Image endBackground;

	private static LabyrinthGameManager instance;
	private PlayerController player;

	void Start()
	{
		Time.timeScale = 1.0f;
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
		player = FindObjectOfType<PlayerController>();
	}

	void Update()
	{
		if (player.IsDead)
		{
			ShowEndText();
			if (Input.GetKeyDown(KeyCode.R))
			{
				RestartGame();
			}
		}

#if UNITY_STANDALONE
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
#endif
	}

	public static Coroutine RestartGame()
	{
		return instance.StartCoroutine(instance.IRestartGame());
	}

	IEnumerator IRestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		yield return null;
	}

	Coroutine ShowEndText()
	{
		if (endText.color.a > 0.0f)
			return null;
		return StartCoroutine(IShowEndText());
	}

	IEnumerator IShowEndText()
	{
		endBackground.gameObject.SetActive(true);
		float t = 0.0f;
		while (t < 1.0f)
		{
			endText.color = Color.Lerp(Color.clear, Color.black, t);
			t += Time.deltaTime;
			yield return null;
		}
		Time.timeScale = 0.0f;
	}
}
