using UnityEngine;

public abstract class IPhysicsComponent : MonoBehaviour {
  public abstract HitShape Shape { get; set; }
  public abstract float X { get; set; }
  public abstract float Y { get; set; }
  public abstract void Displace (Vector2 displacement);
  public void Displace (float x, float y) { Displace(new Vector2(x, y)); }
  public abstract void CheckCollision ();
}
