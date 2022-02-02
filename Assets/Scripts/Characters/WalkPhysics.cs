using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WalkPhysics : IPhysicsComponent {

  #region Declarations

  #region Options

  public float WalkSpeed = 8;
  public float WalkAccelerationTime = .2f;
  public float WalkDecelerationTime = .1f;

  public float JumpTime = .3f;
  public float JumpHeight = 2.3f;
  public float JumpSharpness = 1.3f;
  public float GravityAcceleration = 26;
  public bool Flipped = false;

  #endregion

  #region State

  public interface EventListener {
    void OnFallStart ();
    void OnJumpEnd ();
    void OnLand ();
  }
  private EventListener listener; public WalkPhysics Listen (EventListener listener) { this.listener = listener; return this; }

  private HitBox box; public HitBox Box { get { return box; } }
  private float Direction { get { return Flipped ? -1 : 1; } }

  private bool grounded = false; public bool Grounded { get { return grounded; } }
  [SerializeField] private Vector2 velocity = new Vector2(0, 0); public Vector2 Velocity { get { return velocity; } }
  private Vector2 previous_position;

  [SerializeField] private float input_x;

  // Jumping
  private bool jumping = false; public bool Jumping { get { return jumping; } }
  [SerializeField] private float jump_elapsed_time;
  [SerializeField] private float jump_initial_y;
  
  #endregion State

  #region Helpers

  public override HitShape Shape {
    get { return box; }
    set {
      if (value is HitBox) box = (HitBox) value;
      else Debug.LogError("Tried to set WalkPhysics shape to non-box");
    }
  }
  public float OffsetHorizontal { get { return (Box.OffsetX * Direction) - (Box.W / 2); } }
  public override float X {
    get { return transform.position.x + OffsetHorizontal; }
    set { transform.position = new Vector3(value - OffsetHorizontal, transform.position.y, transform.position.z); }
  }

  public override float Y {
    get { return transform.position.y + Box.OffsetY; }
    set { transform.position = new Vector3(transform.position.x, value, transform.position.z); }
  }

  public float XW { get { return X + Box.W; } }
  public float YH { get { return Y + Box.H; } }

  #endregion Helpers

  #endregion Declarations

  #region Logic

  void Update () {
    previous_position = transform.position;
    previous_position.x += OffsetHorizontal;

    #region Vertical

    // Update position

    if (jumping) {
      grounded = false;
      Y = jump_initial_y + JumpHeight * Mathf.Sqrt(1 - Mathf.Pow(1 - (jump_elapsed_time / JumpTime), JumpSharpness));
      
      bool ceiling_collision = Game.CurrentChunk.Ceilings.Any((System.Func<Tile, bool>)(ceiling => {
        if ( (X > ceiling.X && X < ceiling.XW) || (XW > ceiling.X && XW < ceiling.XW) || (X < ceiling.X && XW > ceiling.XW) ) {
          if ( (previous_position.y + this.Box.H < ceiling.Y && YH >= ceiling.Y) ) {
            Y = ceiling.Y - this.Box.H;
            return true;
          }
        }

        return false;
      }));

      jump_elapsed_time += 1f / Game.FRAME_RATE;

      if (ceiling_collision || jump_elapsed_time >= JumpTime) {
        velocity.y = 0;
        jumping = false;
        listener.OnJumpEnd();
        listener.OnFallStart();
      }
      else velocity.y = Y - previous_position.y;
    } else if (!grounded) {
      velocity.y -= GravityAcceleration / Game.FRAME_RATE;
      Y += velocity.y / Game.FRAME_RATE;
    }

    CheckVerticalCollision();

    #endregion Vertical

    #region Horizontal

    if (input_x != 0) {
      velocity.x += ( (WalkSpeed / Game.FRAME_RATE) / (WalkAccelerationTime * Game.FRAME_RATE) ) * input_x;
      if ( Mathf.Abs(velocity.x) > (WalkSpeed / Game.FRAME_RATE) )
        velocity.x = (WalkSpeed / Game.FRAME_RATE) * input_x;
    } else if ( velocity.x != 0 ) {
      var polarity = velocity.x > 0 ? 1 : -1;
      velocity.x -= ( (WalkSpeed / Game.FRAME_RATE) / (WalkDecelerationTime * Game.FRAME_RATE) ) * polarity;
      if ( polarity != (velocity.x > 0 ? 1 : -1) )
        velocity.x = 0;
    }
    X += velocity.x;

    CheckHorizontalCollision();

    #endregion Horizontal

    if (DebugOptions.DrawPlayerMovementCollision) {
      Debug.DrawLine(new Vector2(X, YH), new Vector2(XW, YH), Color.green);
      Debug.DrawLine(new Vector2(X, Y), new Vector2(XW, Y), Color.green);
      Debug.DrawLine(new Vector2(X, Y), new Vector2(X, YH), Color.green);
      Debug.DrawLine(new Vector2(XW, Y), new Vector2(XW, YH), Color.green);
    }
  }

  #endregion Logic

  #region Input

  public void CheckVerticalCollision () {
    grounded = Game.CurrentChunk.Floors.Any(floor => {
      if ( (X > floor.X && X < floor.XW) || (XW > floor.X && XW < floor.XW) ) {
        if ( (previous_position.y > floor.YH && Y <= floor.YH) || Y == floor.YH ) {
          if ( Y < floor.YH ) {
            Y = floor.YH;
            listener.OnLand();
          }
          velocity.y = 0;
          return true;
        }
      }
      return false;
    });
  }

  public void CheckHorizontalCollision () {
    var delta_x = X - previous_position.x;

    if (delta_x > 0) Game.CurrentChunk.WallsLeft.Any((wall => {
      if (wall.CheckCollision(previous_position.x + this.Box.W, XW, Y, (float)this.Box.H)) {
        X = wall.X - this.Box.W;
        velocity.x = 0;
        return true;
      }
      return false;
    }));
    else if (delta_x < 0) Game.CurrentChunk.WallsRight.Any((System.Func<Wall, bool>)(wall => {
      if (wall.CheckCollision(previous_position.x, X, Y, (float)this.Box.H)) {
        X = wall.X;
        velocity.x = 0;
        return true;
      }
      return false;
    }));
  }

  public override void Displace (Vector2 displacement) {
    X += displacement.x;
    Y += displacement.y;
    CheckCollision();
  }

  public override void CheckCollision () {
    CheckHorizontalCollision();
    CheckVerticalCollision();
  }

  public void Jump () {
    if (grounded) {
      jumping = true;
      jump_initial_y = Y;
      jump_elapsed_time = 0;
    }
  }

  public void StopJump () {
    if (jumping) {
      jumping = false;
      listener.OnJumpEnd();
      listener.OnFallStart();
    }
  }

  public void Walk (float input) {
    input_x = input;
  }

  #endregion
}

#region Inspector

[CustomEditor(typeof(WalkPhysics))]
public class WalkPhysicsEditor : Editor {
  private SerializedProperty jump_elapsed_time, input_x, velocity;
  void OnEnable () {
    jump_elapsed_time = serializedObject.FindProperty("jump_elapsed_time");
    input_x = serializedObject.FindProperty("input_x");
    velocity = serializedObject.FindProperty("velocity");
  }

  public override void OnInspectorGUI () {
    var component = (WalkPhysics) target;

    EditorGUILayout.LabelField("Jump", EditorStyles.boldLabel);
      
    component.JumpHeight = EditorGUILayout.FloatField("Height", component.JumpHeight);
    component.JumpTime = EditorGUILayout.FloatField("Time", component.JumpTime);
    component.JumpSharpness = EditorGUILayout.FloatField("Sharpness", component.JumpSharpness);
    component.GravityAcceleration = EditorGUILayout.FloatField("Gravity", component.GravityAcceleration);

    EditorGUILayout.Separator();

    EditorGUILayout.LabelField("Walk", EditorStyles.boldLabel);
    component.WalkSpeed = EditorGUILayout.FloatField("Max Speed", component.WalkSpeed);
    component.WalkAccelerationTime = EditorGUILayout.FloatField("Acceleration Time", component.WalkAccelerationTime);
    component.WalkDecelerationTime = EditorGUILayout.FloatField("Deceleration Time", component.WalkDecelerationTime);

    if (Application.isPlaying) {
      EditorGUILayout.Separator();

      EditorGUILayout.LabelField("Monitoring", EditorStyles.boldLabel);
      EditorGUILayout.Toggle("Grounded", component.Grounded); 
      EditorGUILayout.Toggle("Jumping", component.Jumping);
      EditorGUILayout.Slider("Walk", velocity.vector2Value.x, -component.WalkSpeed / Game.FRAME_RATE, component.WalkSpeed / Game.FRAME_RATE);

      EditorGUILayout.Separator();

      EditorGUILayout.LabelField("Velocity", EditorStyles.boldLabel);
      EditorGUILayout.Vector2Field("Per Frame", component.Velocity);
      EditorGUILayout.Vector2Field("Per Second", component.Velocity * 60);
    }
  }

  void OnSceneGUI () {
    var walk = target as WalkPhysics;

    Handles.color = Color.green;

    if (walk.Box != null) {
      Handles.DrawLine(new Vector2(walk.X, walk.YH), new Vector2(walk.XW, walk.YH));
      Handles.DrawLine(new Vector2(walk.X, walk.Y), new Vector2(walk.XW, walk.Y));
      Handles.DrawLine(new Vector2(walk.X, walk.Y), new Vector2(walk.X, walk.YH));
      Handles.DrawLine(new Vector2(walk.XW, walk.Y), new Vector2(walk.XW, walk.YH));
    }
  }
}

#endregion Inspector