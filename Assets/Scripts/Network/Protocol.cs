using UnityEngine;

namespace Network
{
	[System.Serializable]
	public class TextMessage
	{
		public string message;
	}

	[System.Serializable]
	public class GameStart
	{
	}

	[System.Serializable]
	public class GameFinish
	{
	}

	[System.Serializable]
	public class SpawnUnit
	{
		public string id;
		public Vector3 spawnPosition;
		public Vector3 destinationPosition;
	}
}
