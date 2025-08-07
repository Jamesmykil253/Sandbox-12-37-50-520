// File: Assets/_Project/Scripts/Characters/PlayerMovement.cs
// Version: 2.0 (Architecturally Compliant)

using UnityEngine;

namespace Platformer
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float initialJumpHeight = 2f;
        public float doubleJumpBoostVelocity = 5f;
        public float jumpHoldDuration = 0.25f;
        public float jumpCutOffMultiplier = 0.75f;
        public float gravity = -30f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float groundCheckRadius = 0.5f;
        [SerializeField] private Vector3 groundCheckOffset;
        
        public bool IsGrounded { get; private set; }
        public Vector3 Velocity { get; set; }
        public int JumpsRemaining { get; set; }

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public void Tick(Vector2 input)
        {
            CheckGroundedStatus();
            
            if (!IsGrounded)
            {
                Velocity = new Vector3(Velocity.x, Velocity.y + gravity * Time.deltaTime, Velocity.z);
            }

            float speed = GetComponent<CharacterStats>().baseStats.Speed;
            _horizontalVelocity = new Vector3(input.x, 0, input.y) * speed;
            Velocity = new Vector3(_horizontalVelocity.x, Velocity.y, _horizontalVelocity.z);

            _controller.Move(Velocity * Time.deltaTime);

            if (input != Vector2.zero)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(input.x, 0, input.y));
            }
            
            if (IsGrounded && Velocity.y < 0)
            {
                Velocity = new Vector3(Velocity.x, -2f, Velocity.z);
            }
        }

        private void CheckGroundedStatus()
        {
            Vector3 spherePosition = transform.position + groundCheckOffset;
            IsGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayerMask, QueryTriggerInteraction.Ignore);
        }
    }
}