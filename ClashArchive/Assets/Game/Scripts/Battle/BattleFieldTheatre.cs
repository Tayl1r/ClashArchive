using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFieldTheatre : MonoBehaviour
{
    public SpawnPoint[] SpawnPoints;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (SpawnPoints != null)
            foreach (var spawn in SpawnPoints)
                if (spawn != null)
                    Gizmos.DrawLine(transform.position, spawn.transform.position);
    }
}
