using UnityEngine;

/// <summary>
/// Put this on any serialized field to group it into a collapsible foldout
/// in the Inspector. Fields sharing the same group name are drawn together,
/// in the order their group name is first encountered on the class.
///
/// Usage:
///
///     [Foldout("Movement")]
///     public float speed;
///
///     [Foldout("Movement")]
///     public float jumpHeight;
///
///     [Foldout("Combat")]
///     public int damage;
///
///     public string notGrouped; // drawn normally, above the foldouts
/// </summary>
public class FoldoutAttribute : PropertyAttribute
{
    public readonly string GroupName;

    public FoldoutAttribute(string groupName)
    {
        GroupName = groupName;
    }
}