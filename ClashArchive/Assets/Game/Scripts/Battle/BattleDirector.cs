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
        SpawnPlayerCharacters(playerCharacters, _battleField.PlayerSpawnPoints);
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
            throw new System.Exception($"Battle Field is advancing to a stage number {stage} it cannot support");

        var spawnPoints = _battleField.GetTheatreSpawnPoints(_currentStage);
        if (spawnPoints != null)
            SpawnEnemies(_missionTemplate.stages[stage].MissionSpawns, spawnPoints);
        else
            throw new System.Exception($"Battle Field does not have spawn points for {stage}");

        Debug.Log($"Stage {stage} preperation: Places, everyone! Places!");
        foreach (var entity in BattleModel.Instance.ActiveBattleEntities.Data)
        {
        }
    }

    public List<BattleEntity> SpawnEnemies(List<CharacterSpawnTemplate> characterSpawnTemplates, List<SpawnPoint> spawnPoints)
    {
        if (spawnPoints == null)
            throw new System.Exception($"No spawn points provided by Battle Field");

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
            newEntity.Populate(characterTemplate, BattleEntityTeam.Enemy, false);
            newEntity.OnDefeat += EnemyDefeated;
            _remainingStageEnemies++;
        }

        return results;
    }

    private List<BattleEntity> SpawnPlayerCharacters(List<CharacterSpawnTemplate> characterSpawnTemplates, PlayerSpawnRows spawnRows)
    {
        List<BattleEntity> results = new List<BattleEntity>();

        // This is some Yandere Dev shit.
        int frontRowCount = 0;
        int middleRowCount = 0;
        int backRowCount = 0;

        foreach (var spawnTemplate in characterSpawnTemplates)
        {
            CharacterTemplate characterTemplate = spawnTemplate.CharacterTemplate;
            if (characterTemplate.CharacterRow == CharacterRow.Front)
                frontRowCount++;
            else if (characterTemplate.CharacterRow == CharacterRow.Middle)
                middleRowCount++;
            else
                backRowCount++;
        }

        int frontRowIndex = 0;
        int middleRowIndex = 0;
        int backRowIndex = 0;

        foreach (var spawnTemplate in characterSpawnTemplates)
        {
            CharacterTemplate characterTemplate = spawnTemplate.CharacterTemplate;
            Vector3 position = spawnRows.transform.position;
            BattleEntity newEntity = Instantiate(characterTemplate.Prefab, transform);
            newEntity.Populate(characterTemplate, BattleEntityTeam.Player, true);

            _battleField.targetGroup.AddMember(newEntity.transform, 1f, 1f);

            if (characterTemplate.CharacterRow == CharacterRow.Front)
            {
                position += spawnRows.frontRow.GetPoint(frontRowIndex, frontRowCount);
                frontRowIndex++;
            }
            else if (characterTemplate.CharacterRow == CharacterRow.Middle)
            {
                position += spawnRows.frontRow.GetPoint(middleRowIndex, middleRowCount);
                middleRowIndex++;
            }
            else
            {
                position += spawnRows.frontRow.GetPoint(backRowIndex, backRowCount);
                backRowIndex++;
            }
            newEntity.transform.position = position;
            results.Add(newEntity);
            Debug.DrawLine(spawnRows.transform.position, position, Color.magenta, 60f);
                
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
