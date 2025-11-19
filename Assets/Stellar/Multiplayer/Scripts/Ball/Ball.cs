using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    [Networked]
    private TickTimer life { get; set; }

    [SerializeField] private float speed = 5f;
    [SerializeField] private float hitRadius = 0.5f;

    private static readonly HashSet<Player> s_players = new HashSet<Player>();

    public static void RegisterPlayer(Player p)
    {
        s_players.Add(p);
    }

    public static void UnregisterPlayer(Player p)
    {
        s_players.Remove(p);
    }

    public void Init()
    {
        life = TickTimer.CreateFromSeconds(Runner, 5f);
    }

    public override void FixedUpdateNetwork()
    {
        if (life.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        transform.position += speed * transform.forward * Runner.DeltaTime;

        if (!Object.HasStateAuthority)
            return;

        PlayerRef shooter = Object.InputAuthority;

        foreach (var p in s_players)
        {
            if (p == null || p.Object == null)
                continue;

            if (p.Object.InputAuthority == shooter)
                continue;

            if ((p.transform.position - transform.position).sqrMagnitude <= hitRadius * hitRadius)
            {
                p.ApplyHit();
                Runner.Despawn(Object);
                break;
            }
        }
    }
}
