using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour {
  private WalkPhysics walk;
  private Actioner actioner;
  private PlayerActions actions;

  void Awake () {
    if ( (walk = this.gameObject.GetComponentInChildren<WalkPhysics>()) == null )
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
    #region Input

    if (Input.GetButtonDown("Crouch"))
      actioner.Queue(actions.Crouch);
    else if (Input.GetButtonUp("Crouch"))
      actioner.Queue(actions.Idle);
    if (actioner.CurrentAction.Name == "Idle")
      walk.Walk(Input.GetAxis("Horizontal"));
    actioner.InputDirection(Input.GetAxis("Horizontal"));
    if (Input.GetButtonDown("Jump")) walk.Jump(true);
    if (Input.GetButtonUp("Jump")) walk.Jump(false);
    if (Input.GetButtonDown("Fire1")) TestAttack();

    #endregion
  }

  [ContextMenu("Test Attack")]
  void TestAttack () {
    actioner.Queue(actions.TestAttack);
  }
}
