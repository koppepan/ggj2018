using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
	[CreateAssetMenu(menuName = "ScriptableObject/NetworkHostConfig", fileName = "NetworkHostConfig", order = 0)]
	public class NetworkHost : ScriptableObject
	{
		public string serverHost;
	}
}
