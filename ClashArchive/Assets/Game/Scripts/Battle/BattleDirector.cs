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
    private int _preparingCharacters = 0;

    void Start()
    {
        if (_battleField == null)
            _battleField = BattleField.Instance;
        Instance = this;

        var characters = SpawnCharacters(playerCharacters, _battleField.PlayerSpawnPoints, BattleEntityTeam.Player, false);
        foreach(var character in characters)
            _battleField.targetGroup.AddMember(character.transform, 1f, 1f);

        SetupStage(_currentStage);
        _remainingTotalEnemies = GetTotalEnemies(_missionTemplate);
    }

    private void SetupStage(int stage)
    {
        if (stage >= _missionTemplate.stages.Count)
            throw new System.Exception($"Battle Field does not support stage number {stage}.");

        var spawnPoints = _battleField.GetTheatreSpawnPoints(_currentStage);
        var enemyCharacters = SpawnCharacters(_missionTemplate.stages[stage].MissionSpawns, spawnPoints, BattleEntityTeam.Enemy, true);
        foreach(var character in enemyCharacters)
        {
            character.OnDefeat += EnemyDefeated;
            _remainingStageEnemies++;
        }

        Debug.Log($"Stage {stage} preperation: Places, everyone! Places!");
        foreach (var entity in BattleModel.Instance.ActiveBattleEntities.Data)
        {
            if (entity.Team == BattleEntityTeam.Player)
            {
                Vector3 position = _battleField.GetRowPosition(_currentStage, entity.transform.position, entity.CharacterTemplate.CharacterRow);
                _preparingCharacters++;
                entity.EnterPrep(position);
            }
        }
    }

    public void OnPreperationComplete()
    {
        _preparingCharacters--;
        if (_preparingCharacters <= 0)
        {
            foreach (var entity in BattleModel.Instance.ActiveBattleEntities.Data)
                entity.StartCombat();
        }
    }

    public List<BattleEntity> SpawnCharacters(List<CharacterSpawnTemplate> characterSpawnTemplates, List<SpawnPoint> spawnPoints, BattleEntityTeam team, bool randomisePoints)
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
                if (randomisePoints)
                    spawnPoint = RandomUtils.RandomFrom(rowSpawns[characterTemplate.CharacterRow]);
                else
                    spawnPoint = rowSpawns[characterTemplate.CharacterRow][0];
                rowSpawns[characterTemplate.CharacterRow].Remove(spawnPoint);
                allSpawnPoints.Remove(spawnPoint);
            }
            else if (allSpawnPoints.Count > 0)
            {
                if (randomisePoints)
                    spawnPoint = RandomUtils.RandomFrom(allSpawnPoints);
                else
                    spawnPoint = allSpawnPoints[0];
                allSpawnPoints.Remove(spawnPoint);
                Debug.LogWarning($"There are not enough \"{characterTemplate.CharacterRow}\" spawn points");
            }
            else
            {
                throw new System.Exception("There are not enough total spawn points for this wave");
            }

            BattleEntity newEntity = Instantiate(characterTemplate.Prefab, spawnPoint.transform.position, spawnPoint.transform.rotation, transform);
            results.Add(newEntity);
            newEntity.Populate(characterTemplate, team, team == BattleEntityTeam.Player);

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
