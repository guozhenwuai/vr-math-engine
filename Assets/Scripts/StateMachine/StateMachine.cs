using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private Dictionary<uint, IState> mDictionaryState;

    private IState mCurrentState;

    public StateMachine()
    {
        mDictionaryState = new Dictionary<uint, IState>();
        mCurrentState = null;
    }

    public bool RegisterState(IState state)
    {
        if (state == null)
        {
            return false;
        }
        if (mDictionaryState.ContainsKey(state.GetStateID()))
        {
            return false;
        }
        mDictionaryState.Add(state.GetStateID(), state);
        return true;
    }

    public IState GetState(uint stateID)
    {
        IState state = null;
        mDictionaryState.TryGetValue(stateID, out state);
        return state;
    }

    public void StopState()
    {
        if (mCurrentState == null)
         {
            return;
        }
        mCurrentState.OnLeave(null);
        mCurrentState = null;
    }

    public bool UnRegisterState(uint StateID)
    {
        if (!mDictionaryState.ContainsKey(StateID))
        {
            return false;
        }
        if (mCurrentState != null && mCurrentState.GetStateID() == StateID)
        {
            return false;
        }
        mDictionaryState.Remove(StateID);
        return true;
    }

    public delegate void BetweenSwitchState(IState from, IState to);

    public BetweenSwitchState BetweenSwitchStateCallBack = null;

    public bool SwitchState(uint newStateID)
    {
        if (mCurrentState != null && mCurrentState.GetStateID() == newStateID)
        {
            return false;
        }
        IState newState = null;
        mDictionaryState.TryGetValue(newStateID, out newState);
        if (newState == null)
        {
            return false;
        }
        if (mCurrentState != null)
        {
            mCurrentState.OnLeave(newState);
        }
        
        IState oldState = mCurrentState;
        mCurrentState = newState;
        if (BetweenSwitchStateCallBack != null)
        {
            BetweenSwitchStateCallBack(oldState, mCurrentState);
        }
        
        newState.OnEnter(this, oldState);
        return true;
    }

    public bool IsState(uint stateID)
    {
        return mCurrentState == null ? false : mCurrentState.GetStateID() == stateID;
    }

    public void OnUpdate()
    {
        if (mCurrentState != null)
        {
            mCurrentState.OnUpdate();
        }
    }
}