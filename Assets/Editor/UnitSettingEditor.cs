using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class UnitSettingEditor : EditorWindow
{

	UnitSettingTable table;
	List<string> removeList = new List<string> ();

	[MenuItem("Settings/UnitSetting")]
	public static void Open()
	{
		EditorWindow.GetWindow<UnitSettingEditor> ("ユニット設定");
	}

	void OnEnable()
	{
		table = Resources.Load<UnitSettingTable> ("UnitSettings");
	}

	void OnGUI()
	{
		if (GUILayout.Button ("sort")) {
			table.settings = table.settings.OrderBy (x => x.id).ToList ();
		}
		EditorGUILayout.BeginHorizontal ();

		if (GUILayout.Button ("+", GUILayout.Width (25))) {
			table.settings.Add (new UnitSettings ());
		}
		EditorGUILayout.LabelField ("id", GUILayout.Width (150));
		EditorGUILayout.LabelField ("名前", GUILayout.Width (150));
		EditorGUILayout.LabelField ("見た目", GUILayout.Width (100));
		EditorGUILayout.LabelField ("Tower", GUILayout.Width (50));

		EditorGUILayout.LabelField ("コスト", GUILayout.Width (50));
		EditorGUILayout.LabelField ("体力", GUILayout.Width (50));

		EditorGUILayout.LabelField ("攻撃力", GUILayout.Width (50));
		EditorGUILayout.LabelField ("攻撃速度s", GUILayout.Width (50));
		EditorGUILayout.LabelField ("移動速度", GUILayout.Width (50));

		EditorGUILayout.LabelField ("有効範囲", GUILayout.Width (50));
		EditorGUILayout.LabelField ("感知範囲", GUILayout.Width (50));

		EditorGUILayout.LabelField ("優先ターゲット", GUILayout.Width (100));
		EditorGUILayout.LabelField ("攻撃優先度", GUILayout.Width (100));
		EditorGUILayout.LabelField ("キャラ倍率", GUILayout.Width (100));
		EditorGUILayout.LabelField ("タワー倍率", GUILayout.Width (100));

		EditorGUILayout.LabelField ("召喚", GUILayout.Width (100));

		EditorGUILayout.EndHorizontal ();

		table.settings.ForEach (x => DrawProperty (x));
		if (removeList.Any ()) {
			table.settings = table.settings.Where (x => !removeList.Contains (x.id)).ToList ();
			removeList.Clear ();
		}
		EditorUtility.SetDirty (table);
	}

	void DrawProperty(UnitSettings setting)
	{
		EditorGUILayout.BeginHorizontal ();

		if (GUILayout.Button ("-", GUILayout.Width(25))) {
			removeList.Add (setting.id);
		}

		setting.id = EditorGUILayout.TextField (setting.id, GUILayout.Width(150));
		setting.displayName = EditorGUILayout.TextField (setting.displayName, GUILayout.Width(150));
		setting.appearanceId = EditorGUILayout.TextField (setting.appearanceId, GUILayout.Width(100));
		setting.isTower = EditorGUILayout.Toggle (setting.isTower, GUILayout.Width(50));

		setting.cost = EditorGUILayout.IntField (setting.cost, GUILayout.Width(50));
		setting.hitPoint = EditorGUILayout.IntField (setting.hitPoint, GUILayout.Width(50));

		setting.attackPower = EditorGUILayout.IntField (setting.attackPower, GUILayout.Width(50));
		setting.attackSpeed = EditorGUILayout.FloatField (setting.attackSpeed, GUILayout.Width(50));
		setting.moveSpeed = EditorGUILayout.FloatField (setting.moveSpeed, GUILayout.Width(50));

		setting.effectiveRange = EditorGUILayout.FloatField (setting.effectiveRange, GUILayout.Width(50));
		setting.sensingRange = EditorGUILayout.FloatField (setting.sensingRange, GUILayout.Width(50));

		setting.attackTarget = EditorGUILayout.IntField (setting.attackTarget, GUILayout.Width(100));
		setting.attackPriority = EditorGUILayout.IntField (setting.attackPriority, GUILayout.Width(100));
		setting.characterAttackRatio = EditorGUILayout.FloatField (setting.characterAttackRatio, GUILayout.Width(100));
		setting.towerAttackRatio = EditorGUILayout.FloatField (setting.towerAttackRatio, GUILayout.Width(100));

		setting.summonId = EditorGUILayout.TextField (setting.summonId, GUILayout.Width (100));

		EditorGUILayout.EndHorizontal ();
	}
}
