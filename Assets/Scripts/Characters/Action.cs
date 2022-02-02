using UnityEngine;

public enum ActionType { Walk, Jump, Dodge, Attack, Interact, Idle, Other, All }

public class Action {
  public ActionType Type;
  public string Name = "Unnamed";
  public bool Loops = false;
  public ActionStep[] Steps;
  public ActionStep CurrentStep { get { return Steps[i_current_step]; } }
  public Sprite[] Sprites;

  public delegate void ActionEventHandler (Actioner actioner);
  public ActionEventHandler OnStart = (actioner) => {};
  public ActionEventHandler OnUpdate = (actioner) => {};
  public ActionEventHandler OnEnd = (actioner) => {};

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
    OnStart(actioner);

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
