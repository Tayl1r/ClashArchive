using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterRow
{
    Front, Middle, Back, Boss
}

[System.Serializable]
public class BaseStats
{
    [Header("Scaling Stats")]
    public int Health = 742;
    public int Attack = 157;
    public int Defence = 130;

    [Header("Combat")]
    public float MoveSpeed = 5f;
    public float CriticalRate = 0.05f;
    public float ReloadTime = 1.05f;

    [Header("Derived From Weapon Type?!")]
    public int MaxAmmo = 10;
    public bool UseCover = true;
    public float CoverBlockChance = 0.5f;
    [Tooltip("Cannot attack if the opponent is further than this")] public float MaximumAttackRange = 10f;
    [Tooltip("When attacking without cover, try to stand at least this close")] public float IdealAttackRange = 8f;
    public float AttackInterval = 0.5f;
}

[CreateAssetMenu(fileName = "Character Template", menuName = "ClashArchive/Character Template", order = 1000)]
public class CharacterTemplate : ScriptableObject
{
    [SerializeField] private string _characterName = default;
    public string CharacterName { get { return _characterName; } }

    [SerializeField] private BattleEntity _prefab = default;
    public BattleEntity Prefab { get { return _prefab; } }

    [SerializeField] private CharacterRow _characterRow = default;
    public CharacterRow CharacterRow { get { return _characterRow; } }

    [Header("Statistics")]
    [SerializeField] private BaseStats _characterStats = default;
    public BaseStats CharacterStats { get { return _characterStats; } }

    [Header("Ailities")]
    [SerializeField] private AbilityTemplate _autoAttackAbility = default;
    public AbilityTemplate AutoAttackAbility { get { return _autoAttackAbility; } }
}
