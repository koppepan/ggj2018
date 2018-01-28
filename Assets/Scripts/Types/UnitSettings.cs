using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitSettings
{
	public string id;

	public string displayName;
	public string appearanceId;

	public bool isTower;
	public int cost;

	public int hitPoint;

	public int attackPower;
	public float attackSpeed;

	public float effectiveRange;
	public float sensingRange;

	public float moveSpeed;

	public int attackTarget;
	public int attackPriority;
	public float characterAttackRatio;
	public float towerAttackRatio;

	public string summonId;
}

