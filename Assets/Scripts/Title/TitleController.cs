using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class TitleController : MonoBehaviour {

	[SerializeField]
	Text serverText;
	[SerializeField]
	Button serverStartButton;
	[SerializeField]
	Button clientStartButton;
	[SerializeField]
	InputField input;

	[SerializeField]
	GameObject presetToggleRoot;

	BoolReactiveProperty startGame = new BoolReactiveProperty(false);


	#if UNITY_STANDALONE
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void OnBeforeSceneLoadRuntimeMethod()
	{
		var width = 1024f;
		var height = 768f;

		Screen.SetResolution((int)width, (int)height, false);
	}
	#endif

	void Start()
	{
		var serverIp = IPAddress.Parse (UnityEngine.Network.player.ipAddress).ToString ();

		serverText.text = serverIp;

		serverStartButton.onClick.AddListener (() => {
			serverStartButton.onClick.RemoveAllListeners ();
			clientStartButton.onClick.RemoveAllListeners();
			GameStart (true);
		});

		clientStartButton.onClick.AddListener (() => {
			serverStartButton.onClick.RemoveAllListeners();
			clientStartButton.onClick.RemoveAllListeners();
			GameStart(false);
		});

		Network.Client.Instance.onAcceptConnect += AcceptSession;
		Network.Client.Instance.onCloseSession += CloseSession;

		startGame.Where (x => x).ObserveOnMainThread ().Subscribe (_ => {
			UnityEngine.SceneManagement.SceneManager.LoadScene ("GamePlay");
		}).AddTo (gameObject);
	}

	void OnDestroy()
	{
		if (Network.Client.Instance == null) {
			return;
		}

		Network.Client.Instance.onAcceptConnect -= AcceptSession;
		Network.Client.Instance.onCloseSession -= CloseSession;
	}

	void GameStart(bool server)
	{
		var toggles = presetToggleRoot.GetComponentsInChildren<UnityEngine.UI.Toggle> ();
		var preset = toggles.Where (x => x.isOn).First ().transform.Find ("Label").GetComponent<UnityEngine.UI.Text> ().text;

		if (server) {
			Network.Client.Instance.CreateSession (25000);
		} else {
			Network.Client.Instance.CreateSession (input.text, 25000);
		}
	}

	void AcceptSession()
	{
		startGame.Value = true;
	}

	void CloseSession()
	{
		startGame.Value = false;
	}
}
