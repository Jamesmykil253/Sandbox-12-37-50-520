// File: Assets/_Project/Scripts/Core/StateMachine.cs
// Version: 2.0 (Fully Refactored)

using UnityEngine;
using System.Collections.Generic;
using System;
using Platformer.Core;

namespace Platformer
{
    public class StateMachine : MonoBehaviour
    {
        public IState CurrentState { get; private set; }

        public void Tick(Vector2 moveInput)
        {
            CurrentState?.Update(moveInput);
        }

        public void FixedTick()
        {
            CurrentState?.FixedUpdate();
        }

        public void SetState(IState state)
        {
            CurrentState?.OnExit();
            CurrentState = state;
            CurrentState?.OnEnter();
        }
    }
}