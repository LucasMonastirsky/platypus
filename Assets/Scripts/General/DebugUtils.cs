using UnityEngine;

public static class DebugUtils {
  public static void DrawThickLine (float start_x, float start_y, float end_x, float end_y, Color color) {
    DrawThickLine(new Vector2(start_x, start_y), new Vector2(end_x, end_y), color, DebugOptions.LineThickness);
  }
  public static void DrawThickLine (Vector2 start, Vector2 end, Color color, float thickness, int segments = 8) {
    if (start.x != end.x && start.y == end.y) {
      var top_left = new Vector2(start.x, start.y + thickness / 2);
      var top_right = new Vector2(end.x, end.y + thickness / 2);
      var bottom_left = new Vector2(start.x, start.y - thickness / 2);
      var bottom_right = new Vector2(end.x, end.y - thickness / 2);

      Debug.DrawLine(top_left, top_right, color);
      Debug.DrawLine(bottom_left, bottom_right, color);

      Debug.DrawLine(top_left, bottom_left, color);
      Debug.DrawLine(top_right, bottom_right, color);

      for (int i = 1; i < segments; ++i) Debug.DrawLine(
        new Vector2(start.x, start.y - (thickness / 2 ) + (thickness / segments) * i),
        new Vector2(end.x, end.y - (thickness / 2 ) + (thickness / segments) * i),
        color
      );
    }
    else if (start.y != end.y && start.x == end.x) {
      Debug.DrawLine(new Vector2(start.x + thickness / 2, start.y), new Vector2(end.x + thickness / 2, end.y), color);
      Debug.DrawLine(new Vector2(start.x - thickness / 2, start.y), new Vector2(end.x - thickness / 2, end.y), color);
    }
  }

  public static void DrawRectangle (float x, float y, float w, float h, Color color) {
    Debug.DrawLine(new Vector2(x, y), new Vector2(x + w, y), color);
    Debug.DrawLine(new Vector2(x + w, y), new Vector2(x + w, y + h), color);
    Debug.DrawLine(new Vector2(x + w, y + h), new Vector2(x, y + h), color);
    Debug.DrawLine(new Vector2(x, y + h), new Vector2(x, y), color);
  }
}
