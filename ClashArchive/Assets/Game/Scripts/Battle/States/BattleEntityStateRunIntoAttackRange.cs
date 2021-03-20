using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattleEntityStateRunIntoAttackRange : IState
{
    private NavMeshAgent _navMeshAgent;
    private BattleEntity _entity;

    public BattleEntityStateRunIntoAttackRange(NavMeshAgent navMeshAgent, BattleEntity entity)
    {
        _navMeshAgent = navMeshAgent;
        _entity = entity;
    }

    public void Tick()
    {

    }

    public void OnEnter()
    {
        var direction = (_entity.CurrentCombatTarget.transform.position - _entity.transform.position).normalized;
        var position = _entity.CurrentCombatTarget.transform.position + direction * 5f;

        /*if (_entity.DesiredCover != null)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.SetDestination(position);
            _entity.Animator.SetBool("walkFlag", true);
        } */
    }

    public void OnExit()
    {
        //    _navMeshAgent.enabled = false;
        _entity.Animator.SetBool("walkFlag", false);
    }
}
