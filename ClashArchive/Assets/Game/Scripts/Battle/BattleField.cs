using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleField : MonoBehaviour
{
    [SerializeField] private BattleFieldTheatre _playerSpawnPoints = default;
    public BattleFieldTheatre PlayerSpawnPoints { get { return _playerSpawnPoints; } }
    [SerializeField] private List<BattleFieldTheatre> _theatres = default;

    public BattleFieldTheatre SetTheatre(int stage)
    {
        if (stage >= _theatres.Count)
            return null;
        _playerSpawnPoints.transform.position = _theatres[stage].transform.position;
        return _theatres[stage];
    }

    private void OnDrawGizmos()
    {
        if (_theatres == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (var theatre in _theatres)
            if (theatre != null)
                Gizmos.DrawWireSphere(theatre.transform.position, 1f);
    }
}