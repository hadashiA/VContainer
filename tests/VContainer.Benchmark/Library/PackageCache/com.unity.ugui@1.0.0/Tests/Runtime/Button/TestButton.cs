using System.Collections.Generic;
using UnityEngine.UI;

class TestButton : Button
{
    public bool isStateNormal { get { return currentSelectionState == SelectionState.Normal; } }
    public bool isStateHighlighted { get { return currentSelectionState == SelectionState.Highlighted; } }
    public bool isStatePressed { get { return currentSelectionState == SelectionState.Pressed; } }
    public bool isStateDisabled { get { return currentSelectionState == SelectionState.Disabled; } }
    public bool isStateSelected { get { return currentSelectionState == SelectionState.Selected; } }

    private bool IsTransitionTo(int index, SelectionState selectionState)
    {
        return index < m_StateTransitions.Count && m_StateTransitions[index] == selectionState;
    }

    public bool IsTransitionToNormal(int index) { return IsTransitionTo(index, SelectionState.Normal); }
    public bool IsTransitionToHighlighted(int index) { return IsTransitionTo(index, SelectionState.Highlighted); }
    public bool IsTransitionToPressed(int index) { return IsTransitionTo(index, SelectionState.Pressed); }
    public bool IsTransitionToDisabled(int index) { return IsTransitionTo(index, SelectionState.Disabled); }

    private readonly List<SelectionState> m_StateTransitions = new List<SelectionState>();

    public int StateTransitionCount { get { return m_StateTransitions.Count; } }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        m_StateTransitions.Add(state);
        base.DoStateTransition(state, instant);
    }
}
