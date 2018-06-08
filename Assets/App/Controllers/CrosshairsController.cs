using UnityEngine;

namespace App.Controllers
{
	public class CrosshairsController : MonoBehaviour
	{
		public GameObject Green;
		public GameObject Gray;

		private bool _isGreen;
		public bool IsGreen
		{
			get { return _isGreen; }
			set
			{
				Green.SetActive(value);
				Gray.SetActive(!value);
				_isGreen = value;
			}
		}
	}
}
