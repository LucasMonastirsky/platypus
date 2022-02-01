using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour {

  protected Player player;
  
  public abstract void OnEquip ();
  public abstract void OnUnEquip ();

  void Start () {
    if ( (player = gameObject.GetComponent<Player>()) == null)
      Debug.LogError("Weapon found no Player component");
  }
}
