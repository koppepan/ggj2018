using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network
{
	public class Reciever {

		Queue<Network.Message> queue = new Queue<Network.Message>();

		public event System.Action<TextMessage> OnRecvTextMessage = delegate{};
		public event System.Action<GameStart> OnRecvGameStart = delegate{};
		public event System.Action<GameFinish> OnRecvGameFinish = delegate{};
		public event System.Action<SpawnUnit> OnRecvSpawnUnit = delegate{};

		public void Recv(Network.Message msg)
		{
			queue.Enqueue (msg);
		}
		public void Update()
		{
			while (queue.Count > 0) {
				var msg = queue.Dequeue ();
				switch (msg.key) {

				case "TextMessage":
					OnRecvTextMessage (JsonUtility.FromJson<TextMessage> (msg.body));
					break;

				case "GameStart":
					OnRecvGameStart (JsonUtility.FromJson<GameStart> (msg.body));
					break;

				case "GameFinish":
					OnRecvGameFinish (JsonUtility.FromJson<GameFinish> (msg.body));
					break;

				case "SpawnUnit":
					OnRecvSpawnUnit (JsonUtility.FromJson<SpawnUnit> (msg.body));
					break;
				}
			}
		}
	}
}
