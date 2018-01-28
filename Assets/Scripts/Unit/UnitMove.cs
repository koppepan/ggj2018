using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMove : MonoBehaviour {

    public UnitSettings parameter;

    Vector3 statposition;
    Vector3 endposition;

    // Use this for initialization
    void Start ()
    {
        

        transform.position = statposition;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(parameter.isTower)
        {
            Attack();
        }
        else if(!parameter.isTower)
        {
            Installation();
        }

	}

    //移動
    void Move()
    {

    }

    //攻撃
    void Attack()
    {
          Move();
    }

    //タワーの行動
    void Installation()
    {
        Move();
    }


}
