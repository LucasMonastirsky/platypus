using UnityEngine;

public abstract class HitShape {
  public float OffsetX, OffsetY;
  private int direction = 1;
  public int Direction {
    get { return direction; }
    set { direction = value < 0 ? -1 : 1; }
  }
  public abstract float X { get; }
  public abstract float Y { get; }

  protected Transform transform;

  public HitShape FollowTransform (Transform transform) {
    this.transform = transform;
    return this;
  }

  public abstract bool Overlaps (HitShape other);
  public abstract bool Overlaps (Vector2 point);
  public bool Overlaps (float x, float y) { return Overlaps(new Vector2(x, y)); }

  public abstract void Draw (Color color);
}

public class HitBox : HitShape {
  public float W, H;
  public float XW { get { return X + W; } }
  public float YH { get { return Y + H; } }
  public override float X { get {
    var displacement = Direction == 1 ? OffsetX : -(OffsetX + W);
    return transform != null ? transform.position.x + displacement : displacement;
  }}
  public override float Y { get { return transform != null ? transform.position.y + OffsetY : OffsetY; } }

  public HitBox (float offset_x, float offset_y, float w, float h) {
    OffsetX = offset_x; OffsetY = offset_y; W = w; H = h;
  }

  public override bool Overlaps (HitShape other) {
    if (other is HitBox) {
      var box = (HitBox) other;

      if (X < box.X)
        if (XW > box.X)
          return (Y > box.Y)
            ? Y < box.YH
            : YH > box.Y;
        else return false;
      else
        if (X < box.XW)
          return (Y > box.Y)
            ? Y < box.YH
            : YH > box.Y;
        else return false;
    }

    Debug.LogError("Unhandled type in HitShape.Overlap");
    return false;
  }

  public override bool Overlaps(Vector2 point) {
    return (
      point.x > X
      && point.x < XW
      && point.y > Y
      && point.y < YH
    );
  }

  public override void Draw (Color color) {
    Debug.DrawLine(new Vector3(X, Y), new Vector3(XW, Y), color);
    Debug.DrawLine(new Vector3(XW, Y), new Vector3(XW, YH), color);
    Debug.DrawLine(new Vector3(XW, YH), new Vector3(X, YH), color);
    Debug.DrawLine(new Vector3(X, YH), new Vector3(X, Y), color);
  }
}
