using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolReturnTrigger : MonoBehaviour
{

    private void OnTriggerExit2D(Collider2D other)
    {
        string tag = other.gameObject.tag;

        bool success = ObjectPooler.Instance.ReturnToPool(tag, other.gameObject);
        if(!success)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if(player != null)
            {
                player.HandlePlayerDeath();
                return;
            }

            other.gameObject.SetActive(false);
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist. Deactivating object out of bounds.");
        }
    }
}
