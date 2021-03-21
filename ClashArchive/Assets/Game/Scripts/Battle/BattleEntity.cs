using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public enum BattleEntityTeam { None, Player, Enemy }

public class BattleEntity : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private NavMeshAgent _navMeshAgent = default;
    public NavMeshAgent NavMeshAgent { get { return _navMeshAgent; } }
    [SerializeField] private Animator _animator = default;
    public Animator Animator { get { return _animator; } }

    // Stuff
    private AbilitySystem _abilitySystem;
    public Action<BattleEntity> OnDefeat;

    // 
    public BattleEntityTeam Team { private set; get; } = BattleEntityTeam.None;
    public CharacterTemplate CharacterTemplate { private set; get; }
    public BaseStats CharacterStats { private set; get; }
    public BattleEntity CurrentCombatTarget { private set; get; }
    public Vector3 CurrentMoveTarget { private set; get; }
    public CoverPosition CurrentCover { private set; get; }

    public static List<CoverPosition> OccupiedCover;
    private bool isActive  = false;
    private int _health;

    private void Start()
    {
        if (OccupiedCover == null)
            OccupiedCover = new List<CoverPosition>();

        _abilitySystem = new AbilitySystem(this);
        // Temp. until this is spawned in by code
        BattleModel.Instance.ActiveBattleEntities.AddMember(this);
    }

    public void Populate(CharacterTemplate characterTemplate, BattleEntityTeam team, bool activeImmediately)
    {
        Team = team;
        CharacterTemplate = characterTemplate;
        CharacterStats = characterTemplate.CharacterStats;
        _health = characterTemplate.CharacterStats.Health;
        if (Team == BattleEntityTeam.Enemy)
            _health = _health / 4;
        isActive = activeImmediately;
    }

    private void OnDestroy()
    {
        OnDefeat?.Invoke(this);
        InvalidateCurrentCover();
        BattleModel.Instance.ActiveBattleEntities.RemoveMember(this);
    }
    private void Update()
    {
        if (isActive)
        {
            if (_abilitySystem.IsRunning())
            {
                _abilitySystem.Tick();
                return;
            }

            if (CurrentCombatTarget != null && _navMeshAgent.remainingDistance < 0.1f)
            {
                _navMeshAgent.isStopped = true;
                _abilitySystem.TriggerNewAbility(CharacterTemplate.AutoAttackAbility);
            }

            if (CurrentCombatTarget == null)
                GetTarget();

            if (CurrentCombatTarget != null)
            {
                GetPosition();
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(CurrentMoveTarget);
            }
        }
        else
        {
            CheckAlert();
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

    private void GetPosition()
    {
        if (CurrentCombatTarget == null)
            throw new Exception("Shouldn't be getting a position with no target set");

        CoverPosition newCover = GetCoverPosition();
        if (newCover != CurrentCover)
            InvalidateCurrentCover();

        CurrentCover = newCover;
        if (CurrentCover != null)
        {
            OccupiedCover.Add(CurrentCover);
            CurrentMoveTarget = CurrentCover.GetPosition();
        }
        else
        {
            CurrentMoveTarget = GetOutOfCoverPosition();
        }
    }

    #region Cover
    private CoverPosition GetCoverPosition()
    {
        if (CurrentCover != null && IsCoverValid(CurrentCover, false))
            return CurrentCover;

        CoverPosition closestCover = null;
        float closestRecord = float.MaxValue;
        foreach (var cover in BattleModel.Instance.ActiveCoverEntities.Data)
        {
            if (IsCoverValid(cover, true))
            {
                float distanceToSelf = Vector3.Distance(transform.position, cover.GetPosition());
                if (distanceToSelf < closestRecord)
                {
                    closestRecord = distanceToSelf;
                    closestCover = cover;
                }
            }
        }
        return closestCover;
    }

    private bool IsCoverValid(CoverPosition cover, bool CheckOccupied)
    {
        // Is someone else using this?
        if (CheckOccupied && OccupiedCover.Contains(cover))
            return false;

        // Is this too far away?
        Vector3 coverPosition = cover.GetPosition();
        float distanceToSelf = Vector3.Distance(transform.position, cover.GetPosition());
        if (distanceToSelf >= 15f)
            return false;

        // Can I reach the enemy from here?
        Vector3 enemyPosition = CurrentCombatTarget.transform.position;
        float distanceToEnemy = Vector3.Distance(coverPosition, enemyPosition);
        if (distanceToEnemy > CharacterStats.MaximumAttackRange)
            return false;

        // Does this face the enemy?
        Vector3 direction = (enemyPosition - coverPosition).normalized;
        if (Vector3.Dot(direction, cover.transform.forward) < 0.75)
            return false;

        // Is an enemy too close to this cover?
        bool isEnemyTooClose = false;
        foreach (var battleEntity in BattleModel.Instance.ActiveBattleEntities.Data)
        {
            if (battleEntity.Team == Team)
                continue;

            if (Vector3.Distance(coverPosition, battleEntity.transform.position) < 5f)
            {
                isEnemyTooClose = true;
                break;
            }

            if (battleEntity.CurrentCover != null && Vector3.Distance(coverPosition, battleEntity.CurrentCover.GetPosition()) < 5f)
            {
                isEnemyTooClose = true;
                break;
            }
        }
        if (isEnemyTooClose)
            return false;

        return true;
    }

    private void InvalidateCurrentCover()
    {
        if (CurrentCover != null && OccupiedCover.Contains(CurrentCover))
            OccupiedCover.Remove(CurrentCover);
    }

    private Vector3 GetOutOfCoverPosition()
    {
        Vector3 enemyPosition = CurrentCombatTarget.transform.position;
        if (Vector3.Distance(transform.position, enemyPosition) <= CharacterStats.MaximumAttackRange)
            return transform.position;

        Vector3 direction = (transform.position - enemyPosition).normalized;        
        return enemyPosition + (direction * CharacterStats.IdealAttackRange);
    }
#endregion

    private void CheckAlert()
    {
        foreach (var battleEntity in BattleModel.Instance.ActiveBattleEntities.Data)
        {
            if (battleEntity.Team == Team)
                continue;

            if (Vector3.Distance(transform.position, battleEntity.transform.position) < 10f)
            {
                SoundAlert();
                return;
            }
        }
    }

    private void SoundAlert()
    {
        foreach (var battleEntity in BattleModel.Instance.ActiveBattleEntities.Data)
        {
            if (battleEntity.Team == Team && battleEntity != this)
                battleEntity.OnDetectEnemy();
        }
    }

    public void OnDetectEnemy()
    {
        isActive = true;
    }


    public void TakeDamage(int damage)
    {
        if (!isActive)
            SoundAlert();

        _health -= damage;
        if (_health <= 0)
            Destroy(gameObject);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_navMeshAgent.hasPath)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up, CurrentMoveTarget);
        }

        if (CurrentCombatTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, CurrentCombatTarget.transform.position);
        }

        // if (Application.isPlaying && _stateMachine != null)
         //   Handles.Label(transform.position, _stateMachine.GetStateString());

        Handles.Label(transform.position, _health.ToString());
    }
#endif
}