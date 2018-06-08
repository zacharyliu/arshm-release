using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour {

	[Header("ANIMATORS")]
	public Animator canvasAnimator;

	[Header("PANELS")]
	public List<GameObject> panels = new List<GameObject>();
	public int currentPanelIndex = 0;
	public GameObject currentPanel;
	private CanvasGroup canvasGroup;

	[Header("ANIMATION SETTINGS")]
	private bool fadeOut = false;
	private bool fadeIn = false;
	[Range(0, 10)]public float fadeFactor = 8f;

	void Update ()
	{
		if (fadeOut)
			canvasGroup.alpha -= fadeFactor * Time.deltaTime;
		if (fadeIn) 
		{
			canvasGroup.alpha += fadeFactor * Time.deltaTime;
		}
	}

	public void ChangePanel (int newPage) 
	{
		if (newPage != currentPanelIndex)
			StartCoroutine ("ChangePage", newPage);
	}

	public IEnumerator ChangePage (int newPage)
	{
		canvasGroup = currentPanel.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 1f;
		fadeIn = false;
		fadeOut = true;

		while(canvasGroup.alpha > 0)
		{
			yield return 0;
		}
		currentPanel.SetActive(false);

		fadeIn = true;
		fadeOut = false;
		currentPanelIndex = newPage;
		currentPanel = panels [currentPanelIndex];
		currentPanel.SetActive (true);
		canvasGroup = currentPanel.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;

		while (canvasGroup.alpha <1f)
		{
			yield return 0;
		}

		canvasGroup.alpha = 1f;
		fadeIn = false;

		yield return 0;
	}
}
