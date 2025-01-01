using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CharacterColliderScript;

public class CharacterModelDataInitializer : MonoBehaviour
{
    [SerializeField] private List<WeaponColliderScript> _basicModelColliders = new List<WeaponColliderScript>();

    public List<WeaponColliderScript> GetModelBasicColliders() { return _basicModelColliders; }
}
