using System;
using UnityEngine;

public static class GameObjectUtils
{
    public static void SetActiveIfExists(GameObject gameObject, bool isActive)
    {
        if (gameObject == null)
            return;
        gameObject.SetActive(isActive);
    }

    public static void SetActiveIfExists(Component component, bool isActive)
    {
        if (component == null)
            return;
        SetActiveIfExists(component.gameObject, isActive);
    }
}
