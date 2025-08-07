// File: Assets/_Project/Scripts/States/PlayerAirborneState.cs
// Fix: Explicitly specified Platformer.Core.Logger to resolve ambiguity.

using UnityEngine;

namespace Platformer
{
    public class PlayerAirborneState : State
    {
        private float _jumpHoldTimer;
        
        public PlayerAirborneState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) {}

        public override void OnEnter()
        {
            _jumpHoldTimer = player.Movement.jumpHoldDuration;
            player.SetDebugColor(player.airborneColor);
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.StateManagement, "[PlayerAirborneState] Entered", player);
        }

        public override void Update()
        {
            if (player.ConsumeAttackPress() && !player.Combat.IsAttackOnCooldown)
            {
                stateMachine.ChangeState(new PlayerAttackState(player, stateMachine));
                return;
            }

            if (player.ConsumeJumpBuffer() && player.Movement.JumpsRemaining > 0)
            {
                player.Movement.JumpsRemaining--;
                var v = player.Movement.Velocity;
                v.y = player.Movement.doubleJumpBoostVelocity;
                player.Movement.Velocity = v;
                player.SetDebugColor(player.doubleJumpColor);
                Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Movement, "Double Jump!", player);
            }
            else if (player.Movement.Velocity.y > 0 && player.IsJumpButtonPressed && _jumpHoldTimer > 0)
            {
                var v = player.Movement.Velocity;
                v.y += player.Movement.gravity * (1f - player.Movement.jumpCutOffMultiplier) * Time.deltaTime;
                player.Movement.Velocity = v;
                _jumpHoldTimer -= Time.deltaTime;
                player.SetDebugColor(player.highJumpColor);
            }
            else
            {
                if (!player.IsJumpButtonPressed && player.Movement.Velocity.y > 0)
                {
                    var v = player.Movement.Velocity;
                    v.y += player.Movement.gravity * player.Movement.jumpCutOffMultiplier * Time.deltaTime;
                    player.Movement.Velocity = v;
                }
            }
        }

        public override void FixedUpdate()
        {
            PlayerStateUtilities.SafeMovementTick(player, player.MoveInput);
        }

        public override void OnExit()
        {
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.StateManagement, "[PlayerAirborneState] Exited", player);
        }
    }
}