﻿using UnityEngine;

public class PlayerInput : MonoBehaviour, WalkPhysics.EventListener {
  private WalkPhysics physics;
  private Actioner actioner;
  private PlayerActions actions;

  void Awake () {
    if ( (physics = this.gameObject.GetComponentInChildren<WalkPhysics>().Listen(this)) == null )
      Debug.LogError("Player has no Walk component");
    if ( (actioner = this.gameObject.GetComponent<Actioner>()) == null )
      Debug.LogError("Player has no Actioner component");
    if ( (actions = this.gameObject.GetComponent<PlayerActions>()) == null )
      Debug.LogError("Player has no PlayerActions component");
  }

  void Start () {
    actioner.Initialize(actions.Idle);
  }

  void Update () {
    if (Input.GetButtonDown("Crouch"))
      actioner.Queue(actions.Crouch);
    else if (Input.GetButtonUp("Crouch") && actioner.CurrentAction.Name == actions.Crouch.Name)
      actioner.Queue(actions.Idle);

    if (actioner.CurrentAction.Name == "Idle" && Input.GetAxis("Horizontal") != 0)
      actioner.Queue(actions.Walk);
    if (actioner.CurrentAction.Name == "Walk" && Input.GetAxis("Horizontal") == 0)
      actioner.Queue(actions.Idle);

    if (actioner.CurrentAction.Name == "Idle" || actioner.CurrentAction.Name == "Jump" || actioner.CurrentAction.Name == "Fall")
      physics.Walk(Input.GetAxis("Horizontal"));
    
    if (Input.GetButtonDown("Jump")) actioner.Queue(actions.Jump);
    else if (Input.GetButtonUp("Jump")) physics.StopJump();
  
    if (Input.GetButtonDown("Fire1") && physics.Grounded) TestAttack();

    actioner.InputDirection(Input.GetAxis("Horizontal"));
  }

  [ContextMenu("Test Attack")]
  void TestAttack () {
    actioner.Queue(actions.TestAttack);
  }

  #region WalkPhysics Events

  public void OnFallStart () {
    actioner.Queue(actions.Fall);
  }
  public void OnJumpEnd () { }
  public void OnLand () {
    actioner.Queue(actions.Idle);
  }

  #endregion
}