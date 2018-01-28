using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySE : MonoBehaviour {
	public AudioManager.SE se;

	public void play(){
		AudioManager.Instance.PlaySE(se);
	}
}
