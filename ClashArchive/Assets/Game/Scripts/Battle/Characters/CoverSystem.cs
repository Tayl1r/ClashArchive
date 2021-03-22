using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CoverSystem
{
    private BattleEntity _owner;
    public CoverLocation IdealCover { private set; get; }

    private static List<CoverLocation> _occupiedCover;
    private static float CoverSearchRange = 5f;
    public Action<CoverLocation> OnFindNewIdealCover;
    public Action OnCoverInvalid;

    public CoverSystem(BattleEntity owner)
    {
        _owner = owner;
        if (_occupiedCover == null)
            _occupiedCover = new List<CoverLocation>();
    }

    public void Tick()
    {
        if (!_owner.CharacterTemplate.CharacterStats.UseCover)
            return;

        // If we're snug and can shoot the opponent, return
        if (IsInCover() && _owner.IsTargetInRange())
            return;

        // If we're heading to some good cover, return
        if (IdealCover != null)
        {
            if (IsValidCover(IdealCover))
                return;
        }

        // Otherwise, look around
        InvalidateCurrentCover();
        if (_owner.CurrentCombatTarget != null)
        {
            IdealCover = SearchForCover();
            if (IdealCover != null)
            {
                _occupiedCover.Add(IdealCover);
                OnFindNewIdealCover?.Invoke(IdealCover);
            }
        }
    }

    public void OnDestroy()
    {
        InvalidateCurrentCover();
    }

    public bool IsInCover()
    {
        if (IdealCover == null)
            return false;

        if (Vector3.Distance(_owner.transform.position, IdealCover.GetPosition()) > 0.5f)
            return false;

        return true;
    }

    private void InvalidateCurrentCover()
    {
        if (IdealCover != null && _occupiedCover.Contains(IdealCover))
        {
            _occupiedCover.Remove(IdealCover);
            OnCoverInvalid?.Invoke();
        }
         IdealCover = null;
    }

    private CoverLocation SearchForCover()
    {
        float closestCover = float.MaxValue;
        CoverLocation bestCover = null;

        foreach(var coverLocation in BattleModel.Instance.ActiveCoverEntities.Data)
        {
            float distance = Vector3.Distance(_owner.transform.position, coverLocation.GetPosition());
            if (distance < closestCover && distance < CoverSearchRange)
            {
                if (IsValidCover(coverLocation))
                {
                    closestCover = distance;
                    bestCover = coverLocation;
                }
            }
        }
        return bestCover;
    }

    private bool IsValidCover(CoverLocation coverLocation)
    {
        if (coverLocation == null)
            return false;

        Vector3 coverPos = coverLocation.GetPosition();
        Vector3 selfPos = _owner.transform.position;
        Vector3 enemyPos = _owner.CurrentCombatTarget.transform.position;

        // Is this occupied?
        if (_occupiedCover.Contains(coverLocation))
            return false;

        // Is this close enough for me to enter?
        if (!MathUtils.PointInCircle(coverPos, CoverSearchRange, selfPos))
            return false;

        // Can I actually attack from here?
        if (!MathUtils.PointInCircle(coverLocation.GetPosition(), _owner.CharacterStats.MaximumAttackRange, _owner.CurrentCombatTarget.transform.position))
            return false;

        // Does this face the enemy?
        Vector3 direction = (enemyPos - coverPos).normalized;
        if (Vector3.Dot(direction, coverLocation.transform.forward) < 0.75)
            return false;

        // Is this on the Nav Mesh?
        if (!NavMesh.SamplePosition(coverPos, out NavMeshHit hit, 0.1f, NavMesh.AllAreas))
            return false;

        Debug.DrawLine(selfPos, coverPos, Color.magenta);

        return true;
    }
}
