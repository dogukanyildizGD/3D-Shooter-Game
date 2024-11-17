using UnityEngine;
using Mirror;

public class BotAI : NetworkBehaviour
{
    public float moveSpeed = 3.0f;
    private Transform target;

    void Start()
    {
        if (!isServer) return; // AI sadece sunucu taraf�nda �al���r

        FindClosestPlayer();
    }

    void Update()
    {
        if (!isServer) return; // AI mant��� sunucu taraf�nda �al��mal�

        if (target != null)
        {
            // Hedefe do�ru hareket et
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }
    }

    private void FindClosestPlayer()
    {
        var players = FindObjectsOfType<LobbyPlayer>();
        float minDistance = float.MaxValue;
        LobbyPlayer closestPlayer = null;

        foreach (var player in players)
        {
            if (player.isLocalPlayer) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            target = closestPlayer.transform;
        }
    }
}
