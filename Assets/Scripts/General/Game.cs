using UnityEngine;

public class Game : MonoBehaviour {
  private static Game instance;

  public static int FRAME_RATE = 60;
  public int FrameRate = 60;

  [SerializeField] private Chunk current_chunk;
  public static Chunk CurrentChunk { get { return instance.current_chunk; } }

  void Awake () {
    Application.targetFrameRate = FRAME_RATE;
    Game.instance = this;
  }

  void Update () {
    Application.targetFrameRate = FrameRate;
  }
}
