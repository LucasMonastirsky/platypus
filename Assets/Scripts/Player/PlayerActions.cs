using UnityEditor;
using UnityEngine;

public class PlayerActions : MonoBehaviour {
  public Attack TestAttack = new Attack () {
    Name = "Test Attack",
    Steps = new AttackStep[] {
      new AttackStep {
        Duration = 4,
        CancellableBy = CANCELLABLE_BY.ALL,
        PhysicsShape = new HitBox(0, 0, .9f, 1.7f),
        SpriteOffset = new Vector2(0.2f, 0f),
      }, new AttackStep {
        Duration = 7,
        CancellableBy = CANCELLABLE_BY.NONE,
        PhysicsShape = new HitBox(0f, 0, .8f, 1.7f),
        SpriteOffset = new Vector2(.4f, 0),
      }, new AttackStep {
        Duration = 5,
        Damage = 10,
        CancellableBy = CANCELLABLE_BY.NONE,
        PhysicsShape = new HitBox(0.33f, 0, .9f, 1.7f),
        SpriteOffset = new Vector2(.9f, 0),
        UpdateDisplacement = new Vector2(0.3f, 0),
        AttackShape = new HitBox(1.5f, .85f, 1.1f, .2f),
      },
    }
  };

  public Action Idle = new Action () {
    Name = "Idle",
    Type = ActionType.Idle,
    Loops = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 3,
        CancellableBy = CANCELLABLE_BY.ALL,
        PhysicsShape = new HitBox(0, 0, 1, 1.7f),
      },
    }
  };

  public Action Crouch = new Action () {
    Name = "Crouch",
    Type = ActionType.Idle,
    Loops = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 1,
        CancellableBy = CANCELLABLE_BY.ALL,
        PhysicsShape = new HitBox(0, 0, 1, 1),
        SpriteOffset = new Vector2(.1f, 0),
      },
    },
  };

  public Action Jump = new Action () {
    Name = "Jump",
    Type = ActionType.Jump,
    Loops = false,
    Steps = new ActionStep[] {
      new ActionStep () {
        Freeze = true,
        PhysicsShape = new HitBox(0, 0, 1, 1),
      },
    }
  };

  public Action Fall = new Action () {
    Name = "Fall",
    Type = ActionType.Other,
    Loops = false,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 5,
        PhysicsShape = new HitBox(0, 0, 1, 1),
      }, new ActionStep () {
        Duration = 1,
        Freeze = true,
        PhysicsShape = new HitBox(0, 0, 1, 1),
      }
    }
  };

  void Awake () {
    Idle.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Idle"));
    Crouch.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Crouch"));
    Jump.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Jump"));
    Fall.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Fall"));
    TestAttack.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/TestAttack"));
  }
}

[CustomEditor(typeof(PlayerActions))]
public class PlayerActionsEditor : Editor {
  private ActionInspector action_inspector = new ActionInspector();
  public override void OnInspectorGUI() {
    var player = (PlayerActions) target;
    var actions = new [] { player.Idle, player.Crouch, player.TestAttack };

    action_inspector.Draw(actions);
  }
}
