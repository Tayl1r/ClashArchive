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

    public CoverSystem(BattleEntity owner)
    {
        _owner = owner;
        if (_occupiedCover == null)
            _occupiedCover = new List<CoverLocation>();
    }

    public void OnDestroy()
    {
        UnsetCover();
    }

    private void SetAsCover(CoverLocation coverLocation)
    {
        if (IdealCover != null)
            UnsetCover();

        IdealCover = coverLocation;
        _occupiedCover.Add(IdealCover);
    }

    private void UnsetCover()
    {
        if (IdealCover == null)
            return;
        _occupiedCover.Remove(IdealCover);
        IdealCover = null;
    }

    // This isn't the cheapest thing, but it's okay once at the start of a round
    public CoverLocation GetRandomUsefulCoverNearPoint(Vector3 point, bool setAsTarget)
    {
        if (!_owner.CharacterStats.UseCover)
            return null;

        var potentialResults = new List<CoverLocation>();
        foreach(var coverLocation in BattleModel.Instance.ActiveCoverEntities.Data)
        {
            foreach(var entity in BattleModel.Instance.ActiveBattleEntities.Data)
            {
                if (entity.Team != _owner.Team)
                    continue;

                if (IsValidCover(coverLocation, point, entity.transform.position, CoverSearchRange, _owner.CharacterStats.MaximumAttackRange))
                { 
                    potentialResults.Add(coverLocation);
                    break;
                }
            }
        }

        CoverLocation result = null;

        if (potentialResults.Count > 0)
            result = RandomUtils.RandomFrom(potentialResults);
        if (setAsTarget)
            SetAsCover(IdealCover);
        return result;
    }

    private List<CoverLocation> GetNearbyCover(Vector3 position, float searchRange)
    {
        var results = new List<CoverLocation>();
        foreach (var coverLocation in BattleModel.Instance.ActiveCoverEntities.Data)
        {
            if (MathUtils.PointInCircle(coverLocation.GetPosition(), searchRange, position))
                results.Add(coverLocation);
        }
        return results;
    }


    /*
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
    */
    private static bool IsValidCover(CoverLocation coverLocation, Vector3 characterPosition, Vector3 enemyPosition, float searchRange, float attackRange)
    {
        if (coverLocation == null)
            return false;

        Vector3 coverPosition = coverLocation.GetPosition();

        // Is this occupied?
        if (_occupiedCover.Contains(coverLocation))
            return false;

        // Is this close enough for me to enter?
        if (!MathUtils.PointInCircle(coverPosition, searchRange, characterPosition))
            return false;

        // Does this face the enemy?
        Vector3 direction = (enemyPosition - coverPosition).normalized;
        if (Vector3.Dot(direction, coverLocation.transform.forward) < 0.75)
            return false;

        // Can I actually attack from here?
        if (!MathUtils.PointInCircle(coverLocation.GetPosition(), attackRange, enemyPosition))
            return false;

        /*
        // Is this on the Nav Mesh?
        if (!NavMesh.SamplePosition(coverPos, out NavMeshHit hit, 0.1f, NavMesh.AllAreas))
            return false;*/

        Debug.DrawLine(characterPosition, coverPosition, Color.magenta, 0.1f);

        return true;
    }
}
