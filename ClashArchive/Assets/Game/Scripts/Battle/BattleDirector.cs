using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDirector : MonoBehaviour
{
    public static BattleDirector Instance;
    [SerializeField] private BattleField _battleField;

    [Header("Debug")]
    [SerializeField] private List<CharacterSpawnTemplate> playerCharacters = default;
    [SerializeField] private MissionTemplate _missionTemplate = default;

    private int _currentStage = 0;
    private int _remainingStageEnemies = 0;
    private int _remainingTotalEnemies = 0;

    void Start()
    {
        if (_battleField == null)
            _battleField = BattleField.Instance;
        Instance = this;

        List<BattleEntity> characters = SpawnCharacters(playerCharacters, _battleField.PlayerSpawnPoints, BattleEntityTeam.Player);
        foreach(var character in characters)
            _battleField.targetGroup.AddMember(character.transform, 1f, 1f);

        SetupStage(_currentStage);
        _remainingTotalEnemies = GetTotalEnemies(_missionTemplate);
    }

#if UNITY_EDITOR
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _currentStage++;
            SetupStage(_currentStage);
        }
    }
#endif

    private void SetupStage(int stage)
    {
        if (stage >= _missionTemplate.stages.Count)
            throw new System.Exception($"Battle Field does not support stage number {stage}.");

        var spawnPoints = _battleField.GetTheatreSpawnPoints(_currentStage);
        SpawnCharacters(_missionTemplate.stages[stage].MissionSpawns, spawnPoints, BattleEntityTeam.Enemy);

        Debug.Log($"Stage {stage} preperation: Places, everyone! Places!");
        foreach (var entity in BattleModel.Instance.ActiveBattleEntities.Data)
        {
        }
    }

    public List<BattleEntity> SpawnCharacters(List<CharacterSpawnTemplate> characterSpawnTemplates, List<SpawnPoint> spawnPoints, BattleEntityTeam team)
    {
        if (spawnPoints == null)
            throw new System.Exception($"No spawn points provided");

        Dictionary<CharacterRow, List<SpawnPoint>> rowSpawns = new Dictionary<CharacterRow, List<SpawnPoint>>();
        List<SpawnPoint> allSpawnPoints = new List<SpawnPoint>();
        List<BattleEntity> results = new List<BattleEntity>();

        foreach (var spawn in spawnPoints)
        {
            allSpawnPoints.Add(spawn);
            if (!rowSpawns.ContainsKey(spawn.CharacterRow))
                rowSpawns[spawn.CharacterRow] = new List<SpawnPoint>();
            rowSpawns[spawn.CharacterRow].Add(spawn);
        }

        foreach (var enemy in characterSpawnTemplates)
        {
            SpawnPoint spawnPoint = null;
            CharacterTemplate characterTemplate = enemy.CharacterTemplate;
            if (rowSpawns.ContainsKey(characterTemplate.CharacterRow) && rowSpawns[characterTemplate.CharacterRow].Count > 0)
            {
                spawnPoint = RandomUtils.RandomFrom(rowSpawns[characterTemplate.CharacterRow]);
                rowSpawns[characterTemplate.CharacterRow].Remove(spawnPoint);
                allSpawnPoints.Remove(spawnPoint);
            }
            else if (allSpawnPoints.Count > 0)
            {
                spawnPoint = RandomUtils.RandomFrom(allSpawnPoints);
                Debug.LogWarning($"There are not enought \"{characterTemplate.CharacterRow}\" spawn points for this wave");
            }
            else
            {
                throw new System.Exception("There are not enough total spawn points for this wave");
            }

            BattleEntity newEntity = Instantiate(characterTemplate.Prefab, spawnPoint.transform.position, spawnPoint.transform.rotation, transform);
            results.Add(newEntity);
            newEntity.Populate(characterTemplate, team, team == BattleEntityTeam.Player);
            newEntity.OnDefeat += EnemyDefeated;
            _remainingStageEnemies++;
        }

        return results;
    }

    private int GetTotalEnemies(MissionTemplate missionTemplate)
    {
        int result = 0;
        foreach (var wave in missionTemplate.stages)
            result += wave.MissionSpawns.Count;
        return result;
    }

    private void EnemyDefeated(BattleEntity entity)
    {
        entity.OnDefeat -= EnemyDefeated;
        _remainingStageEnemies--;
        _remainingTotalEnemies--;
        Debug.Log(_remainingTotalEnemies);

        if (_remainingStageEnemies <= 0)
        {
            _currentStage++;
            if (_currentStage >= _missionTemplate.stages.Count)
            {

            }
            else
            {
                SetupStage(_currentStage);
            }
        }
    }
}
