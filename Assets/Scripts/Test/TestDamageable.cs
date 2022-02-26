using UnityEngine;

public class TestDamageable : MonoBehaviour, IDamageable {
  public int Health => health;
  private int health;

  public HitShape Shape => shape;
  private HitBox shape = new HitBox(0, 0, 1, 2);

  public void Damage(int damage) {
    health -= damage;
  }

  void Start () {
    shape.FollowTransform(transform);
  }

  void Update () {
    shape.Draw(Color.red);
  }
}
