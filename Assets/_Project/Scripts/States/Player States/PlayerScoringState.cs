using UnityEngine;

namespace Platformer
{
    public class PlayerScoringState : State
    {
        private float _scoringTimer;

        public PlayerScoringState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) {}

        public override void OnEnter()
        {
            if (player.Scoring.CurrentGoalZone == null)
            {
                Platformer.Core.Logger.Warning(Platformer.Core.Logger.LogCategory.Scoring, "Entered scoring state but CurrentGoalZone is null. Returning to Idle.", player);
                stateMachine.ChangeState(new PlayerIdleState(player, stateMachine));
                return;
            }
            _scoringTimer = player.Scoring.CoinCount * player.Scoring.CurrentGoalZone.timePerCoin;
            player.SetScoringVisual(true);
            player.SetDebugColor(Color.white);
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.StateManagement, "[PlayerScoringState] Entered", player);
        }

        public override void Update()
        {
            _scoringTimer -= Time.deltaTime;

            if (_scoringTimer <= 0f)
            {
                bool scoringSucceeded = player.Scoring.ScorePoints();
                if (scoringSucceeded)
                {
                    Platformer.Core.Logger.Info(Platformer.Core.Logger.LogCategory.Scoring, "Scoring completed successfully!", player);
                }
                stateMachine.ChangeState(new PlayerIdleState(player, stateMachine));
                return;
            }

            if (!player.IsScoreButtonPressed || player.MoveInput != Vector2.zero)
            {
                Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.Scoring, "Scoring cancelled by player action.", player);
                stateMachine.ChangeState(new PlayerIdleState(player, stateMachine));
                return;
            }
        }

        public override void OnExit()
        {
            player.SetScoringVisual(false);
            Platformer.Core.Logger.Debug(Platformer.Core.Logger.LogCategory.StateManagement, "[PlayerScoringState] Exited", player);
        }
    }
}
