using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private CharacterRow _characterRow;
    public CharacterRow CharacterRow { get { return _characterRow; } }

    public void OnDrawGizmos()
    {
        switch (_characterRow)
        {
            case CharacterRow.Front:
                Gizmos.color = Color.red;
                break;
            case CharacterRow.Middle:
                Gizmos.color = Color.yellow;
                break;
            case CharacterRow.Back:
                Gizmos.color = Color.green;
                break;
            case CharacterRow.Boss:
                Gizmos.color = Color.magenta;
                break;
        }
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
