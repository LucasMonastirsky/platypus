using UnityEditor;
using UnityEngine;

public class Actioner : MonoBehaviour {
  private WalkPhysics physics; public WalkPhysics Physics { get { return physics; } }
  private Action current_action, queued_action, default_action;
  public Action CurrentAction { get { return current_action; } }
  public ActionStep CurrentStep { get { return CurrentAction.CurrentStep; } }
  private GameObject child;
  private SpriteRenderer sprite_renderer;
  private int direction = 1; public int Direction { get { return direction; } set { SetDirection(value); } }

  public Vector2 SpriteOffset { set { child.transform.localPosition = new Vector3(value.x * direction, value.y); } }

  void Awake () {
    child = new GameObject("Sprite");
    child.transform.parent = this.transform;
    child.transform.localPosition = Vector3.zero;
    sprite_renderer = child.AddComponent<SpriteRenderer>();
    physics = GetComponent<WalkPhysics>();
  }

  void Update () {
    CurrentAction.Update();
  }

  public void Initialize (Action default_action) {
    this.default_action = default_action;
    current_action = default_action.Start(this);
  }

  public void Queue (Action new_action) {
    if (CurrentStep.IsCancellableBy(new_action)) {
      CurrentStep.Cancel();
      current_action = new_action.Start(this);
    } else { // this might not work idk
      queued_action = new_action;
    }
  }

  public void InputDirection (float value) {
    if (CurrentStep.AllowsFlip && value != 0) SetDirection((int) value);
  }

  private void SetDirection (int value) {
    if (value >= -1 && value != 0 && value <= 1) {
      direction = value;
      sprite_renderer.flipX = direction < 0;
      Physics.Flipped = direction < 0;
    }
    else Debug.LogWarning($"Called Actioner.SetDirection with invalid value: {value}");
  }

  public void OnActionEnd () {
    if (queued_action != null) {
      current_action = queued_action.Start(this);
      queued_action = null;
    }
    else current_action = default_action.Start(this);
  }

  public void SetSprite (Sprite sprite) {
    sprite_renderer.sprite = sprite;
  }
}

[CustomEditor(typeof(Actioner))]
public class ActionerEditor : Editor {}
