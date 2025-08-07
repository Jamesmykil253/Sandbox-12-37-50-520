// File: Assets/_Project/Scripts/States/PlayerAttackState.cs
// Fix: Explicitly specified Platformer.Core.Logger to resolve ambiguity.

using UnityEngine;

namespace Platformer
{
    public class PlayerAttackState : State
    {
        private float _attackDuration = 0.5f;
        private float _timer;
        
        public PlayerAttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) {}

        public override void OnEnter()
        {
            if (!PlayerStateUtilities.ValidateStateParameters(player, stateMachine, "PlayerAttackState.OnEnter")) return;

            player.Combat.ExecuteAttack();
            _timer = _attackDuration;
            
            bool isEmpowered = player.MyStats.IsNextAttackEmpowered();
            Color attackColor = PlayerStateUtilities.GetAttackColor(player, isEmpowered);
            player.SetDebugColor(attackColor);
            
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Combat, $"Attack state entered - {(isEmpowered ? "Empowered" : "Normal")} attack", player);
        }

        public override void Update()
        {
            if (!PlayerStateUtilities.ValidateStateParameters(player, stateMachine, "PlayerAttackState.Update")) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Combat, "Attack completed, returning to idle", player);
                stateMachine.ChangeState(new PlayerIdleState(player, stateMachine));
            }
        }

        public override void FixedUpdate()
        {
            float currentMultiplier = Mathf.Lerp(player.Combat.attackMoveSpeedMultiplier, 1f, 1f - (_timer / _attackDuration));
            Vector2 modifiedInput = player.MoveInput * currentMultiplier;
            PlayerStateUtilities.SafeMovementTick(player, modifiedInput);
        }
    }
}