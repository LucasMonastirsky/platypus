public interface IDamageable {
  int Health { get; }
  HitShape Shape { get; }
  void Damage (int damage);
}
