using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum BattleEntityTeam { None, Player, Enemy }

public class BattleEntity : MonoBehaviour
{
    private enum BattleEntityState
    {
        Waiting, Prep, Moving, Attacking
    }


    [Header("Setup")]
    [SerializeField] private NavMeshAgent _navMeshAgent = default;
    public NavMeshAgent NavMeshAgent { get { return _navMeshAgent; } }
    [SerializeField] private Animator _animator = default;
    public Animator Animator { get { return _animator; } }

    // Internal
    private AbilitySystem _abilitySystem;
    private CoverSystem _coverSystem;
    public Action<BattleEntity> OnDefeat;
    private BattleEntityState _currentState;

    public BattleEntityTeam Team { private set; get; } = BattleEntityTeam.None;
    public CharacterTemplate CharacterTemplate { private set; get; }
    public BaseStats CharacterStats { private set; get; }
    public BattleEntity CurrentCombatTarget { private set; get; }
    public Vector3 CurrentMoveTarget { private set; get; }

    private bool isActive  = false;
    private int _health;

    private void Start()
    {
        _abilitySystem = new AbilitySystem(this);
        _coverSystem = new CoverSystem(this);
    }

    public void Populate(CharacterTemplate characterTemplate, BattleEntityTeam team, bool activeImmediately)
    {
        Team = team;
        CharacterTemplate = characterTemplate;
        CharacterStats = characterTemplate.CharacterStats;
        _health = characterTemplate.CharacterStats.Health;
        if (Team == BattleEntityTeam.Enemy)
            _health = _health / 4;
        _currentState = activeImmediately ? BattleEntityState.Moving : BattleEntityState.Waiting;

        BattleModel.Instance.ActiveBattleEntities.AddMember(this);
    }

    private void OnDestroy()
    {
        OnDefeat?.Invoke(this);
        _coverSystem.OnDestroy();
        BattleModel.Instance.ActiveBattleEntities.RemoveMember(this);
    }
    private void Update()
    {
        if (_currentState == BattleEntityState.Waiting)
            return;

        if (_currentState == BattleEntityState.Prep)
            return;

        if (CurrentCombatTarget == null)
        {
            GetTarget();
            if (CurrentCombatTarget == null)
            {
                _currentState = BattleEntityState.Waiting;
                return;
            }
        }

        if (_currentState == BattleEntityState.Attacking)
        {
            _abilitySystem.Tick();
            if (!_abilitySystem.IsRunning())
            {
                if (Vector3.Distance(transform.position, CurrentCombatTarget.transform.position) <= CharacterStats.MaximumAttackRange)
                    _abilitySystem.TriggerNewAbility(CharacterTemplate.AutoAttackAbility);
                else
                    _currentState = BattleEntityState.Moving;
            }
            else
            {
                return;
            }
        }

        if (_currentState == BattleEntityState.Moving)
        {
            if (Vector3.Distance(transform.position, CurrentMoveTarget) < 0.25f)
            {
                _navMeshAgent.isStopped = true;
                _abilitySystem.TriggerNewAbility(CharacterTemplate.AutoAttackAbility);
                _currentState = BattleEntityState.Attacking;
                return;
            }
            else
            {
                _coverSystem.Tick();
                Vector3 position = _coverSystem.IdealCover != null ? _coverSystem.IdealCover.GetPosition() : GetIntoAttackRange();
                CurrentMoveTarget = position;
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(CurrentMoveTarget);
            }
        }
    }
    

    private void GetTarget()
    {
        BattleEntity closestEntity = null;
        float closestRecord = float.MaxValue;

        foreach (var battleEntity in BattleModel.Instance.ActiveBattleEntities.Data)
        {
            if (battleEntity.Team != Team)
            {
                float distance = Vector3.Distance(transform.position, battleEntity.transform.position);
                if (distance < closestRecord)
                {
                    closestEntity = battleEntity;
                    closestRecord = distance;
                }
            }
        }
        CurrentCombatTarget = closestEntity;
    }

    public bool IsTargetInRange()
    {
        if (CurrentCombatTarget == null)
            return false;

        if (Vector3.Distance(transform.position, CurrentCombatTarget.transform.position) > CharacterStats.MaximumAttackRange)
            return false;

        return true;
    }

    private Vector3 GetPosition()
    {
        if (CurrentCombatTarget == null)
            throw new Exception("Shouldn't be getting a position with no target set");

        if (CharacterTemplate.CharacterStats.UseCover)
            _coverSystem.Tick();

        if (_coverSystem.IdealCover != null)
            return _coverSystem.IdealCover.GetPosition();
        else
            return GetIntoAttackRange();
    }

    private Vector3 GetIntoAttackRange()
    {
        Vector3 enemyPosition = CurrentCombatTarget.transform.position;
        if (Vector3.Distance(transform.position, enemyPosition) <= CharacterStats.MaximumAttackRange)
            return transform.position;

        Vector3 direction = (transform.position - enemyPosition).normalized;        
        return enemyPosition + (direction * CharacterStats.IdealAttackRange);
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
            Destroy(gameObject);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_abilitySystem.IsRunning() && CurrentCombatTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, CurrentCombatTarget.transform.position);
        }

        if (_coverSystem.IdealCover)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, _coverSystem.IdealCover.transform.position);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, CurrentMoveTarget);
        }
        string debug = "In Cover: " + _coverSystem.IsInCover().ToString();
        if (CurrentCombatTarget != null)
            debug += "\nTarget: " + CurrentCombatTarget.name;
        else
            debug += "\nTarget: null";
        debug += "\nMovePos: " + CurrentMoveTarget;
        debug += "\nNav Stopped: " + _navMeshAgent.isStopped.ToString();
        debug += "\nState: " + _currentState;
        debug += "\nHealth: " + _health;

        Handles.Label(transform.position, debug);
    }
#endif
}