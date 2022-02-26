using System.Linq;
using UnityEditor;
using UnityEngine;

public class WalkPhysics : IPhysicsComponent {

  #region Declarations

  #region Options

  public float WalkSpeed = 8;
  public float WalkAccelerationTime = .2f;
  public float WalkDecelerationTime = .1f;
  public float WalkOverflowDecelerationTime = .5f;

  public float AirAccelerationTime = .3f;
  public float AirDecelerationTime = .2f;
  public float AirPassiveDecelerationTime = 1f;

  public float JumpTime = .3f;
  public float JumpHeight = 2.3f;
  public float JumpSharpness = 1.3f; // the lower the value, the smoother the jump will be
  public float GravityAcceleration = 90f; // meters per second squared
  public float HardLandingThreshold = 30f; // velocity per second required to trigger a hard landing

  public float WallHangFallAcceleration = 20f;
  public float WallHangDeceleration = 20f;
  public float WallHangFallSpeedMax = 2f;
  public float WallHangSize = .5f; // vertical size of the collision box
  public float WallHangOffset = .2f; // distance from Y that the collision box starts at
  public float WallJumpHorizontalSpeed = 15f;

  public float CompensationRate = 2; // speed of movement when compensating for the collision box overlapping a wall

  public bool Flipped = false;

  #endregion

  #region State

  public interface EventListener {
    void OnFallStart ();
    void OnJumpEnd ();
    void OnLand ();
    void OnHardLand ();
    void OnWallSlideStart ();
  }
  private EventListener listener;
  public WalkPhysics Listen (EventListener listener) { this.listener = listener; return this; }

  private HitBox box; public HitBox Box { get { return box; } }
  private float Direction { get { return Flipped ? -1 : 1; } }

  private bool grounded = false; public bool Grounded { get { return grounded; } }
  [SerializeField] private Vector2 velocity = new Vector2(0, 0); public Vector2 Velocity { get { return velocity; } }
  private Vector2 previous_position;

  [SerializeField] private float input_x;

  public bool IgnorePlatforms = false;

  // Jumping
  private bool jumping = false; public bool Jumping { get { return jumping; } }
  [SerializeField] private float jump_elapsed_time;
  [SerializeField] private float jump_initial_y;

  // Wall Hang
  private Wall colliding_wall = null;
  private bool wall_hanging = false;

  private Wall overlapping_wall = null;
  
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

  public float CenterX { get { return X + Box.W / 2; } }

  #endregion Helpers

  #endregion Declarations

  #region Logic

  void Update () {
    if (Paused) return;

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
        if (!wall_hanging) {
          wall_hanging = true;
          listener.OnWallSlideStart();
        }
        if (velocity.y > 0) {
          velocity.y -= (WallHangDeceleration / Game.FRAME_RATE) / Game.FRAME_RATE;
          Mathf.Clamp(velocity.y, 0, Mathf.Infinity);
        } else {
          velocity.y -= (WallHangFallAcceleration / Game.FRAME_RATE) / Game.FRAME_RATE;
          Mathf.Clamp(velocity.y, -WallHangFallSpeedMax / Game.FRAME_RATE, Mathf.Infinity);
        }
        Y += velocity.y;
      } else {
        wall_hanging = false;
        velocity.y -= (GravityAcceleration / Game.FRAME_RATE) / Game.FRAME_RATE;
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
      var overflow_deceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (WalkOverflowDecelerationTime * Game.FRAME_RATE);
      Debug.Log($"{acceleration_rate} | {overflow_deceleration_rate} | {WalkOverflowDecelerationTime}");

      if (input_x != 0 && AllowMovement) {
        if (velocity.x == 0)
          Accelerate(acceleration_rate, WalkSpeed);
        else if (Mathf.Sign(input_x) == Mathf.Sign(velocity.x)) { // input direction equals velocity direction
          if (Mathf.Abs(velocity.x) > WalkSpeed / Game.FRAME_RATE) {// if passing top speed, decelerate
            velocity.x -= overflow_deceleration_rate * input_x;
            if (Mathf.Abs(velocity.x) < WalkSpeed / Game.FRAME_RATE) velocity.x = WalkSpeed * input_x / Game.FRAME_RATE;
          }
          else Accelerate(acceleration_rate, WalkSpeed);
        }
        else // walking against velocity
          velocity.x += deceleration_rate * input_x;
      }
      else { // no input or movement is not allowed
        if (velocity.x != 0) {
          var sign = Mathf.Sign(velocity.x);
          velocity.x -= deceleration_rate * sign;
          if (sign != Mathf.Sign(velocity.x)) // if we overcompensated
            velocity.x = 0;
        }
      }
    }
    else { // if airborne
      var passive_deceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (AirPassiveDecelerationTime * Game.FRAME_RATE);

      if (overlapping_wall != null && ((int) overlapping_wall.Side) != Mathf.Sign(velocity.x)) {
        velocity.x = 0;
      }
      else if (input_x != 0 && AllowMovement) {
        var acceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (AirAccelerationTime * Game.FRAME_RATE);
        var deceleration_rate = (WalkSpeed / Game.FRAME_RATE) / (AirDecelerationTime * Game.FRAME_RATE);

        if (velocity.x == 0)
          Accelerate(acceleration_rate, WalkSpeed);
        else if (Mathf.Sign(input_x) == Mathf.Sign(velocity.x))
          if (Mathf.Abs(velocity.x) < WalkSpeed / Game.FRAME_RATE) Accelerate(acceleration_rate, WalkSpeed);
          else velocity.x -= passive_deceleration_rate * input_x;
        else // input against velocity
          velocity.x += deceleration_rate * input_x;
      }
      else { // no input or input is not allowed
        if (velocity.x != 0) {
          var old_sign = Mathf.Sign(velocity.x);
          velocity.x -=  passive_deceleration_rate * old_sign;
          if (old_sign != Mathf.Sign(velocity.x))
            velocity.x = 0;
        }
      }
    }

    X += velocity.x;

    #endregion Horizontal

    CheckFallCollision();
    CheckHorizontalCollision();

    // compensate position if overlapping a wall
    if (overlapping_wall != null) {
      if ( Y < overlapping_wall.Y && YH < overlapping_wall.Y ) {
        overlapping_wall = null;
      }
      else if (overlapping_wall.Side == SIDE.LEFT) {
        if (input_x != -1) X -= CompensationRate / Game.FRAME_RATE;
        if ( XW < overlapping_wall.X ) {
          X = overlapping_wall.X - Box.W;
          overlapping_wall = null;
        }
      }
      else {
        if (input_x != 1) X += CompensationRate / Game.FRAME_RATE;
        if ( X > overlapping_wall.X ) {
          X = overlapping_wall.X;
          overlapping_wall = null;
        }  
      }
    }

    #region Debug

    if (DebugOptions.DrawPlayerMovementCollision) {
      DebugUtils.DrawRectangle(X, Y, Box.W, Box.H, Color.green);

      if (!grounded) {
        DebugUtils.DrawRectangle(X, Y + WallHangOffset, Box.W, WallHangSize, wall_hanging ? Color.blue : Color.green);
      }
    }

    if (DebugOptions.DrawPlayerMovementInputs) {
      var mid = new Vector2(X + Box.W / 2, Y + Box.H / 2);

      // if (input_x != 0) {
        DebugUtils.DrawThickLine(mid.x, Y, input_x == 1 ? XW : X, Y, Color.white);
        DebugUtils.DrawThickLine(mid.x, Y, mid.x + Box.W / 2 * (Velocity.x / (WalkSpeed / Game.FRAME_RATE)), Y, Color.red);
      // }
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
    var old_grounded = grounded;
    grounded = Game.CurrentChunk.Floors.Any(floor => {
      if ( floor.FallThrough && IgnorePlatforms ) return false;

      if (CenterX > floor.X && CenterX < floor.XW) {
        if ( (previous_position.y > floor.YH && Y <= floor.YH) || Y == floor.YH ) {
          if ( Y < floor.YH ) {
            Y = floor.YH;
            if (velocity.y < -HardLandingThreshold / Game.FRAME_RATE)
              listener.OnHardLand();
            else listener.OnLand();
          }
          velocity.y = 0;
          return true;
        }
      }
      else if (!floor.FallThrough && Y >= floor.Y && Y <= floor.YH ) {
        if ( floor.HasWallLeft && XW > floor.X && XW < floor.XW ) {
          overlapping_wall = floor.WallLeft;
        }
        else if ( floor.HasWallRight && X > floor.X && X < floor.XW ) {
          overlapping_wall = floor.WallRight;
        }
      }
      return false;
    });
    if (old_grounded && !grounded)
      listener.OnFallStart();
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

  public override void CheckCollision () {
    CheckHorizontalCollision();
    CheckFallCollision();
  }

  public override void Displace (Vector2 displacement) {
    X += displacement.x;
    Y += displacement.y;
    if (displacement.y < 0) CheckFallCollision();
    if (displacement.x != 0) CheckHorizontalCollision();
  }

  public void SetVelocity (float x, float y) {
    velocity.x = x;
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
    component.WalkOverflowDecelerationTime = EditorGUILayout.FloatField("Overflow Deceleration Time", component.WalkOverflowDecelerationTime);

    EditorGUILayout.LabelField("Air", EditorStyles.boldLabel);
    component.AirAccelerationTime = EditorGUILayout.FloatField("Air Acceleration Time", component.AirAccelerationTime);
    component.AirDecelerationTime = EditorGUILayout.FloatField("Air Deceleration Time", component.AirDecelerationTime);
    component.AirPassiveDecelerationTime = EditorGUILayout.FloatField("Air Passive Deceleration Time", component.AirPassiveDecelerationTime);
    component.HardLandingThreshold = EditorGUILayout.FloatField("Hard Landing Threshold", component.HardLandingThreshold);
    component.CompensationRate = EditorGUILayout.FloatField("Overlap Compensation Rate", component.CompensationRate);

    EditorGUILayout.Separator();

    EditorGUILayout.LabelField("Wall Hang", EditorStyles.boldLabel);
    component.WallHangFallSpeedMax = EditorGUILayout.FloatField("Max Speed", component.WallHangFallSpeedMax);
    component.WallHangFallAcceleration = EditorGUILayout.FloatField("Acceleration", component.WallHangFallAcceleration);
    component.WallHangDeceleration = EditorGUILayout.FloatField("Deceleration", component.WallHangDeceleration);
    component.WallHangSize = EditorGUILayout.FloatField("Size", component.WallHangSize);
    component.WallHangOffset = EditorGUILayout.FloatField("Offset", component.WallHangOffset);
    component.WallJumpHorizontalSpeed = EditorGUILayout.FloatField("Jump Horizontal Speed", component.WallJumpHorizontalSpeed);

    if (Application.isPlaying) {
      EditorGUILayout.Separator();

      EditorGUILayout.LabelField("Monitoring", EditorStyles.boldLabel);
      EditorGUILayout.Toggle("Grounded", component.Grounded); 
      EditorGUILayout.Toggle("Jumping", component.Jumping);
      EditorGUILayout.Slider("Walk", velocity.vector2Value.x, -component.WalkSpeed / Game.FRAME_RATE, component.WalkSpeed / Game.FRAME_RATE);

      EditorGUILayout.Separator();

      EditorGUILayout.LabelField("Velocity", EditorStyles.boldLabel);
      EditorGUILayout.Vector2Field("Per Frame", component.Velocity);
      EditorGUILayout.Vector2Field("Per Second", component.Velocity * Game.FRAME_RATE);
    }

    if (DebugOptions.FastPlayerInspector) Repaint();
  }
}

#endregion Inspector
