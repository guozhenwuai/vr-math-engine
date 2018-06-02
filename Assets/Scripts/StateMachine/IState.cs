using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    uint GetStateID();

    void OnEnter(StateMachine machine, IState state, object param);
    void OnLeave(IState nextState);

    void OnUpdate();
}