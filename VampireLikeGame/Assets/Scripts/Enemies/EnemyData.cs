using UnityEngine;

namespace VampireLike
{
    [CreateAssetMenu(menuName = "VampireLike/Enemy", fileName = "NewEnemy")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName = "Enemy";
        public GameObject prefab;
        public float maxHealth = 10f;
        public float moveSpeed = 2f;
        public float contactDamage = 5f;
        public float contactDamageInterval = 1f;
        public float experienceValue = 1f;
    }
}
