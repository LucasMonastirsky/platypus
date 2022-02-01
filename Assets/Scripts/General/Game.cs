using UnityEditor;
using UnityEngine;

public class Game : MonoBehaviour {
  public static int FRAME_RATE = 60;

  [SerializeField] private Chunk current_chunk;
  public static Chunk CurrentChunk { get { return instance.current_chunk; } }
  private static Game instance;

  void Awake () {
    Application.targetFrameRate = FRAME_RATE;
    Game.instance = this;
  }
}


