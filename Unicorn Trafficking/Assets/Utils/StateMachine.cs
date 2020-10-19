using System;
using System.Collections.Generic;
using UnityEngine;

// Adapted from this tutorial (Credit: Jason Weimann)
// https://www.youtube.com/watch?v=V75hgcsCGOM
public class StateMachine
{
    class Transition
    {
        public IState To { get; }
        public Func<bool> Condition { get; }
        public Transition(IState to, Func<bool> condition)
        {
            To = to;
            Condition = condition;
        }
    }
    
    public interface IState
    {
        void UpdateTick();
        void FixedTick();
        void OnEnter();
        void OnExit();
        #if UNITY_EDITOR
        void DrawGizmos();
        #endif
    }

    public IState _curState { get; private set; }
    
    /*  Classes that implement IState are the "Type" in this dictionary.
        The List contains all transitions *out of* that state. */
    private Dictionary<Type, List<Transition>> transitionDictionary = new Dictionary<Type, List<Transition>>();
    
    // All possible transitions out of curState are stored in this dictionary.
    private List<Transition> curStateTransitions = new List<Transition>();
    // universalTransitions are transitions that can occur at any time, regardless of our current state.
    private List<Transition> universalTransitions = new List<Transition>();
    
    /*  We return this if there are no transitions for the current state. This happens when a state has
        no specific transitions of it's own, and is affected only by universalTransitions. */
    private static List<Transition> noTransitionsAvailable = new List<Transition>(0);
    
    public void UpdateTick()
    {
        _curState?.UpdateTick();
    }
    public void FixedTick()
    {
        if (ShouldTransition(out var newTransition))
            SetState(newTransition.To);
        
        _curState?.FixedTick();
    }

    private bool ShouldTransition(out Transition acceptedTransition)
    {
        // Evaluate the universal transitions to see if we should change
        foreach (var transition in universalTransitions)
        {
            if (transition.Condition())
            {
                acceptedTransition = transition;
                return true;
            }
        }

        // Evaluate the current state transitions to see if we should change
        foreach (var transition in curStateTransitions)
        {
            if (transition.Condition())
            {
                acceptedTransition = transition;
                return true;
            }
        }

        acceptedTransition = null;
        return false;
    }

    public void AddTransition(IState from, IState to, Func<bool> condition)
    {
        if (transitionDictionary.TryGetValue(from.GetType(), out var transitions) == false)
        {
            transitions = new List<Transition>();
            transitionDictionary[from.GetType()] = transitions;
        }
        
        transitions.Add(new Transition(to, condition));
    }
    
    public void AddUniversalTransition(IState to, Func<bool> condition)
    {
        universalTransitions.Add(new Transition(to, condition));
    }

    public void SetState(IState newState)
    {
        if (_curState == newState) return;
        
        // Call OnExit on current state to clean up anything we need to.
        _curState?.OnExit();
        _curState = newState;

        // Update curStateTransitions for our new state
        if (!transitionDictionary.TryGetValue(_curState.GetType(), out curStateTransitions))
            curStateTransitions = noTransitionsAvailable;
        
        // Enter the new state.
        _curState.OnEnter();
    }

    #if UNITY_EDITOR
    public void DrawGizmos()
    {
        _curState?.DrawGizmos();
    }
    #endif
}
