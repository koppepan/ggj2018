using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitSpawn : MonoBehaviour {
	//public Text UnitCountText;
	public Text CoinCostText;

	private string UnitButtonID;
	private int UnitCount;
	public float timeOut;

	bool ButtonFlag;
	Toggle BT1;

	public string ButtonId {
		get {
			return UnitButtonID;
		}
	}

	public void SetParameter(string id)
	{
		UnitButtonID = id;

		UnitSettings setting = UnitSettingLoader.GetSetting (UnitButtonID);
		CoinCostText.text = setting.cost.ToString();

		var sprite = Resources.Load<Sprite> ("Textures/" + setting.appearanceId);
		transform.Find("Image").GetComponent<UnityEngine.UI.Image>().sprite = sprite;
	}

	//スポーン数0体、コスト読み込み
	void Start () {
		UnitCount = 0;
		timeOut = 1.0f;
		ButtonFlag = true;

		BT1 = GetComponent<Toggle> ();
		BT1.onValueChanged.AddListener (flg => {

			transform.Find("Background").GetComponent<UnityEngine.UI.Image>().color = flg ? Color.red : Color.white;

			if(flg)
			{
				GameManager.Instance.OnUnitButtonDown(this);
				BT1.interactable = false;
				ButtonFlag = false;
				StartCoroutine(FuncCoroutine());
			}
			else
			{
				BT1.interactable = true;
				ButtonFlag = true;
			}
		});
	}

	IEnumerator FuncCoroutine() {
		yield return new WaitForSeconds(timeOut);
		BT1.interactable = true;
		ButtonFlag = true;
	}

	//ボタンダウン時
	public void UnitSpwanDown(){
		GameManager.Instance.OnUnitButtonDown (this);
		if (!ButtonFlag) {
			BT1.interactable = true;
			ButtonFlag = true;
		}
	}
	//ボタンアップ時にカウントアップ
	public void UnitSpwanUpToUnitCountUp(){
		UnitCount += 1;
		CoinCostText.text = UnitCount.ToString();
		StartCoroutine( FuncCoroutine() );
		BT1.interactable = false;
		ButtonFlag = false;

	}


}
