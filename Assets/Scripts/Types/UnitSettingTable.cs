using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/UnitSettings", fileName = "UnitSettings", order = 0)]
[System.Serializable]
public class UnitSettingTable : ScriptableObject {
	public List<UnitSettings> settings = new List<UnitSettings>();
}
