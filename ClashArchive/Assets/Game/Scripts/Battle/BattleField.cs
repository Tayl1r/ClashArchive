using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BattleField : MonoBehaviour
{
    [System.Serializable]
    public class SpawnRows
    {
        public Transform transform;
        public SpawnRow frontRow;
        public SpawnRow middleRow;
        public SpawnRow backRow;
    }

    [System.Serializable]
    public class SpawnRow
    {
        public Vector3 start;
        public Vector3 end;

        public Vector3 GetPoint(int index, int count)
        {
            float t = (float)index / (float)count;
            return Vector3.Lerp(start, end, t);
        }
    }

    public static BattleField Instance;

    [Header("Stage Setup")]
    [SerializeField] private SpawnRows _spawnPointsPlayer;
    [SerializeField] private List<SpawnRows> _spawnPointsEnemies;
    [SerializeField] private Cinemachine.CinemachineTargetGroup _targetGroup;

    [Header("Debug")]
    [SerializeField] private bool _alwaysDrawDebug = default;
    [SerializeField] private List<CharacterSpawnTemplate> playerCharacters;
    [SerializeField] private MissionTemplate _missionTemplate;

    
    private int _currentStage = 0;
    private int _remainingEnemies = 0;

    void Start()
    {
        Instance = this;
        SetupStage(_currentStage);
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
        if (stage == 0)
            SpawnCharacters(playerCharacters, _spawnPointsPlayer, BattleEntityTeam.Player, true);
        if (stage >= _missionTemplate.stages.Count)
            throw new System.Exception("Battlefield is advancing to a stage number it cannot support");
        SpawnCharacters(_missionTemplate.stages[stage].MissionSpawns, _spawnPointsEnemies[stage], BattleEntityTeam.Enemy, false);
    }

    private void SpawnCharacters(List<CharacterSpawnTemplate> characterSpawnTemplates, SpawnRows spawnRows, BattleEntityTeam team, bool isPlayer)
    {
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
            newEntity.Populate(characterTemplate, team, isPlayer);

            if (isPlayer)
                _targetGroup?.AddMember(newEntity.transform, 1f, 1f);

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

            Debug.DrawLine(spawnRows.transform.position, position, Color.magenta, 60 * 60);

            if (!isPlayer)
            {
                newEntity.OnDefeat += EnemyDefeated;
                _remainingEnemies++;
            }
        }
    }

    private void EnemyDefeated(BattleEntity entity)
    {
        entity.OnDefeat -= EnemyDefeated;
        _remainingEnemies--;
        Debug.Log(_remainingEnemies);
        if (_remainingEnemies <= 0)
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

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (_alwaysDrawDebug)
            DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        DrawSpawnRowsGizmos(_spawnPointsPlayer);
        foreach (var spawnRows in _spawnPointsEnemies)
            DrawSpawnRowsGizmos(spawnRows);
    }

    private void DrawSpawnRowsGizmos(SpawnRows spawnRows)
    {
        if (spawnRows == null || spawnRows.transform == null)
            return;

        Vector3 position = spawnRows.transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(position + spawnRows.backRow.start, position + spawnRows.backRow.end);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(position + spawnRows.middleRow.start, position + spawnRows.middleRow.end);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position + spawnRows.frontRow.start, position + spawnRows.frontRow.end);
    }
    #endregion

    /*
    [System.Serializable]
    public class BattleFieldTeamBounds
    {
        public Vector2 centre;
        public Vector2 size;
    }*/

    /*public bool IsLeftOfBounds(Vector3 point)
    {
        Vector3 origin = Position + new Vector3(_mainBounds.centre.x, 0, _mainBounds.centre.y);
        Vector3 size = new Vector3(_mainBounds.size.x, 0, _mainBounds.size.y) * 0.5f;

        if (origin.x - size.x > point.x)
            return true;
        return false;
    }

    public bool IsPointInBounds(Vector3 point)
    {
        Vector3 origin = Position + new Vector3(_mainBounds.centre.x, 0, _mainBounds.centre.y);
        Vector3 size = new Vector3(_mainBounds.size.x, 0, _mainBounds.size.y) * 0.5f;

        if (origin.x - size.x > point.x)
            return false;
        if (origin.x + size.x < point.x)
            return false;
        if (origin.z - size.z > point.z)
            return false;
        if (origin.z + size.z < point.z)
            return false;

        return true;
    }

    public Vector3 GetTravelDirectionForTeam(BattleEntityTeam team)
    {
        if (team == BattleEntityTeam.Player)
            return Vector3.right;
        return Vector3.left;
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (alwaysDrawDebug)
            DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        if (_stageOrigins != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _stageOrigins.Length; i++)
            {
                Gizmos.DrawWireSphere(_stageOrigins[i], 1f);
                if (i != 0)
                    Gizmos.DrawLine(_stageOrigins[i - 1], _stageOrigins[i]);
            }
        }

        Gizmos.color = Color.green;
        DrawBounds(_playerBounds);
        Gizmos.color = Color.red;
        DrawBounds(_enemyBounds);
        Gizmos.color = Color.white;
        DrawBounds(_mainBounds);
    }

    private void DrawBounds(BattleFieldTeamBounds bounds)
    {
        Vector3 origin = Position + new Vector3(bounds.centre.x, 0, bounds.centre.y);
        Vector3 size = new Vector3(bounds.size.x, 0, bounds.size.y);
        Gizmos.DrawWireCube(origin, size);
    }
    #endregion
*/
}
