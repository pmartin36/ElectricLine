using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ContextManager : MonoBehaviour
{

	public virtual void Awake() {
		GameManager.Instance.ContextManager = this;
	}

	public virtual void Start() {

	}
	public abstract void HandleInput(InputPackage p);
}
