using UnityEngine;
using System.Collections;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private GameObject anchor;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset;

    // Camera shake variables
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.7f;
    private float dampingSpeed = 1.0f;
    private Vector3 shakePosition;

    public static WorldManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        anchor = FindObjectOfType<Ship>().gameObject;
    }

    void FixedUpdate()
    {
        if (shakeDuration > 0)
        {
            shakePosition = Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            shakePosition = Vector3.zero;
        }
        Vector3 desiredPosition = new Vector3(anchor.transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z + offset.z) + shakePosition;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void TriggerShake()
    {
        shakeDuration = 0.3f;
    }
}
