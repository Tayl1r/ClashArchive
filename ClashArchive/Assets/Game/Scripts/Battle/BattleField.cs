using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class BattleFieldBounds
{
    public Vector2 offset;
    public Vector2 size;
}

[System.Serializable]
public class BattleFieldTheatre
{
    public Transform centrePoint;
    public List<SpawnPoint> SpawnPoints;
}

public class BattleField : MonoBehaviour
{
    public static BattleField Instance;

    [Header("Stage Setup")]
    [SerializeField] private List<SpawnPoint> _playerSpawnPoints = default;
    public List<SpawnPoint> PlayerSpawnPoints { get { return _playerSpawnPoints; } }
    [Space]
    [SerializeField] private List<BattleFieldTheatre> _theatres = default;
    [Space]
    public Cinemachine.CinemachineTargetGroup targetGroup = default; // TODO: Probably make this its own class

    [Header("Camera Bounds")]
    [SerializeField] private BattleFieldBounds _mainBounds = default;

    [Header("Debug")]
    [SerializeField] private bool _alwaysDrawDebug = default;

    void Start()
    {
        Instance = this;
    }

    private BattleFieldTheatre GetTheatre(int index)
    {
        if (_theatres == null || _theatres.Count <= index)
            return null;
        return _theatres[index];
    }

    public List<SpawnPoint> GetTheatreSpawnPoints(int index)
    {
        return GetTheatre(index).SpawnPoints;
    }

    public bool IsLeftOfBounds(Vector3 point)
    {
        Vector3 origin = targetGroup.transform.position + new Vector3(_mainBounds.offset.x, 0, _mainBounds.offset.y);
        Vector3 size = new Vector3(_mainBounds.size.x, 0, _mainBounds.size.y) * 0.5f;

        if (origin.x - size.x > point.x)
            return true;
        return false;
    }

    public bool IsPointInBounds(Vector3 point)
    {
        Vector3 origin = targetGroup.transform.position + new Vector3(_mainBounds.offset.x, 0, _mainBounds.offset.y);
        Vector3 size = new Vector3(_mainBounds.size.x, 0, _mainBounds.size.y) * 0.5f;

        if (origin.x - size.x > point.x)
            return false;
        if (origin.x + size.x < point.x)
            return false;
        /*if (origin.z - size.z > point.z)
            return false;
        if (origin.z + size.z < point.z)
            return false;*/

        return true;
    }

    public Vector3 GetRowPosition(int stage, Vector3 currentPosition, CharacterRow characterRow)
    {
        var theatre = GetTheatre(stage);       
        float x = theatre.centrePoint.transform.position.x;
        Vector3 result = currentPosition;

        switch(characterRow)
        {
            case CharacterRow.Middle:
                x -= 3;
                break;
            case CharacterRow.Back:
                x -= 6;
                break;
            default:
                x -= 0;
                break;
        }
        result.x = x;

        if (NavMesh.SamplePosition(result, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            return hit.position;

        return result;
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
        if (_alwaysDrawDebug)
            DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        if (_theatres != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _theatres.Count; i++)
            {
                if (_theatres[i].centrePoint != null)
                {
                    Gizmos.DrawWireSphere(_theatres[i].centrePoint.position, 1f);
                    if (i != 0 && _theatres[i - 1].centrePoint != null)
                        Gizmos.DrawLine(_theatres[i].centrePoint.position, _theatres[i].centrePoint.position);

                    foreach (var spawnPoint in _theatres[i].SpawnPoints)
                        Gizmos.DrawLine(_theatres[i].centrePoint.transform.position, spawnPoint.transform.position);
                }
            }
        }

        Gizmos.color = Color.white;
        DrawBounds(_mainBounds);
    }

    private void DrawBounds(BattleFieldBounds bounds)
    {
        if (targetGroup != null)
        {
            Vector3 origin = targetGroup.transform.position + new Vector3(bounds.offset.x, 0, bounds.offset.y);
            Vector3 size = new Vector3(bounds.size.x, 0, bounds.size.y);
            Gizmos.DrawWireCube(origin, size);
        }
    }
    #endregion
}