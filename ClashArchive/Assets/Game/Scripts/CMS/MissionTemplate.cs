using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSpawnTemplate
{
    public CharacterTemplate CharacterTemplate;
    public int LevelOffset;
}

[System.Serializable]
public class MissionStage
{
    public int CharacterBaseLevel;
    public List<CharacterSpawnTemplate> MissionSpawns;
}

[CreateAssetMenu(fileName ="Mission Template", menuName = "ClashArchive/Mission Template", order = 1000)]
public class MissionTemplate : ScriptableObject
{

    public List<MissionStage> stages;   
}
