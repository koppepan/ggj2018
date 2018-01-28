using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Unit : MonoBehaviour {

	[SerializeField]
	UnityEngine.UI.Slider hpBar;


	UnitSettings parameter; //!< ユニットのパラメータ。この中の値に合わせて振る舞いを変える。
	int currentHitPoint = 0; //!< 現在の体力値（上限はparameterが持ち、これは現在の値が入る）

    int downhp = 2;
    float downtime = 1;
    Vector2 endposition;

    Unit target;

	float lastAttackTime = 0;
    

    RectTransform _rect;
	public RectTransform Rect
	{
		get {
			if (_rect == null) {
				_rect = GetComponent<RectTransform> ();
			}
			return _rect;
		}
	}
	public bool isTower {
		get {
			return parameter.isTower;
		}
	}
	public string Id
	{
		get {
			return parameter.id;
		}
	}

	//! 通信・攻撃範囲を外部から取得するためのパラメータ
	public float effectRange {
		get{ return parameter.effectiveRange; }
	}
	public float sensingRange
	{
		get{ return parameter.sensingRange; }
	}

	//! ユニットが生存しているかどうか
	public bool isAlive {
		get{ return currentHitPoint > 0;}
	}

	Func<Unit, Unit> getNearEnemy;
	Func<Unit, Unit> getNearTower;
	Action<string, Unit> summon = delegate {};
 
    //! パラメータの設定 生成時にGameManagerから呼ばれる
	public void SetParameter(UnitSettings unitSetting, Vector2 destPosition,
		Func<Unit, Unit> getNearEnemy,
		Func<Unit, Unit> getNearTower,
		Action<string, Unit> summon)
	{
		parameter = unitSetting;
		currentHitPoint = parameter.hitPoint;

		endposition = destPosition;

		this.getNearEnemy = getNearEnemy;
		this.getNearTower = getNearTower;
		this.summon = summon;

		this.ObserveEveryValueChanged (x => x.currentHitPoint).Subscribe (hp => {
			hpBar.value = (float)hp / (float)parameter.hitPoint;
		}).AddTo(gameObject);
	}

    //! ユニット更新処理
    public void Tick(){

		if (!isAlive) {
			return;
		}
        // 更新処理 GameManagerから毎フレーム呼ばれる
        // 移動・攻撃などをユニットのパラメータに合わせて行うように実装する   

        SetTarget();

		if ((!isTower)) {
			Move ();
		}

        //攻撃
        Attack();

        //攻撃キャラはhp減少
        DownHp();
    }

    void Move()
    {
		var dest = target == null ? endposition : target.Rect.anchoredPosition;
		if (Vector2.Distance (dest, Rect.anchoredPosition) < 1.5f) {
			if (!string.IsNullOrEmpty(parameter.summonId)) {
				summon (parameter.summonId, this);
				currentHitPoint = 0;
			}
			return;
		}


        if (target == null)
        {
            var vector = (endposition - Rect.anchoredPosition).normalized;
            Rect.localRotation = Quaternion.Euler(0, vector.x < 0 ? 180 : 0, 0);
            Rect.anchoredPosition += vector * parameter.moveSpeed;

            return;
        }

        var distance = Vector2.Distance(target.Rect.anchoredPosition, Rect.anchoredPosition);
        if (distance < parameter.effectiveRange)
        {
            return;
        }

        if (target != null)
        {
            var vector = (target.Rect.anchoredPosition - Rect.anchoredPosition).normalized;
            Rect.localRotation = Quaternion.Euler(0, vector.x < 0 ? 180 : 0, 0);
            Rect.anchoredPosition += vector * parameter.moveSpeed;
        }
    }

	void Attack()
	{
		if (target == null) {
			return;
		}

		if (Time.time - lastAttackTime < parameter.attackSpeed) {
			return;
		}

		var distance = Vector2.Distance (target.Rect.anchoredPosition, Rect.anchoredPosition);
		if (distance < parameter.effectiveRange) {
			target.Damage (parameter.attackPower);
			lastAttackTime = Time.time;
		}
	}

    //ダメージを受ける
    public void Damage(int power){
        currentHitPoint -= power;
    }


    //ターゲットの設定
    void SetTarget()
    {
        //移動の分岐        0 = 敵に向けて移動
        //                  1 = タワーに向けて移動
        if (parameter.attackTarget == 0){
            target = getNearEnemy(this);
        } else if (parameter.attackTarget == 1) {
            target = getNearTower(this);
        }
    }

    void DownHp()
    {
        downtime -= Time.deltaTime;
        if (downtime <= 0.0)
        {
            downtime = 1.0f;
            if (parameter.attackPower != 0 && parameter.attackSpeed != 0)
            {
                currentHitPoint -= downhp;
            }
        }
    }
}
