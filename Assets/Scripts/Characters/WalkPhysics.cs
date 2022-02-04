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

  public float AirAccelerationTime = .3f;
  public float AirDecelerationTime = .2f;
  public float AirPassiveDecelerationTime = .2f;

  public float JumpTime = .3f;
  public float JumpHeight = 2.3f;
  public float JumpSharpness = 1.3f;
  public float GravityAcceleration = 1.5f;

  public float WallHangFallAcceleration = .5f;
  public float WallHangFallSpeedMax = 2f;
  public float WallHangDeceleration = 2f;
  public float WallHangSize = .5f;
  public float WallHangOffset = .2f;
  public float WallJumpHorizontalSpeed = 10f;

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

  // Wall Hang
  private Wall colliding_wall = null;
  private bool wall_hanging = false;
  
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

    if (jumping) {
      grounded = false;
      Y = jump_initial_y + JumpHeight * Mathf.Sqrt(1 - Mathf.Pow(1 - (jump_elapsed_time / JumpTime), JumpSharpness));
      
      jump_elapsed_time += 1f / Game.FRAME_RATE;
      bool ceiling_collision = Game.CurrentChunk.Ceilings.Any(ceiling => {
        if ( (X > ceiling.X && X < ceiling.XW) || (XW > ceiling.X && XW < ceiling.XW) || (X < ceiling.X && XW > ceiling.XW) ) {
          if ( (previous_position.y + this.Box.H < ceiling.Y && YH >= ceiling.Y) ) {
            Y = ceiling.Y - this.Box.H;
            return true;
          }
        }

        return false;
      });

      if (ceiling_collision || jump_elapsed_time >= JumpTime) {
        velocity.y = 0;
        jumping = false;
        listener.OnJumpEnd();
        listener.OnFallStart();
      }
      else velocity.y = Y - previous_position.y;
    }
    else if (!grounded) {
      if ( input_x != 0
        && colliding_wall != null
        && input_x == (colliding_wall?.Side == SIDE.LEFT ? 1 : -1)
        && WallHangSize < colliding_wall.Height
        && Y + WallHangOffset > colliding_wall.Y
        && Y + WallHangOffset + WallHangSize < colliding_wall.YH
      ) { // Wall Hang
        wall_hanging = true;
        if (velocity.y > 0) {
          velocity.y -= WallHangDeceleration / Game.FRAME_RATE;
          Mathf.Clamp(velocity.y, 0, Mathf.Infinity);
        } else {
          velocity.y -= WallHangFallAcceleration / Game.FRAME_RATE;
          Mathf.Clamp(velocity.y, -WallHangFallSpeedMax / Game.FRAME_RATE, Mathf.Infinity);
        }
        Y += velocity.y;
      } else {
        wall_hanging = false;
        velocity.y -= GravityAcceleration / Game.FRAME_RATE;
        Y += velocity.y;
      }
    }

    #endregion Vertical

    #region Horizontal

    void Accelerate (float rate, float max) {
      velocity.x += rate * input_x;
      velocity.x = Mathf.Clamp(velocity.x, -max / Game.FRAME_RATE, max / Game.FRAME_RATE);
    }

    if (grounded) {
      var acceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (WalkAccelerationTime * Game.FRAME_RATE);
      var deceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (WalkDecelerationTime * Game.FRAME_RATE);

      if (input_x != 0) {
        if (velocity.x == 0)
          Accelerate(acceleration_rate, WalkSpeed);
        else if (Mathf.Sign(input_x) == Mathf.Sign(velocity.x)) { // input direction equals velocity direction
          if (Mathf.Abs(velocity.x) > WalkSpeed / Game.FRAME_RATE) // if passing top speed, decelerate
            Accelerate(-deceleration_rate, WalkSpeed);
          else Accelerate(acceleration_rate, WalkSpeed);
        }
        else // walking against velocity
          velocity.x += deceleration_rate * input_x;
      }
      else { // no input
        if (velocity.x != 0) {
          var sign = Mathf.Sign(velocity.x);
          velocity.x -= deceleration_rate * sign;
          if (sign != Mathf.Sign(velocity.x)) // if we overcompensated
            velocity.x = 0;
        }
      }
    }
    else { // if airborne
      if (input_x != 0) {
        var acceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (AirAccelerationTime * Game.FRAME_RATE);
        var deceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (AirDecelerationTime * Game.FRAME_RATE);

        if (velocity.x == 0)
          Accelerate(acceleration_rate, WalkSpeed);
        else if (Mathf.Sign(input_x) == Mathf.Sign(velocity.x))
          Accelerate(acceleration_rate, WalkSpeed);
        else // input against velocity
          velocity.x += deceleration_rate * input_x;
      }
      else { // no input
        if (velocity.x != 0) {
          var old_sign = Mathf.Sign(velocity.x);
          velocity.x -= WalkSpeed / (AirPassiveDecelerationTime * Game.FRAME_RATE) * old_sign;
          if (old_sign != Mathf.Sign(velocity.x))
            velocity.x = 0;
        }
      }
    }

    X += velocity.x;

    #endregion Horizontal

    CheckFallCollision();
    CheckHorizontalCollision();

    #region Debug

    if (DebugOptions.DrawPlayerMovementCollision) {
      DebugUtils.DrawRectangle(X, Y, Box.W, Box.H, Color.green);

      if (!grounded) {
        DebugUtils.DrawRectangle(X, Y + WallHangOffset, Box.W, WallHangSize, wall_hanging ? Color.blue : Color.green);
      }
    }

    if (DebugOptions.DrawPlayerMovementInputs) {
      var mid = new Vector2(X + Box.W / 2, Y + Box.H / 2);

      if (input_x != 0) {
        DebugUtils.DrawThickLine(mid.x, Y, input_x == 1 ? XW : X, Y, Color.white);
        DebugUtils.DrawThickLine(mid.x, Y, mid.x + Box.W / 2 * (Velocity.x / (WalkSpeed / Game.FRAME_RATE)), Y, Color.red);
      }
      if (jumping) {
        DebugUtils.DrawThickLine(mid.x, mid.y, mid.x, YH, Color.white);
        DebugUtils.DrawThickLine(mid.x, mid.y, mid.x, mid.y + (Box.H / 2) * (jump_elapsed_time / JumpTime), Color.red);
      }
    }

    #endregion Debug
  }

  #endregion Logic

  #region Input

  public void CheckFallCollision () {
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
    previous_position.y = Y;
  }

  public void CheckHorizontalCollision () {
    var delta_x = X - previous_position.x;
    colliding_wall = null;

    if (delta_x > 0) Game.CurrentChunk.WallsLeft.Any((wall => {
      if (wall.CheckCollision(previous_position.x + this.Box.W, XW, Y, (float)this.Box.H)) {
        X = wall.X - this.Box.W;
        Mathf.Clamp(velocity.x, -Mathf.Infinity, 0);
        colliding_wall = wall;
        return true;
      }
      return false;
    }));
    else if (delta_x < 0) Game.CurrentChunk.WallsRight.Any((System.Func<Wall, bool>)(wall => {
      if (wall.CheckCollision(previous_position.x, X, Y, (float)this.Box.H)) {
        X = wall.X;
        Mathf.Clamp(velocity.x, 0, Mathf.Infinity);
        colliding_wall = wall;
        return true;
      }
      return false;
    }));

    previous_position.x = X;
  }

  public override void Displace (Vector2 displacement) {
    X += displacement.x;
    Y += displacement.y;
    if (displacement.y < 0) CheckFallCollision();
    if (displacement.x != 0) CheckHorizontalCollision();
  }

  public override void CheckCollision () {
    CheckHorizontalCollision();
    CheckFallCollision();
  }

  public void Jump () {
    if (grounded) {
      jumping = true;
      jump_initial_y = Y;
      jump_elapsed_time = 0;
    }
    else if (wall_hanging) {
      jumping = true;
      jump_initial_y = Y;
      jump_elapsed_time = 0;
      velocity.x = (WallJumpHorizontalSpeed / Game.FRAME_RATE) * (int) colliding_wall.Side;
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
    component.AirAccelerationTime = EditorGUILayout.FloatField("Air Acceleration Time", component.AirAccelerationTime);
    component.AirDecelerationTime = EditorGUILayout.FloatField("Air Deceleration Time", component.AirDecelerationTime);

    EditorGUILayout.Separator();

    EditorGUILayout.LabelField("Wall Hang", EditorStyles.boldLabel);
    component.WallHangFallSpeedMax = EditorGUILayout.FloatField("Max Speed", component.WallHangFallSpeedMax);
    component.WallHangFallAcceleration = EditorGUILayout.FloatField("Acceleration", component.WallHangFallAcceleration);
    component.WallHangDeceleration = EditorGUILayout.FloatField("Deceleration", component.WallHangDeceleration);
    component.WallHangSize = EditorGUILayout.FloatField("Size", component.WallHangSize);
    component.WallHangOffset = EditorGUILayout.FloatField("Offset", component.WallHangOffset);

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

    if (DebugOptions.FastPlayerInspector) Repaint();
  }
}

#endregion Inspector
