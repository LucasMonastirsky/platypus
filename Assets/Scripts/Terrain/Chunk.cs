using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Chunk : MonoBehaviour {

  [SerializeField] private float width = 20; public float Width { get { return width; } }

  [SerializeField] private float height = 20; public float Height { get { return height; } }

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

  #region Terrain

  private List<Tile> floors = new List<Tile>(); public List<Tile> Floors { get { return floors; } }
  private List<Tile> ceilings = new List<Tile>(); public List<Tile> Ceilings { get { return ceilings; } } 
  private List<Wall> walls_left = new List<Wall>(); public List<Wall> WallsLeft { get { return walls_left; } }
  private List<Wall> walls_right = new List<Wall>(); public List<Wall> WallsRight { get { return walls_right; } }

  #endregion Terrain

  private List<IDamageable> damageables = new List<IDamageable>(); public List<IDamageable> Damageables { get { return damageables; } }

  void Start () {
    #region Find Terrain

    new List<GameObject>(GameObject.FindGameObjectsWithTag(TAGS.TERRAIN)).ForEach(terrain => {
      var terrain_pos = terrain.transform.position;
      if (terrain_pos.x >= this.X && terrain_pos.x < this.X + this.Width
      && terrain_pos.y >= this.Y && terrain_pos.y < this.Y + this.Height) {
        var tile = terrain.GetComponent<Tile>();
        if (tile == null)
          Debug.LogWarning("Found tile with no tile component: " + terrain.name);
        else {
          if (tile.HasTop) floors.Add(tile);
          if (tile.HasBottom) ceilings.Add(tile);
          if (tile.HasWallLeft) walls_left.Add(tile.WallLeft);
          if (tile.HasWallRight) walls_right.Add(tile.WallRight);
        }
      }
    });
    new List<GameObject>(GameObject.FindGameObjectsWithTag(TAGS.DAMAGEABLE)).ForEach(damageable => {
      var component = damageable.GetComponent<IDamageable>();
      if (component == null)
        Debug.LogError("Found object with tag Damageable but not IDamageable component");
      else
        damageables.Add(component);
    });

    #endregion
  }
  
}

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor {

  public override void OnInspectorGUI () {
    DrawDefaultInspector();
  }

  void OnSceneGUI () {
    var chunk = target as Chunk;

    Handles.color = Color.yellow;

    Handles.DrawLine(new Vector3(chunk.X, chunk.Y), new Vector3(chunk.X + chunk.Width, chunk.Y));
    Handles.DrawLine(new Vector3(chunk.X + chunk.Width, chunk.Y), new Vector3(chunk.X + chunk.Width, chunk.Y + chunk.Height));
    Handles.DrawLine(new Vector3(chunk.X + chunk.Width, chunk.Y + chunk.Height), new Vector3(chunk.X, chunk.Y + chunk.Height));
    Handles.DrawLine(new Vector3(chunk.X , chunk.Y + chunk.Height), new Vector3(chunk.X , chunk.Y));
  }

}
