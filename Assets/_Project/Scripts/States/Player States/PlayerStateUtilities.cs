// File: Assets/_Project/Scripts/States/PlayerStateUtilities.cs
// Fix: Explicitly specified Platformer.Core.Logger to resolve ambiguity.

using UnityEngine;

namespace Platformer
{
    public static class PlayerStateUtilities
    {
        public static bool ValidateStateParameters(PlayerController player, StateMachine stateMachine, string context)
        {
            if (player == null)
            {
                Platformer.Core.Logger.Error(Platformer.Core.Logger.LogCategory.StateManagement, $"[{context}] PlayerController is null!");
                return false;
            }
            if (stateMachine == null)
            {
                Platformer.Core.Logger.Error(Platformer.Core.Logger.LogCategory.StateManagement, $"[{context}] StateMachine is null!", player);
                return false;
            }
            return true;
        }

        public static void EnterGroundedState(PlayerController player, Color debugColor, string stateName)
        {
            player.SetDebugColor(debugColor);
            player.Movement.JumpsRemaining = 1;
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.StateManagement, $"[{stateName}] Entered", player);
        }

        public static bool HandleCommonGroundedTransitions(PlayerController player, StateMachine stateMachine)
        {
            if (player.Scoring.CanStartScoring() && player.IsScoreButtonPressed)
            {
                stateMachine.ChangeState(new PlayerScoringState(player, stateMachine));
                return true;
            }

            if (player.ConsumeAttackPress() && !player.Combat.IsAttackOnCooldown)
            {
                stateMachine.ChangeState(new PlayerAttackState(player, stateMachine));
                return true;
            }

            if (player.ConsumeJumpBuffer())
            {
                var v = player.Movement.Velocity;
                v.y = Mathf.Sqrt(player.Movement.initialJumpHeight * -2f * player.Movement.gravity);
                player.Movement.Velocity = v;
                return true; 
            }
            return false;
        }

        public static void HandleMovementStateTransitions(PlayerController player, StateMachine stateMachine, bool currentStateIsIdle)
        {
            if (currentStateIsIdle && player.MoveInput != Vector2.zero)
            {
                stateMachine.ChangeState(new PlayerGroundedState(player, stateMachine));
            }
            else if (!currentStateIsIdle && player.MoveInput == Vector2.zero)
            {
                stateMachine.ChangeState(new PlayerIdleState(player, stateMachine));
            }
        }

        public static void SafeMovementTick(PlayerController player, Vector2 input)
        {
            if (player.Movement != null)
            {
                player.Movement.Tick(input);
            }
            else
            {
                Platformer.Core.Logger.Error(Platformer.Core.Logger.LogCategory.Movement, "PlayerMovement component is null in SafeMovementTick!", player);
            }
        }

        public static Color GetAttackColor(PlayerController player, bool isEmpowered)
        {
            return isEmpowered ? player.empoweredAttackColor : player.attackColor;
        }
    }
}