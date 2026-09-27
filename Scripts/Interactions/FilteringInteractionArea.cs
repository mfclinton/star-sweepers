using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilteringInteractionArea : InteractionAreaBase
{
    [SerializeField] private FilterSystem filterSystem;

    public override void ExecuteInteraction(PlayerController playerController)
    {
        filterSystem.CycleFilter();
    }
}
