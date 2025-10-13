using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterHealth : NetworkBehaviour, IDamageable
{
    [SerializeField] private bool Debugging;

    [SerializeField] private float _health = 100;
    public float Health => _health;
    [field: SerializeField] public float MaxHealth { get; set; } = 100;

    [SerializeField] private GameObject ExplosionPrefab;
    
    public void ReceiveDamage(float damageAmount, Vector3 hitPosition, GameAgent sender)
    {
        _health -= damageAmount;
        if (Debugging)
        {
            Debug.Log(
                $"CharacterHealth.ReceiveDamage. New Health: {_health}. Attacker: {sender.gameObject.name}. Attacker faction: {sender.ShipFaction}");
        }

        if (_health <= 0)
        {
            if (IsServer)
            {
                VisualizeExplosion();
                Destroy(gameObject);
            }
            VisualizeExplosionClientRpc();
        }
    }

    public void ReceiveHeal(float healAmount, Vector3 hitPosition, GameAgent sender)
    {
        _health += healAmount;
    }
    
    private void VisualizeExplosion()
    {
        Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void VisualizeExplosionClientRpc()
    {
        VisualizeExplosion();
    }
}