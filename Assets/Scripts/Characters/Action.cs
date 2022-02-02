using UnityEngine;

public enum ActionType { Walk, Jump, Dodge, Attack, Interact, Idle, Other, All }

public class Action {
  public ActionType Type;
  public string Name = "Unnamed";
  public bool Loops = false;
  public ActionStep[] Steps;
  public ActionStep CurrentStep { get { return Steps[i_current_step]; } }
  public Sprite[] Sprites;

  private int i_current_step = 0;
  public Actioner Actioner;

  public void Initialize (Sprite[] sprites) {
    this.Sprites = sprites;
    for (int i = 0; i < Steps.Length; ++i) {
      Steps[i].Initialize(this, Sprites[i]);
    }
  }

  public virtual Action Start (Actioner actioner) {
    this.Actioner = actioner;

    i_current_step = 0;
    Steps[i_current_step].Start();
    return this;
  }

  public virtual void Update () {
    CurrentStep.Update();
  }

  public void NextStep () {
    Steps[i_current_step].End();
    if (++i_current_step < Steps.Length)
      Steps[i_current_step].Start();
    else End();
  }

  public delegate void OnEndHandler ();
  public OnEndHandler OnEnd = () => {};
  public virtual void End () {
    OnEnd();
    if (Loops) Start(Actioner);
    else Actioner.OnActionEnd();
  }
}

public class ActionStep {
  #region Options

  public int Duration = 1;
  public ActionType[] CancellableBy = CANCELLABLE_BY.ALL;
  public bool AllowsFlip = true;
  public bool Freeze = false;
  public HitShape PhysicsShape;
  public Vector2 StartDisplacement, UpdateDisplacement;
  public Vector2 SpriteOffset = new Vector2(0, 0);

  #endregion Options

  protected int frame_count; public int FrameCount { get { return frame_count; } }
  protected Sprite sprite;
  protected Action parent_action;
  protected Actioner actioner { get { return parent_action.Actioner; } }
  protected WalkPhysics physics { get { return actioner.Physics; } }

  public void Initialize (Action parent_action, Sprite sprite) {
    this.parent_action = parent_action;
    this.sprite = sprite;
  }

  public virtual bool IsCancellableBy (Action action) {
    if (CancellableBy.Length < 1)
      return false;
    if (CancellableBy[0] == ActionType.All)
      return true;

    foreach (ActionType t in CancellableBy) {
      if (t == action.Type) return true;
    }

    return false;
  }

  public delegate void OnStartHandler ();
  public OnStartHandler OnStart = () => {};
  public virtual ActionStep Start () {
    OnStart();
    frame_count = 0;
    actioner.SetSprite(sprite);
    actioner.SpriteOffset = SpriteOffset;
    physics.Shape = PhysicsShape;
    if (StartDisplacement != null)
      physics.Displace(StartDisplacement.x * actioner.Direction, StartDisplacement.y);
  
    return this;
  }

  public virtual void Update () {
    if (!Freeze && ++frame_count > Duration) {
      parent_action.NextStep();
      return;
    }
    if (UpdateDisplacement != null)
      physics.Displace(UpdateDisplacement.x * actioner.Direction, UpdateDisplacement.y);
  }

  public virtual void End () {}

  public virtual void Cancel () {}
}

public static class CANCELLABLE_BY {
  public static ActionType[] ALL = new ActionType[] { ActionType.All, };
  public static ActionType[] NONE = new ActionType[] { };
}
