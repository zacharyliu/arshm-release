using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Dropdown : MonoBehaviour {

	[Header("ANIMATORS")]
	public Animator dropdownAnimator;

	[Header("OBJECTS")]
	public GameObject fieldTrigger;
	public Text selectedText;
	public Image selectedImage;

	[Header("PLACEHOLDER")]
	public string customText;
	public Sprite customIcon;

	[Header("SETTINGS")]
	[Tooltip("IMPORTANT! EVERY DORPDOWN MUST HAVE A DIFFERENT ID")]
	public int DropdownID = 0;
	public bool customPlaceholder;
	public bool rememberSelection = true;

	//public bool darkTrigger = true;

	private bool isOn;
	private string inAnim = "In";
	private string outAnim = "Out";

	private string sText;
	private string sImage;

	void Start ()
	{
		if (rememberSelection == true)
		{
			sText = PlayerPrefs.GetString (DropdownID + "SelectedText");
			sImage = PlayerPrefs.GetString (DropdownID + "SelectedImage");
		}

		if (customPlaceholder == true)
		{
			selectedText.text = customText;
			selectedImage.sprite = customIcon;
		}

		else 
		{
			selectedText.text = sText;
			//	selectedImage.sprite = 
		}
	}

	public void Animate ()
	{
		if (isOn == true) 
		{
			dropdownAnimator.Play (outAnim);
			isOn = false;
			fieldTrigger.SetActive (false);
		}

		else
		{
			dropdownAnimator.Play (inAnim);
			isOn = true;
			fieldTrigger.SetActive (true);
		}
	}
}
