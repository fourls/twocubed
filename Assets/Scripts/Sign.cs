using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class Sign : MonoBehaviour {
	public string title;
	[TextArea(1,5)]
	public string description;
}
