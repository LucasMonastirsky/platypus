using UnityEngine;

public abstract class Weapon : MonoBehaviour {

  protected PlayerInput player;
  
  public abstract void OnEquip ();
  public abstract void OnUnEquip ();

  void Start () {
    if ( (player = gameObject.GetComponent<PlayerInput>()) == null)
      Debug.LogError("Weapon found no Player component");
  }
}
