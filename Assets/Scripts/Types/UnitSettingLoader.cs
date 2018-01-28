using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UnitSettingLoader {

	static UnitSettingTable _table;

	static private UnitSettingTable Table
	{
		get{
			if (_table == null) {
				_table = Resources.Load<UnitSettingTable> ("UnitSettings");
			}
			return _table;
		}
	}

	static public UnitSettings GetSetting(string id)
	{
		return Table.settings.FirstOrDefault (x => x.id == id);
	}

	static public List<UnitSettings> GetPreset(string id)
	{
		return Table.settings.Where (x => x.id.Split ('_') [0] == id).ToList ();
	}

	static public Dictionary<string, List<UnitSettings>> GetPresetSettings()
	{
		var dic = new Dictionary<string, List<UnitSettings>>();

		Table.settings.ForEach (setting => {

			var key = setting.id.Split ('_') [0];
			if (dic.ContainsKey (key)) {
				dic [key].Add (setting);
			} else {
				dic.Add (key, new List<UnitSettings> (){ setting });
			}

		});

		return dic;
	}
}
