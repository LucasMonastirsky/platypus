using UnityEditor;
using UnityEngine;

public class PlayerActions : MonoBehaviour {
  public Attack TestAttack = new Attack () {
    Name = "Test Attack",
    IsCancellableBy = CANCELLABLE_BY.NONE,
    Steps = new AttackStep[] {
      new AttackStep {
        Duration = 10,
        IsCancellableBy = CANCELLABLE_BY.ALL,
        PhysicsShape = new HitBox(0, 0, .9f, 1.7f),
        SpriteOffset = new Vector2(0.2f, 0f),
      }, new AttackStep {
        Duration = 5,
        PhysicsShape = new HitBox(0f, 0, .8f, 1.7f),
        SpriteOffset = new Vector2(.4f, 0),
        AllowsFlip = false,
      }, new AttackStep {
        Duration = 8,
        Damage = 10,
        PhysicsShape = new HitBox(0.33f, 0, .9f, 1.7f),
        SpriteOffset = new Vector2(.9f, 0),
        UpdateDisplacement = new Vector2(0.3f, 0),
        AttackShape = new HitBox(1.5f, .85f, 1.1f, .2f),
        AllowsFlip = false,
      },
    }
  };

  public Action Idle = new Action () {
    Name = "Idle",
    Loops = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 3,
        IsCancellableBy = CANCELLABLE_BY.ALL,
        PhysicsShape = new HitBox(0, 0, 1, 1.7f),
      },
    }
  };

  public Action Crouch = new Action () {
    Name = "Crouch",
    Loops = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 1,
        IsCancellableBy = CANCELLABLE_BY.ALL,
        PhysicsShape = new HitBox(0, 0, 1, 1),
        SpriteOffset = new Vector2(.1f, 0),
      },
    },
  };

  public Action Walk = new Action () {
    Name = "Walk",
    Loops = true,
    AllowMovement = true,
    OnEnd = (actioner) => { ((WalkPhysics) actioner.Physics).Walk(0); },
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 1,
        IsCancellableBy = CANCELLABLE_BY.ALL,
        PhysicsShape = new HitBox(0, 0, 1, 1.7f),
        OnUpdate = (actioner) => { ((WalkPhysics) actioner.Physics).Walk(actioner.Direction); }
      },
    },
  };

  public Action Jump = new Action () {
    Name = "Jump",
    Loops = false,
    AllowMovement = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Freeze = true,
        PhysicsShape = new HitBox(0, 0, 1, 1.7f),
      },
    },
    OnStart = (actioner) => { ((WalkPhysics) actioner.Physics).Jump(); }
  };

  public Action Fall = new Action () {
    Name = "Fall",
    Loops = false,
    AllowMovement = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 15,
        PhysicsShape = new HitBox(0, 0, 1, 1.7f),
      }, new ActionStep () {
        Duration = 1,
        Freeze = true,
        PhysicsShape = new HitBox(0, 0, 1, 1.7f),
      }
    }
  };

  public Action WallSlide = new Action () {
    Name = "WallSlide",
    Loops = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 1,
        PhysicsShape = new HitBox(0, 0, 1, 1.7f),
      },
    }
  };

  public Action Land = new Action () {
    Name = "Land",
    Loops = false,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 2,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
      }, new ActionStep () {
        Duration = 30,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
      }, new ActionStep () {
        Duration = 2,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
      },
    }
  };

  public Action PreSafetyRoll = new Action () {
    Name = "PreSafetyRoll",
    AllowMovement = true,
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 10,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
      },
    },
  };

  public Action SafetyRoll = new Action () {
    Name = "SafetyRoll",
    Steps = new ActionStep[] {
      new ActionStep () {
        Duration = 5,
        IsCancellableBy = CANCELLABLE_BY.NONE,
        AllowsFlip = true,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
        StartDisplacement = new Vector2(0, 0),
        UpdateDisplacement = new Vector2(0, 0),
      }, new ActionStep () {
        Duration = 5,
        IsCancellableBy = CANCELLABLE_BY.NONE,
        AllowsFlip = false,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
        StartDisplacement = new Vector2(.5f, 0),
        UpdateDisplacement = new Vector2(.0f, 0),
      }, new ActionStep () {
        Duration = 3,
        IsCancellableBy = CANCELLABLE_BY.NONE,
        AllowsFlip = false,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
        StartDisplacement = new Vector2(.5f, 0),
        UpdateDisplacement = new Vector2(.1f, 0),
      }, new ActionStep () {
        Duration = 3,
        IsCancellableBy = CANCELLABLE_BY.NONE,
        AllowsFlip = false,
        PhysicsShape = new HitBox(0, 0, 1, 1f),
        StartDisplacement = new Vector2(.5f, 0),
        UpdateDisplacement = new Vector2(0, 0),
      },
    },
  };

  void Awake () {
    Idle.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Idle"));
    Crouch.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Crouch"));
    Walk.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Walk"));
    Jump.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Jump"));
    Fall.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Fall"));
    WallSlide.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/WallSlide"));
    Land.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/Land"));
    PreSafetyRoll.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/PreSafetyRoll"));
    SafetyRoll.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/SafetyRoll"));
    TestAttack.Initialize(Resources.LoadAll<Sprite>($"Sprites/Test/Player/TestAttack"));
  }
}

[CustomEditor(typeof(PlayerActions))]
public class PlayerActionsEditor : Editor {
  private ActionInspector action_inspector = new ActionInspector();
  public override void OnInspectorGUI() {
    var player = (PlayerActions) target;
    var actions = new [] { 
      player.Idle,
      player.Walk,
      player.Crouch,
      player.Jump,
      player.Fall,
      player.WallSlide,
      player.Land,
      player.PreSafetyRoll,
      player.SafetyRoll,
      player.TestAttack
    };

    action_inspector.Draw(actions);
  }
}
