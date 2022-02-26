using UnityEngine;

public class ActionStep {
  #region Options

  public int Duration = 1;
  public bool AllowsFlip = true;
  public bool Freeze = false;
  public HitShape PhysicsShape;
  public Vector2 StartDisplacement, UpdateDisplacement;
  public Vector2 SpriteOffset = new Vector2(0, 0);
  public Action.IsCancellableByDelegate IsCancellableBy;

  #endregion Options

  public Action.ActionEventHandler OnStart = (action, actioner) => {};
  public Action.ActionEventHandler OnUpdate = (action, actioner) => {};
  public Action.ActionEventHandler OnEnd = (action, actioner) => {};

  protected int frame_count; public int FrameCount { get { return frame_count; } }
  protected Sprite sprite;
  protected Action parent_action;
  protected Actioner actioner { get { return parent_action.Actioner; } }
  protected WalkPhysics physics { get { return actioner.Physics; } }

  public void Initialize (Action parent_action, Sprite sprite) {
    this.parent_action = parent_action;
    this.sprite = sprite;
  }

  public virtual ActionStep Start () {
    frame_count = 0;
    actioner.SetSprite(sprite);
    actioner.SpriteOffset = SpriteOffset;
    physics.Shape = PhysicsShape;
    if (StartDisplacement != null)
      physics.Displace(StartDisplacement.x * actioner.Direction, StartDisplacement.y);

    OnStart(parent_action, actioner);
    Update();
  
    return this;
  }

  public virtual void Update () {
    if (!Freeze && ++frame_count > Duration) {
      End();
      parent_action.NextStep();
      return;
    }
    if (UpdateDisplacement != null)
      physics.Displace(UpdateDisplacement.x * actioner.Direction, UpdateDisplacement.y);
    
    OnUpdate(parent_action, actioner);
  }

  public virtual void End () {
    OnEnd(parent_action, actioner);
  }

  public virtual void Cancel () {}
}

public static class CANCELLABLE_BY {
  public static Action.IsCancellableByDelegate ALL = action => true;
  public static Action.IsCancellableByDelegate NONE = action => false;
}
