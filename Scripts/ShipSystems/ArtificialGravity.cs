using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtificialGravity : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    public Rigidbody2D Rb => rb;

    private void OnValidate() {
        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();

        gameObject.layer = LayerMask.NameToLayer("ArtificialGravity");
    }
}
