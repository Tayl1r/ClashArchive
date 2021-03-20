using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattleEntityStateRunToCover : IState
{
    private NavMeshAgent _navMeshAgent;
    private BattleEntity _entity;

    public BattleEntityStateRunToCover(NavMeshAgent navMeshAgent, BattleEntity entity)
    {
        _navMeshAgent = navMeshAgent;
        _entity = entity;
    }

    public void Tick()
    {
    }

    public void OnEnter()
    {
      /*  if (_entity.DesiredCover != null)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.SetDestination(_entity.DesiredCover.GetPosition());
            _entity.Animator.SetBool("walkFlag", true);
        } */
    }

    public void OnExit()
    {
        //   _navMeshAgent.enabled = false;
        _entity.Animator.SetBool("walkFlag", false);
    }
}
