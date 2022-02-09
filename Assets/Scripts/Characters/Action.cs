using UnityEngine;

public class Action {
  #region Options

  /// <value>testing testing</value>
  public string Name = "Unnamed";
  public string[] Tags = {};
  public ActionStep[] Steps;

  public bool Loops = false;
  public bool AllowMovement = false;
  public bool PausePhysics = false;

  public delegate bool IsCancellableByDelegate (Action action);
  public IsCancellableByDelegate IsCancellableBy = CANCELLABLE_BY.ALL;

  public delegate void ActionEventHandler (Actioner actioner);
  public ActionEventHandler OnStart = (actioner) => {};
  public ActionEventHandler OnUpdate = (actioner) => {};
  public ActionEventHandler OnEnd = (actioner) => {};

  #endregion Options

  #region Vars

  private int i_current_step = 0;
  public Actioner Actioner { get; private set; }
  public ICharacterPhysics Physics { get { return Actioner.Physics; } }
  public Sprite[] Sprites;
  public ActionStep CurrentStep { get { return Steps[i_current_step]; } }

  #endregion Vars

  public void Initialize (Sprite[] sprites) {
    this.Sprites = sprites;
    for (int i = 0; i < Steps.Length; ++i) {
      Steps[i].Initialize(this, Sprites[i]);
    }
  }

  public virtual Action Start (Actioner actioner) {
    this.Actioner = actioner;
    OnStart(actioner);

    Physics.AllowMovement = AllowMovement;
    Physics.Paused = PausePhysics;

    i_current_step = 0;
    Steps[i_current_step].Start();
    return this;
  }

  public virtual void Update () {
    CurrentStep.Update();
    OnUpdate(Actioner);
  }

  public void NextStep () {
    Steps[i_current_step].End();
    if (++i_current_step < Steps.Length)
      Steps[i_current_step].Start();
    else End();
  }

  public virtual void End () {
    OnEnd(Actioner);
    if (Loops) Start(Actioner);
    else Actioner.OnActionEnd();
  }
}
