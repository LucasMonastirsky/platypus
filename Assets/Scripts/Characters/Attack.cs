using UnityEngine;

public class Attack : Action { }

public class AttackStep : ActionStep {
  public int Damage;
  public HitShape AttackShape;

  public override ActionStep Start () {
    if (AttackShape != null) {
      AttackShape.FollowTransform(parent_action.Actioner.transform);
      AttackShape.Direction = parent_action.Actioner.Direction;
    }
    return base.Start();
  }

  public override void Update () {
    if (AttackShape != null) {
      var hits = Game.CurrentChunk.Damageables.FindAll(target => AttackShape.Overlaps(target.Shape));
      if (hits.Count > 0)
        Debug.Log("HIT!");

      if (DebugOptions.DrawAttackHitShapes)
        AttackShape.Draw(Color.green);
    }

    base.Update();
  }
}
