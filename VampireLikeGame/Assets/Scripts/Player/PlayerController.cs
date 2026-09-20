using UnityEngine;

namespace VampireLike
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D _rigidbody;
        private PlayerStats _stats;
        private Vector2 _moveInput;

        public Vector2 FacingDirection { get; private set; } = Vector2.down;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _stats = GetComponent<PlayerStats>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                _moveInput = Vector2.zero;
                return;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(x, y).normalized;

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                FacingDirection = _moveInput;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = _moveInput.x < 0f;
                }
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.MovePosition(_rigidbody.position + _moveInput * (_stats.MoveSpeed * Time.fixedDeltaTime));
        }
    }
}
