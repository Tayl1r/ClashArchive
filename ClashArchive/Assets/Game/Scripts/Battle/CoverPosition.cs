using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPosition : MonoBehaviour
{
    public bool IsActive { private set; get; }

    private void Awake()
    {
        IsActive = true;
    }

    private void Start()
    {
        BattleModel.Instance.ActiveCoverEntities.AddMember(this);
    }

    private void OnDestroy()
    {
        BattleModel.Instance.ActiveCoverEntities.RemoveMember(this);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (IsActive)
            DrawGizmos();
    }

    public void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
            return;

        DrawGizmos();
    }

    private void DrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 position = GetPosition();
        Vector3 angle1 = position + (Quaternion.AngleAxis(45f, Vector3.up) * transform.forward);
        Vector3 angle2 = position + (Quaternion.AngleAxis(-45f, Vector3.up) * transform.forward);
        Gizmos.DrawLine(position, angle1);
        Gizmos.DrawLine(position, angle2);
        Gizmos.DrawLine(angle1, angle2);
    }
}
