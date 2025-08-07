// File: Assets/_Project/Scripts/Core/State.cs
// Version: 1.1 (Refactored)
// Purpose: Defines the core interface and base class for all states.

using UnityEngine;

namespace Platformer.Core
{
    public abstract class State : IState
    {
        protected readonly PlayerController player;
        protected readonly StateMachine stateMachine;

        protected State(PlayerController player, StateMachine stateMachine)
        {
            this.player = player;
            this.stateMachine = stateMachine;
        }

        public virtual void OnEnter() { }
        
        // The Update method now accepts the moveInput vector for robust data flow.
        public virtual void Update(Vector2 moveInput) { }

        public virtual void FixedUpdate() { }

        public virtual void OnExit() { }
    }

    public interface IState
    {
        void OnEnter();
        void Update(Vector2 moveInput);
        void FixedUpdate();
        void OnExit();
    }
}