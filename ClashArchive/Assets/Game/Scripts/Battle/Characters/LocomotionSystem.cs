using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LocomotionSystem 
{
    private NavMeshAgent _agent;
    private Animator _animator;
    private Vector3? _currentDestination;

    private Action _callback;

    public LocomotionSystem(NavMeshAgent navMeshAgent, Animator animator)
    {
        _agent = navMeshAgent;
        _animator = animator;
    }

    public void MoveToPosition(Vector3 position, float speed, Action callback = null)
    {
        _agent.isStopped = false;
        _agent.speed = speed;
        _currentDestination = position;
        _agent.SetDestination(_currentDestination.Value);
        _animator.SetBool("isMoving", true);
        _callback = callback;
    }

    public void Tick()
    {
        if (!_currentDestination.HasValue)
            return;

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance &&
            (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f))
        {
            _currentDestination = null;
            _agent.isStopped = true;
            _animator.SetBool("isMoving", false);
            _callback?.Invoke();
        }
    }
}
