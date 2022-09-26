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

	AudioClip GetRandomST(List<AudioClip> clips)
	{
		return clips[Random.Range(0, clips.Count - 1)];
	}

	AudioClip GetRandomCombatST()
	{
		return GetRandomST(combatSTs);
	}

	AudioClip GetRandomNormalST()
	{
		return GetRandomST(normalSTs);
	}

	void ChangeState(State s) {
		if (s != state) {
			if (s == State.Normal && state == State.Combat)
			{
				/* Change quickly from normal to combat music. */
			}
			else
			{
				/* Fade from combat music back to normal music. */
			}
		}

		state = s;
	}
}
