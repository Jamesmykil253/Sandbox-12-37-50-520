// File: Assets/_Project/Scripts/States/Player States/PlayerBaseState.cs
// Version: 2.0 (Fully Refactored)

using UnityEngine;
using Platformer.Core;

namespace Platformer
{
    public interface IState
    {
        void OnEnter();
        void Update(Vector2 moveInput);
        void FixedUpdate();
        void OnExit();
    }

    public abstract class PlayerBaseState : IState
    {
        protected readonly PlayerController player;
        protected readonly StateMachine stateMachine;

        protected PlayerBaseState(PlayerController player, StateMachine stateMachine)
        {
            this.player = player;
            this.stateMachine = stateMachine;
        }

        public virtual void OnEnter() { }
        public virtual void Update(Vector2 moveInput) { }
        public virtual void FixedUpdate() { }
        public virtual void OnExit() { }
    }
}