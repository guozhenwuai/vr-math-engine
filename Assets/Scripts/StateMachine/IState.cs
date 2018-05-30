using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    uint GetStateID();

    void OnEnter(StateMachine machine, IState preState);
    void OnLeave(IState nextState);

    void OnUpdate();
}