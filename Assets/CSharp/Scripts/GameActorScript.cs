using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameActorScript : MonoBehaviour
{
    public enum ActorType
    {
        Character,
        Prop,
        NPC,
    }

    protected ActorType _actorType = ActorType.Character;
}
