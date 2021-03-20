using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattleEntityStateAutoAttack : IState
{
    private NavMeshAgent _navMeshAgent;
    private BattleEntity _entity;

    public BattleEntityStateAutoAttack(NavMeshAgent navMeshAgent, BattleEntity entity)
    {
        _navMeshAgent = navMeshAgent;
        _entity = entity;
    }

    public void Tick()
    {
        if (_entity.CurrentCombatTarget != null)
        {
            var distance = Vector3.Distance(_entity.transform.position, _entity.CurrentCombatTarget.transform.position);
            if (distance > _entity.CharacterStats.MaximumAttackRange)
            {
                _entity.Animator.SetBool("walkFlag", true);
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(_entity.CurrentCombatTarget.transform.position);
            }
            else
            {
                _entity.Animator.SetBool("walkFlag", false);
                _navMeshAgent.isStopped = true;
            }
        }

       /* if (!_entity._abilitySystem.IsRunning())
        {
            _entity._abilitySystem.TriggerNewAbility(_entity.characterTemplate.AutoAttackAbility);
        }*/
    }

    public void OnEnter()
    {
        _navMeshAgent.enabled = true;
    }

    public void OnExit()
    {
        _entity.Animator.SetBool("walkFlag", false);
        _navMeshAgent.isStopped = true;
        _navMeshAgent.enabled = false;
    }
}
