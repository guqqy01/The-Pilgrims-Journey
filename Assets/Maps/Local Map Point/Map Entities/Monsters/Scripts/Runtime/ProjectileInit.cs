using UnityEngine;
using System.Collections.Generic;

public class ProjectileInit : Attacks.AttackInit
{
    public bool homing;
    public bool bounce;
    public Transform target;
    public Vector2 direction;
    public LayerMask layerMask;
    public ProjectileOwner owner = ProjectileOwner.Monster;
    public HealthAndShieldManager[] ignoreExceptions;
}
