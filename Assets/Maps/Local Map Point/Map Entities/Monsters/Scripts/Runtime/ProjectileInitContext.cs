using UnityEngine;
using System.Collections.Generic;

public class ProjectileInitContext : Heros.Attacks.AttackInitContext
{
    public bool homing;
    public bool bounce;
    public Transform target;
    public Vector2 direction;
    public LayerMask layerMask;
    public ProjectileOwner owner = ProjectileOwner.Monster;
    public HealthAndShieldManager[] ignoreExceptions;
}
