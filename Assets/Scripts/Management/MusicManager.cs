using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
	public enum State
	{
		Normal,
		Combat
	}

	[SerializeField] List<AudioClip> normalSTs = new List<AudioClip>();
	[SerializeField] List<AudioClip> combatSTs = new List<AudioClip>();

	[SerializeField] float fadeSpeed = 0.5f;

	AudioSource normalAS;
	AudioSource combatAS;

	AudioSource fadingTo;
	AudioSource fadingFrom;

	bool fading;
	float fadeTime;

	public State state { get; private set; }
	public MusicManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	private void Start()
	{
		fading = false;

		normalAS = gameObject.AddComponent<AudioSource>();
		normalAS.loop = true;
		normalAS.clip = GetRandomNormalST();
		normalAS.volume = 1.0f;
		normalAS.Play();

		combatAS = gameObject.AddComponent<AudioSource>();
		combatAS.loop = true;
		combatAS.clip = GetRandomCombatST();
		combatAS.volume = 0.0f;
	}

	private void Update()
	{
		if (fading) {
			fadeTime += Time.deltaTime;

			if (fadeTime >= 1.0f)
			{
				fadeTime = 1.0f;
				fading = false;
			}

			fadingFrom.volume = 1.0f - fadeTime;
			fadingTo.volume = fadeTime;
		}
	}

	AudioClip GetRandomST(List<AudioClip> clips)
	{
		int i = Random.Range(0, clips.Count);
		return clips[i];
	}

	AudioClip GetRandomCombatST()
	{
		return GetRandomST(combatSTs);
	}

	AudioClip GetRandomNormalST()
	{
		return GetRandomST(normalSTs);
	}

	public void ChangeState(State s) {
		if (s != state) {
			if (s == State.Normal && state == State.Combat)
			{
				ChangeToCombat();
			}
			else
			{
				ChangeToNormal();
			}
		}

		state = s;
	}

	public void ChangeToCombat()
	{
		fading = true;
		fadeTime = 0.0f;
		fadingFrom = normalAS;
		fadingTo = combatAS;
		combatAS.clip = GetRandomCombatST();
		combatAS.Play();
	}

	public void ChangeToNormal()
	{
		fading = true;
		fadeTime = 0.0f;
		fadingFrom = combatAS;
		fadingTo = normalAS;
		normalAS.clip = GetRandomNormalST();
		normalAS.Play();
	}
}
