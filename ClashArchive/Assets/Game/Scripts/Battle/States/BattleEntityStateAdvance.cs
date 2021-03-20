using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattleEntityStateAdvance : IState
{
    private NavMeshAgent _navMeshAgent;
    private BattleEntity _entity;
    private Vector3 _formation;

    public BattleEntityStateAdvance(NavMeshAgent navMeshAgent, BattleEntity entity, Vector3 startingFormation)
    {
        _navMeshAgent = navMeshAgent;
        _entity = entity;
        _formation = startingFormation;
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
    }

    public void Tick()
    {
        /*Vector3 offset = _entity.Team == BattleEntityTeam.Player || BattleField.Instance.IsLeftOfBounds(_entity.transform.position) ? Vector3.right * 3 : Vector3.left * 3;
        Vector3 position = _entity.transform.position + offset;
        position.z = _formation.z;

        if (BattleField.Instance.IsPointInBounds(position) || BattleField.Instance.IsLeftOfBounds(position))
        {
            _entity.Animator.SetBool("walkFlag", true);
            _navMeshAgent.isStopped = false;
            _navMeshAgent.SetDestination(position);
        }
        else
        {
            _entity.Animator.SetBool("walkFlag", false);
            _navMeshAgent.isStopped = true;
        }*/
    }

    public void OnExit()
    {
        _entity.Animator.SetBool("walkFlag", false);
        _navMeshAgent.isStopped = true;
        _navMeshAgent.enabled = false;
    }
}
