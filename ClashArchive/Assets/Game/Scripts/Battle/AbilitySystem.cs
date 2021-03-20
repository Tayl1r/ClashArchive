using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem
{
    private Animator _animator;
    private AbilityTemplate _currentTemplate;
    private BattleEntity _battleEntity;
    
    private float _time;
    private bool _hasExecuted;

    public AbilitySystem(BattleEntity battleEntity)
    {
        _battleEntity = battleEntity;
        _animator = _battleEntity.Animator;
    }

    public bool TriggerNewAbility(AbilityTemplate abilityTemplate)
    {
        if (abilityTemplate == null)
            return false;

        if (_currentTemplate != null)
        {
            if (!_currentTemplate.isCancellable)
                return false;
            // TODO: Any cancel clean up?
        }

        _currentTemplate = abilityTemplate;
        _time = 0;
        _hasExecuted = false;

        foreach (var action in _currentTemplate.StartUpActions)
            TriggerAbilityAction(action);

        return true;
    }

    private void TriggerAbilityAction(AbilityActionTemplate abilityActionTemplate)
    {
        if (!string.IsNullOrEmpty(abilityActionTemplate.AnimationTrigger))
        {
            _animator?.SetTrigger(abilityActionTemplate.AnimationTrigger);
//            Debug.Log(abilityActionTemplate.AnimationTrigger);
        }

        foreach(var template in abilityActionTemplate.DamageTemplates)
        {
            if (_battleEntity.CurrentCombatTarget != null)
            {
                int damage = (int)(_battleEntity.CharacterTemplate.CharacterStats.Attack * template.DamageModifier);
                _battleEntity.CurrentCombatTarget.TakeDamage(damage);
            }
        }
    }

    public void Tick()
    {
        if (!IsRunning())
            return;

        _time += Time.deltaTime;
        if (_time >= _currentTemplate.StartTime && !_hasExecuted)
        {
            foreach (var action in _currentTemplate.ActiveActions)
                TriggerAbilityAction(action);
            _hasExecuted = true;
        }

        if (_time >= _currentTemplate.Duration)
        {
            foreach (var action in _currentTemplate.FinishActions)
                TriggerAbilityAction(action);
            _currentTemplate = null;
        }
    }

    public bool IsRunning()
    {
        return _currentTemplate != null;
    }
}
