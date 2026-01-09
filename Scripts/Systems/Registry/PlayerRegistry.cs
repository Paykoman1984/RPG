// Assets/Scripts/Systems/Registry/PlayerRegistry.cs
using UnityEngine;

public static class PlayerRegistry
{
    private static GameObject playerInstance;
    private static PlayerController playerController;
    private static IMovable movableComponent;
    private static IAttackable attackableComponent;
    private static IDashable dashableComponent;

    public static GameObject Player => playerInstance;
    public static PlayerController Controller => playerController;
    public static IMovable Movable => movableComponent;
    public static IAttackable Attackable => attackableComponent;
    public static IDashable Dashable => dashableComponent;

    public static void RegisterPlayer(GameObject player)
    {
        playerInstance = player;
        playerController = player.GetComponent<PlayerController>();
        movableComponent = player.GetComponent<IMovable>();
        attackableComponent = player.GetComponent<IAttackable>();
        dashableComponent = player.GetComponent<IDashable>();

        Debug.Log($"Player registered: {player.name}");
    }

    public static void UnregisterPlayer()
    {
        playerInstance = null;
        playerController = null;
        movableComponent = null;
        attackableComponent = null;
        dashableComponent = null;
    }

    public static bool IsPlayerRegistered => playerInstance != null;
}