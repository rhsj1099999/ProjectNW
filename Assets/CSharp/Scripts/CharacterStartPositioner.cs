using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStartPositioner : MonoBehaviour
{
    void Start()
    {
        PlayerScript playerScript = FindFirstObjectByType<PlayerScript>();
        playerScript.GCST<CharacterContollerable>().CharacterTeleport(transform.position);
    }
}
