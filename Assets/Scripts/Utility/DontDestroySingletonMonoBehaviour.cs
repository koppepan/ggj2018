﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroySingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (T)FindObjectOfType(typeof(T));


				if (instance != null) {
					DontDestroyOnLoad (instance);
				}
			}
			return instance;
		}
	}

	protected virtual void Awake()
	{
		CheckInstance();
	}

	protected bool CheckInstance()
	{
		if (this == Instance) { return true; }

		Destroy(this);

		return false;

	}
}
