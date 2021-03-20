using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="Ability Template", menuName = "ClashArchive/Ability Template", order = 1000)]
public class AbilityTemplate : ScriptableObject
{
    public float StartTime = 0.5f;
    public float Duration = 1f;
    public bool isCancellable = false;
    public List<AbilityActionTemplate> StartUpActions;
    public List<AbilityActionTemplate> ActiveActions;
    public List<AbilityActionTemplate> FinishActions;
}

[System.Serializable]
public class AbilityActionTemplate
{
    //TODO: Ideally this would be multiple types but whatever it was good enough for dragon.
    public string AnimationTrigger;
    public List<AbilityActionDamageTemplate> DamageTemplates;
}

[System.Serializable]
public class AbilityActionDamageTemplate
{
    public enum AbilityActionDamageTarget { Current, Forwards, AdvanceDirection }
    public enum AbilityActionDamageShape { Absolute, Hitscan, Cone, Circle }
    public enum AbilityActionDamageRange { Infinite, AttackRange, Specified }

    public AbilityActionDamageTarget Target;
    public AbilityActionDamageShape Shape;
    public AbilityActionDamageRange Range;
    public float DamageModifier = 1;
    [Tooltip("Cone and Circle only")] public float Radius;
    [Tooltip("Cone only")] public float ConeWidth;
}