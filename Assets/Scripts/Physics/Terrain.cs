using UnityEngine;

public class Wall {
  public SIDE Side;
  public float X, Y, Height;

  public float YH { get { return Y + Height; } }

  public Wall (float x, float y, float height, SIDE side) {
    this.X = x;
    this.Y = y;
    this.Height = height;
    this.Side = side;
  }

  public bool CheckCollision (float previous_x, float target_x, float target_y, float target_height) {
    var target_yh = target_y + target_height;

    if (Side == SIDE.LEFT) return
      ( previous_x <= X + Physics.ERROR_MARGIN && target_x >= X - Physics.ERROR_MARGIN)
      && (
        (target_y >= Y && target_y <= YH)
        || (target_yh >= Y && target_yh <= YH)
        || (target_y <= Y && target_yh >= YH)
      );
    else return (
      ( previous_x >= X - Physics.ERROR_MARGIN && target_x <= X + Physics.ERROR_MARGIN )
      && (
        (target_y >= Y && target_y <= YH)
        || (target_yh >= Y && target_yh <= YH)
        || (target_y <= Y && target_yh >= YH)
      )
    );
  }

  public void DrawCollision (Color color) {
    Debug.DrawLine(new Vector2(X, Y), new Vector2(X, YH), color);
  }

  public override string ToString() {
    return $"({X},{Y},{Height},{Side})";
  }
}
