// File: Assets/_Project/Scripts/States/Player States/PlayerGroundedState.cs
// Version: 2.0 (Fully Refactored)

using UnityEngine;

namespace Platformer
{
    public class PlayerGroundedState : PlayerBaseState
    {
        public PlayerGroundedState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) {}

        public override void OnEnter()
        {
            player.SetDebugColor(player.groundedColor);
            player.Movement.JumpsRemaining = 1;
        }

        public override void Update(Vector2 moveInput)
        {
            if (player.ConsumeJumpBuffer() && player.Movement.IsGrounded)
            {
                var v = player.Movement.Velocity;
                v.y = Mathf.Sqrt(player.Movement.initialJumpHeight * -2f * player.Movement.gravity);
                player.Movement.Velocity = v;
            }

            if (moveInput == Vector2.zero)
            {
                stateMachine.SetState(new PlayerIdleState(player, stateMachine));
            }
        }

        public override void FixedUpdate()
        {
            player.Movement.Tick(player.MoveInput);
        }
    }
}