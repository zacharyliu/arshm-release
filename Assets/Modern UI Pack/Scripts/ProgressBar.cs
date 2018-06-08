using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ProgressBar : MonoBehaviour {

	[Header("OBJECTS")]
	public Transform loadingBar;
	public Transform textPercent;

	[Header("VARIABLES (IN-GAME)")]
	public bool isOn;
	public bool restart;
	[Range(0, 100)] public float currentPercent;
	[Range(0, 100)] public int speed;

	void Update ()
	{
		if (currentPercent < 100 && isOn == true) 
		{
			currentPercent += speed * Time.deltaTime;
		}

		if (currentPercent == 100 || currentPercent >= 100 && restart == true) 
		{
			currentPercent = 0;
		}
		loadingBar.GetComponent<Image> ().fillAmount = currentPercent / 100;
		textPercent.GetComponent<Text> ().text = ((int)currentPercent).ToString ("F0") + "%";
	}
}