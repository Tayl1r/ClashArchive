using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    [System.Serializable]
    public class PlayerSpawnRows
    {
        public Transform transform;
        public PlayerSpawnRow frontRow;
        public PlayerSpawnRow middleRow;
        public PlayerSpawnRow backRow;
    }

    [System.Serializable]
    public class PlayerSpawnRow
    {
        public Vector3 start;
        public Vector3 end;

        public Vector3 GetPoint(int index, int count)
        {
            float t = (float)index / count;
            return Vector3.Lerp(start, end, t);
        }
    }

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
    [SerializeField] private PlayerSpawnRows _playerSpawnPoints = default;
    public PlayerSpawnRows PlayerSpawnPoints { get { return _playerSpawnPoints; } }
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

    public List<SpawnPoint> GetTheatreSpawnPoints(int index)
    {
        Debug.Log(index);
        if (_theatres == null || _theatres.Count < index)
            return null;
        return _theatres[index].SpawnPoints;
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
        DrawSpawnRowsGizmos(PlayerSpawnPoints);

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

    private void DrawSpawnRowsGizmos(PlayerSpawnRows spawnRows)
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