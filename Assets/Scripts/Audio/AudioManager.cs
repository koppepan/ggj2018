using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMonoBehaviour<AudioManager> {
	public enum SE{
		GAME_START,
		ATACK_ATK,
		ATACK_DFC,
		PUT_TOWER_SMALL,
		PUT_TOWER_LARGE,
		COUNT_DOWN,
		DEFEAT_CHARACTER,
		DEFEAT_TOWER,
		UNIT_BUTTON_CLICK,
		CHARACTER_PUT_CLICK
	}
	public enum BGM{
		TITLE,
		GAME_PLAY,
		HURRY_UP,
		RESULT
	}

	Dictionary<SE, string> SEPathTable = new Dictionary<SE, string>(){
		{SE.GAME_START, 			"se/se01"},
		{SE.ATACK_ATK, 				"se/se02"},
		{SE.ATACK_DFC, 				"se/se03"},
		{SE.PUT_TOWER_SMALL, 		"se/se04"},
		{SE.PUT_TOWER_LARGE, 		"se/se05"},
		{SE.COUNT_DOWN, 			"se/se06"},
		{SE.DEFEAT_CHARACTER, 		"se/se07"},
		{SE.DEFEAT_TOWER, 			"se/se08"},
		{SE.UNIT_BUTTON_CLICK, 		"se/se09"},
		{SE.CHARACTER_PUT_CLICK, 	"se/se10"}
	};
	Dictionary<BGM, string> BGMPathTable = new Dictionary<BGM, string>(){
		{BGM.TITLE, 	"bgm/bgm01"},
		{BGM.GAME_PLAY, "bgm/bgm02"},
		{BGM.HURRY_UP, 	"bgm/bgm03"},
		{BGM.RESULT, 	"bgm/bgm04"}
	};

	public AudioSource BGMSource;
	public int SESlotNum = 10;
	Dictionary<SE, AudioClip> SEClipTable = new Dictionary<SE, AudioClip>();
	AudioSource[] SESourceSlot = null;

	void Start(){
		SESourceSlot = new AudioSource[SESlotNum];
		for(var i=0; i<SESourceSlot.Length; i++){
			SESourceSlot[i] = gameObject.AddComponent<AudioSource>();
		}
		DontDestroyOnLoad(this);
		LoadSE();
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.Alpha1)){
			PlayBGM(BGM.TITLE);
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)){
			PlayBGM(BGM.GAME_PLAY);
		}
		if(Input.GetKeyDown(KeyCode.Q)){
			PlaySE(SE.CHARACTER_PUT_CLICK);
		}
		if(Input.GetKeyDown(KeyCode.W)){
			PlaySE(SE.DEFEAT_TOWER);
		}
		if(Input.GetKeyDown(KeyCode.E)){
			PlaySE(SE.DEFEAT_CHARACTER);
		}
		if(Input.GetKeyDown(KeyCode.R)){
			PlaySE(SE.COUNT_DOWN);
		}
	}

	void LoadSE(){
		foreach(var keyvalue in SEPathTable){
			SEClipTable.Add(keyvalue.Key, LoadAudioClip(keyvalue.Value));
		}
	}

	public void PlaySE(SE se){
		if(SESourceSlot == null) return;
		foreach(var slot in SESourceSlot){
			if(slot.isPlaying) continue;
			slot.clip = SEClipTable[se];
			slot.Play();
			break;
		}
	}

	public void PlayBGM(BGM bgm){
		AudioClip clip = LoadAudioClip(BGMPathTable[bgm]);
		BGMSource.Stop();
		BGMSource.clip = clip;
		BGMSource.Play();
	}

	private AudioClip LoadAudioClip(string path){
		Debug.Log(path);
		AudioClip audio = Resources.Load(path) as AudioClip;
		Debug.Log(audio.name);
		return audio;
	}
}
