using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAudioManager : MonoBehaviour {
	public GameObject AudioManagerPrefab = null;
	void Awake(){
		if(AudioManager.Instance == null){
			Instantiate(AudioManagerPrefab);
		}
	}
}
