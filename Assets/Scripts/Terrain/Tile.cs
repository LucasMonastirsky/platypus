using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour {

  #region Options

  [SerializeField] private float height = 1; public float Height { get => height; }
  [SerializeField] private float width = 3; public float Width { get => width; }
  [SerializeField] private bool has_top = true; public bool HasTop { get => has_top; }
  [SerializeField] private bool has_bottom = true; public bool HasBottom { get => has_bottom; }
  [SerializeField] private bool has_wall_left = true; public bool HasWallLeft { get => has_wall_left; }
  [SerializeField] private bool has_wall_right = true; public bool HasWallRight { get => has_wall_right; }
  [SerializeField] private bool fall_through = false; public bool FallThrough { get => fall_through; }

  #endregion Options

  private Wall wall_left, wall_right;
  public Wall WallLeft { get { return wall_left; } }
  public Wall WallRight { get { return wall_right; } }

  public float X {
    get { return this.transform.position.x; }
    private set { this.transform.position = new Vector3(value, this.transform.position.y, this.transform.position.z); }
  }

  public float Y {
    get { return this.transform.position.y; }
    private set { this.transform.position = new Vector3(this.transform.position.x, value, this.transform.position.z); }
  }

  public float XW { get { return X + Width; } }

  public float YH { get { return Y + Height; } }

  void Awake () {
    if (HasWallLeft) wall_left = new Wall(X, Y, Height, SIDE.LEFT);
    if (HasWallRight) wall_right = new Wall(XW, Y, Height, SIDE.RIGHT);
  }

  void Update () {
    if (DebugOptions.DrawTerrainCollision) {
      if (HasTop) Debug.DrawLine(new Vector3(X, YH), new Vector3(XW, YH), Color.yellow);
      if (HasBottom) Debug.DrawLine(new Vector3(X, Y), new Vector3(XW, Y));
      if (HasWallLeft) WallLeft.DrawCollision(Color.yellow);
      if (HasWallRight) WallRight.DrawCollision(Color.yellow);
    }
  }
}

[CustomEditor(typeof(Tile))] [CanEditMultipleObjects]
public class TileEditor : Editor {

  public override void OnInspectorGUI () {
    DrawDefaultInspector();
  }

  void OnSceneGUI () {
    var tile = target as Tile;

    Handles.color = Color.yellow;

    if (tile.HasTop) Handles.DrawLine(new Vector3(tile.X, tile.YH), new Vector3(tile.XW, tile.YH));
    if (tile.HasBottom) Handles.DrawLine(new Vector3(tile.X, tile.Y), new Vector3(tile.XW, tile.Y));
    if (tile.HasWallLeft) Handles.DrawLine(new Vector3(tile.X, tile.Y), new Vector3(tile.X, tile.YH));
    if (tile.HasWallRight) Handles.DrawLine(new Vector3(tile.XW, tile.Y), new Vector3(tile.XW, tile.YH));
  }

}
