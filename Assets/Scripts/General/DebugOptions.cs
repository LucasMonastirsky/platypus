using UnityEditor;
using UnityEngine;

public class DebugOptions : MonoBehaviour {
  private static DebugOptions instance;

  public static bool FastPlayerInspector { get { return instance.fast_player_inspector; } set { instance.fast_player_inspector = value; } }
  public bool fast_player_inspector;

  public static bool DrawPlayerMovementInputs { get { return instance.draw_player_movement_inputs; } set { instance.draw_player_movement_inputs = value; } }
  public bool draw_player_movement_inputs;

  public static bool DrawPlayerMovementCollision { get { return instance.draw_player_movement_collision; } set { instance.draw_player_movement_collision = value; } }
  public bool draw_player_movement_collision;

  public static bool DrawTerrainCollision { get { return instance.draw_terrain_collision; } set { instance.draw_terrain_collision = value; } }
  public bool draw_terrain_collision;

  public static bool DrawAttackHitShapes { get { return instance.draw_attack_hit_shapes; } set { instance.draw_attack_hit_shapes = value; } }
  public bool draw_attack_hit_shapes;

  public static float LineThickness { get { return instance.line_thickness; } set { instance.line_thickness = value; } }
  public float line_thickness = .035f;

  #region Colors

  public static Color ColorPlayerCollision = Color.green;
  public static Color ColorTerrainCollision = Color.yellow;

  #endregion

  void Awake () {
    DebugOptions.instance = this;
  }

  public object this [string name] {
    get { return typeof(DebugOptions).GetField(name).GetValue(this); }
    set { typeof(DebugOptions).GetField(name).SetValue(this, value); }
  }
}

[CustomEditor(typeof(DebugOptions))]
public class DebugOptionsEditor : Editor {
  public override void OnInspectorGUI () {
    var options = (DebugOptions) target;
    void CreateCheckBox (string label, string name) {
      options[name] = EditorGUILayout.Toggle(label, (bool) options[name]);
    }

    CreateCheckBox("Fast Player Inspector", "fast_player_inspector");
    CreateCheckBox("Draw Player Movement Inputs", "draw_player_movement_inputs");
    CreateCheckBox("Draw Player Movement Collision", "draw_player_movement_collision");
    CreateCheckBox("Draw Terrain Collision", "draw_terrain_collision");
    CreateCheckBox("Draw Attack Hit Shapes", "draw_attack_hit_shapes");

    options.line_thickness = EditorGUILayout.Slider("Line Thickness", options.line_thickness, 0, .05f);
  }
}