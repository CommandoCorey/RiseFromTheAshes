using UnityEngine;
using System.Collections.Generic;

public struct Notification
{
	public string message;
	public float time;
}

public class Notify : MonoBehaviour {
	static public Notify Instance { get; private set; }

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

	[SerializeField] TMPro.TextMeshProUGUI text;
	[SerializeField] GameObject textContainer;

	float timer;
	Queue<Notification> queue = new Queue<Notification>();

	Notification currentNotif;

	private void Update()
	{
		timer += Time.deltaTime;

		if (queue.Count > 0)
		{
			
			
			if (timer >= currentNotif.time)
			{
				queue.Dequeue();
				timer = 0.0f;

				if (queue.Count == 0)
				{
					text.gameObject.SetActive(false);
					Instance.textContainer.SetActive(false);
				}
			}
		}
	}

	public static void Queue(string message, float time)
	{
		int c = Instance.queue.Count;

		Notification n = new Notification();
		n.message = message;
		n.time = time;
		Instance.queue.Enqueue(n);


		if (c == 0)
		{
			Instance.timer = 0.0f;
			Instance.currentNotif = Instance.queue.Peek();
			Instance.text.text = Instance.currentNotif.message;
			Instance.text.gameObject.SetActive(true);
			Instance.textContainer.SetActive(true);
		}
	}
}
