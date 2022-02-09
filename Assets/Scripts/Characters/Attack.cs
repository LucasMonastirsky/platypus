using UnityEngine;

public class Attack : Action { }

public class AttackStep : ActionStep {
  public int Damage;
  public HitBox AttackShape;

  public override ActionStep Start () {
    if (AttackShape != null) {
      AttackShape.FollowTransform(parent_action.Actioner.transform);
      AttackShape.Direction = parent_action.Actioner.Direction;
    }
    return base.Start();
  }

  public override void Update () {
    if (DebugOptions.DrawAttackHitShapes && AttackShape != null)
      AttackShape.Draw(Color.green);
    base.Update();
  }
}
