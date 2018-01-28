using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class GameManager : SingletonMonoBehaviour<GameManager> {
	enum UNITASIGN {
		OWN,
		OPPONENT
	}

	[SerializeField]
	Unit unitPrefab;
	[SerializeField]
	UnityEngine.UI.Button background;
	[SerializeField]
	Transform unitParent;
	[SerializeField]
	Transform enemyParent;
	[SerializeField]
	Transform buttonParent;
	[SerializeField]
	UnityEngine.UI.Text countDown;
	[SerializeField]
	TrailRenderer trailPrefab;

	[SerializeField]
	UnityEngine.UI.Text costLabel;
	[SerializeField]
	UnityEngine.UI.Text gameTimeLabel;
	[SerializeField]
	int totalCost = 1000;
	[SerializeField]
	int recoveryCost = 30;

	[SerializeField]
	int gameTotalTime;

	bool gameStart = false;
	int currentCost;
	int currentTime;

	Dictionary<UNITASIGN, List<Unit>> unitTable = new Dictionary<UNITASIGN, List<Unit>> ();

	UnitSpawn currentUnitButton = null;
	System.IDisposable spawnDisposable = null;

	BoolReactiveProperty closeSession = new BoolReactiveProperty(false);

	protected override void Awake()
	{
		unitTable.Add (UNITASIGN.OWN, new List<Unit> ());
		unitTable.Add (UNITASIGN.OPPONENT, new List<Unit> ());

		gameTimeLabel.text = string.Format("Time : {0}", gameTotalTime);
		UpdateCost (totalCost);

		System.IDisposable disposable = null;

		int count = 4;

		disposable = Observable.Interval (System.TimeSpan.FromSeconds (1)).Subscribe (_ => {

			count--;
			countDown.text = count.ToString ();

			if(count == 0)
			{
				countDown.text = "Start!!";
			}
			if (count == -1) {
				disposable.Dispose ();

				countDown.text = "";
				gameStart = true;
				SetSpawnObservable();
				SetObservable();
			}
		}).AddTo (gameObject);

		var settings = new List<string> () {
			"red_large_carrier",
			"red_small_carrier",
			"red_unit_1",
			"red_unit_2",
		};

		for (int i = 0; i < 4; i++) {
			var setting = UnitSettingLoader.GetSetting (settings [i]);
			buttonParent.Find ("UnitButton_" + (i + 1).ToString ()).GetComponent<UnitSpawn> ().SetParameter (setting.id);
		}

		if (Network.Client.Instance == null) {
			return;
		}
		Network.Client.Instance.onCloseSession += CloseSession;
		Network.Client.Instance.Reciever.OnRecvSpawnUnit += OnRecvSpawnUnit;

		closeSession.Where (x => x).ObserveOnMainThread ().Subscribe (_ => {
			UnityEngine.SceneManagement.SceneManager.LoadScene ("Title");
		});
	}

	void Start(){
		// BGM再生
		if (AudioManager.Instance != null) {
			AudioManager.Instance.PlayBGM (AudioManager.BGM.GAME_PLAY);
		}
		// カウントダウンSE再生
		// AudioManager.Instance.PlaySE(AudioManager.SE.COUNT_DOWN);
	}

	void SetSpawnObservable()
	{
		if(spawnDisposable != null)
		{
			spawnDisposable.Dispose();
		}
		if(currentUnitButton == null)
		{
			return;
		}

		var time = (UnitSettingLoader.GetSetting(currentUnitButton.ButtonId).hitPoint / 2f) * 0.01f;

		spawnDisposable = background.OnClickAsObservable ().ThrottleFirst (System.TimeSpan.FromSeconds (time)).Subscribe (_ => {

			if(!gameStart)
			{
				return;
			}
			if (currentUnitButton == null || string.IsNullOrEmpty (currentUnitButton.ButtonId)) {
				return;
			}
			CreateOwnUnit (currentUnitButton.ButtonId, Input.mousePosition);
		}).AddTo (gameObject);
	}

	void SetObservable()
	{
		System.IDisposable disposable = null;
		currentTime = gameTotalTime;

		System.IDisposable dis = null;

		dis = Observable.Interval (System.TimeSpan.FromSeconds (1)).Skip (1).Subscribe (_ => {
			UpdateCost (recoveryCost);

			currentTime = Mathf.Max(0, currentTime - 1);
			gameTimeLabel.text = string.Format("Time : {0}", currentTime);
			if(currentTime == 0)
			{
				dis.Dispose();
				countDown.text = "Finish!!";

				Observable.Timer(System.TimeSpan.FromSeconds(3)).Subscribe(__ => {
					CheckWinner();
				}).AddTo(gameObject);

			}
		}).AddTo (gameObject);


		Observable.Interval (System.TimeSpan.FromSeconds (3)).Skip (1).Subscribe (_ => {
			CheckTransmission ();
		}).AddTo (gameObject);
	}

	void CloseSession()
	{
		closeSession.Value = true;
	}
	void OnDestroy()
	{
		if (Network.Client.Instance == null) {
			return;
		}

		Network.Client.Instance.onCloseSession -= CloseSession;
		Network.Client.Instance.Reciever.OnRecvSpawnUnit -= OnRecvSpawnUnit;
	}

	void Update(){
		if (currentTime <= 0) {
			return;
		}
		// ユニット更新処理
		TickUnits();
		// タワーの接続を監視
	}

	private void TickUnits(){

		unitTable [UNITASIGN.OPPONENT].ForEach (x => x.Tick ());
		unitTable [UNITASIGN.OWN].ForEach (x => x.Tick ());


		var dead = unitTable [UNITASIGN.OWN].Where (x => !x.isAlive).ToList();
		unitTable [UNITASIGN.OWN] = unitTable [UNITASIGN.OWN].Where (x => !dead.Contains (x)).ToList ();
		dead.ForEach (x => {
			if(x.isTower){
				AudioManager.Instance.PlaySE(AudioManager.SE.DEFEAT_TOWER);
			}else{
				AudioManager.Instance.PlaySE(AudioManager.SE.DEFEAT_CHARACTER);
			} 
			Destroy (x.gameObject);
		});

		dead = unitTable [UNITASIGN.OPPONENT].Where (x => !x.isAlive).ToList();
		unitTable [UNITASIGN.OPPONENT] = unitTable [UNITASIGN.OPPONENT].Where (x => !dead.Contains (x)).ToList ();
		dead.ForEach (x => {
			if(x.isTower){
				AudioManager.Instance.PlaySE(AudioManager.SE.DEFEAT_TOWER);
			}else{
				AudioManager.Instance.PlaySE(AudioManager.SE.DEFEAT_CHARACTER);
			} 
			Destroy (x.gameObject);
		});

	}

	Unit GetNearEnemy(Unit unit)
	{
		var type = unitTable [UNITASIGN.OWN].Contains (unit) ? UNITASIGN.OPPONENT : UNITASIGN.OWN;
		return unitTable [type]
			.Where(x => !x.isTower)
			.Where (x => Vector2.Distance (x.Rect.anchoredPosition, unit.Rect.anchoredPosition) < unit.sensingRange)
			.OrderBy (x => Vector2.Distance (x.Rect.anchoredPosition, unit.Rect.anchoredPosition))
			.FirstOrDefault ();
	}
	Unit GetNearOwnTower(Unit unit)
	{
		var type = unitTable [UNITASIGN.OWN].Contains (unit) ? UNITASIGN.OWN : UNITASIGN.OPPONENT;
		return unitTable [type].Where(x => x.isTower).OrderBy (x => Vector2.Distance (x.Rect.anchoredPosition, unit.Rect.anchoredPosition)).FirstOrDefault();
	}
	Unit GetNearEnemyTower(Unit unit)
	{
		var type = unitTable [UNITASIGN.OWN].Contains (unit) ? UNITASIGN.OPPONENT : UNITASIGN.OWN;
		return unitTable [type].Where(x => x.isTower)
			.Where (x => Vector2.Distance (x.Rect.anchoredPosition, unit.Rect.anchoredPosition) < unit.sensingRange)
			.OrderBy (x => Vector2.Distance (x.Rect.anchoredPosition, unit.Rect.anchoredPosition))
			.FirstOrDefault();
	}

	//! 対象座標に直接召喚(ユニットから呼ばれるやつ)
	void Summon(string id, Unit self){
		var type = unitTable [UNITASIGN.OWN].Contains (self) ? UNITASIGN.OWN : UNITASIGN.OPPONENT;
		CreateUnit (id, self.Rect.anchoredPosition, self.Rect.anchoredPosition, type);
		// タワー設置SE再生
		switch(id){
			case "large_tower":
				AudioManager.Instance.PlaySE(AudioManager.SE.PUT_TOWER_LARGE);
				break;
			case "small_tower":
				AudioManager.Instance.PlaySE(AudioManager.SE.PUT_TOWER_SMALL);
				break;
			default:
				break;
		}
	}

	//! ユニットを生成して返すだけ
	private Unit CreateUnit(string id, Vector2 spawnPos, Vector2 clickPos, UNITASIGN asign)
	{
		var setting = UnitSettingLoader.GetSetting (id);

		var unit = Instantiate (unitPrefab, asign == UNITASIGN.OWN ? unitParent : enemyParent, false);

		if (!setting.isTower) {
			var controller = Resources.Load<RuntimeAnimatorController> ("Animation/" + setting.appearanceId + "_anime");
			unit.GetComponent<Animator> ().runtimeAnimatorController = controller;
		} else {
			var sprite = Resources.Load<Sprite> ("Textures/" + setting.appearanceId);
			unit.GetComponent<UnityEngine.UI.Image> ().sprite = sprite;
		}

		unit.GetComponent<RectTransform> ().anchoredPosition = spawnPos;
		unit.SetParameter (UnitSettingLoader.GetSetting (id), clickPos, GetNearEnemy, GetNearEnemyTower, Summon);

		unitTable [asign].Add (unit);

		if (asign == UNITASIGN.OPPONENT) {
			unit.GetComponent<UnityEngine.UI.Image> ().color = Color.black;
		}
		if (asign == UNITASIGN.OWN && unit.isTower) {
			var image = unit.transform.Find ("Range").GetComponent<UnityEngine.UI.Image> ();
			image.rectTransform.sizeDelta = Vector2.one * unit.effectRange;
			image.color = new Color (1, 1, 1, 0.2f);
		}

		return unit;
	}

	void CreateOwnUnit(string id, Vector2 destPos)
	{
		var spawnPos = destPos;
		var setting = UnitSettingLoader.GetSetting (id);
		if (!setting.isTower) {
			spawnPos.x = 0;
		}
		if (currentCost < setting.cost) {
			return;
		}
		UpdateCost (-setting.cost);
		var unit = CreateUnit(id, spawnPos, destPos, UNITASIGN.OWN);


		if (Network.Client.Instance != null) {
			Network.Client.Instance.Send (new Network.SpawnUnit {
				id = id,
				spawnPosition = spawnPos,
				destinationPosition = destPos,
			});
		}

		// キャラ配置SE再生
		AudioManager.Instance.PlaySE(AudioManager.SE.CHARACTER_PUT_CLICK);
	}

	private void CreateEnemyUnit(string id, Vector2 destPos)
	{
		destPos = new Vector2 (Mathf.Abs (destPos.x - Screen.width), destPos.y);

		var spawnPos = destPos;
		if (!UnitSettingLoader.GetSetting (id).isTower) {
			spawnPos.x = Screen.width;
		}
		CreateUnit (id, spawnPos, destPos, UNITASIGN.OPPONENT);
	}

	void OnRecvSpawnUnit(Network.SpawnUnit spawn)
	{
		CreateEnemyUnit (spawn.id, spawn.destinationPosition);
	}

	//! ボタンダウンしたら呼ばれる
	public void OnUnitButtonDown(UnitSpawn buttonScript){
		currentUnitButton = buttonScript;
		SetSpawnObservable ();
		// キャラボタンSE再生
		AudioManager.Instance.PlaySE(AudioManager.SE.UNIT_BUTTON_CLICK);
	}

	void UpdateCost(int value)
	{
		currentCost = Mathf.Min (totalCost, currentCost + value);
		currentCost = Mathf.Max (0, currentCost);
		costLabel.text = string.Format ("coin : {0}", currentCost);
	}

	void CheckTransmission()
	{
		var towers = unitTable [UNITASIGN.OWN].Where (x => x.isTower).ToList ();
		if (!towers.Any ()) {
			return;
		}

		towers.ForEach (x => {
			towers.ForEach (y => {

				if (x != y) {

					if (x.effectRange + y.effectRange > Vector2.Distance (x.Rect.anchoredPosition, y.Rect.anchoredPosition)) {

						var pos = Camera.main.ScreenToWorldPoint (x.Rect.anchoredPosition) + Vector3.forward * 5;
						var end = Camera.main.ScreenToWorldPoint (y.Rect.anchoredPosition) + Vector3.forward * 5;

						var trail = Instantiate (trailPrefab, pos, Quaternion.identity);
						trail.startColor = trail.endColor = new Color (Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.5f);

						trail.transform.DOMove (end, 1f).OnComplete (() => {
							Destroy (trail.gameObject);
						});
					}
				}

			});
		});
	}

	void CheckWinner()
	{
		var own = CheckPoint (UNITASIGN.OWN);
		var enemy = CheckPoint (UNITASIGN.OPPONENT);

		if (own == enemy) {
			countDown.text = "DRAW!";
		} else if (own > enemy) {
			countDown.text = "WIN!!";
		} else if (own < enemy) {
			countDown.text = "LOSE...";
		}

		Observable.Timer (System.TimeSpan.FromSeconds (3)).Subscribe (_ => {
			Debug.Log("hoge");
			if(Network.Client.Instance != null)
			{
				closeSession.Value = true;
				Network.Client.Instance.CloseSession();
			}

			UnityEngine.SceneManagement.SceneManager.LoadScene ("Title");
		}).AddTo(gameObject);
	}

	float CheckPoint(UNITASIGN team)
	{
		var towers = unitTable [team].Where (x => x.isTower).ToList ();
		if (!towers.Any ()) {
			return 0;
		}

		var effective = towers.Where (x => {

			return towers.Any (y => {
				return x != y && x.effectRange + y.effectRange > Vector2.Distance (x.Rect.anchoredPosition, y.Rect.anchoredPosition);
			});

		}).ToList ();


		float width = Screen.width;

		if (team == UNITASIGN.OWN) {
			return effective.Sum (x => {

				if (x.Id == "small_tower") {
					return 3f * (x.Rect.anchoredPosition.x / width);
				}
				if (x.Id == "large_tower") {
					return 10f * (x.Rect.anchoredPosition.x / width);
				}
				return 0;

			});
		} else {

			return effective.Sum (x => {

				if (x.Id == "small_tower") {
					return 3f * (Mathf.Abs(width - x.Rect.anchoredPosition.x) / width);
				}
				if (x.Id == "large_tower") {
					return 10f * (Mathf.Abs(width - x.Rect.anchoredPosition.x) / width);
				}
				return 0;
			});

		}
	}
}
