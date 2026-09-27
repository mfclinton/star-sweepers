using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeGenerator : MonoBehaviour
{
    #region Fields

    [Header("Prefabs")]
    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private GameObject headPrefab;

    [Header("Attach Point / Spawn Point")]
    [SerializeField] private Transform ropeAttachPoint;
    [SerializeField] private Transform ropeHeadSpawnPoint;

    [Header("Rope Settings")]
    [SerializeField] private int linkCount;

    private List<GameObject> ropeLinks;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        GenerateRope();
    }

    #endregion

    #region Private Methods

    private void GenerateRope()
    {
        // Instantiate the bodies
        ropeLinks = new List<GameObject>();

        for (int i = 0; i < linkCount; i++)
        {
            GameObject newLink = Instantiate(bodyPrefab, transform);

            HingeJoint2D joint = newLink.GetComponent<HingeJoint2D>();
            
            if(i == 0)
                joint.limits = new JointAngleLimits2D() { min = 0, max = 360 };

            joint.connectedBody = (i > 0) ? ropeLinks[i - 1].GetComponent<Rigidbody2D>() : ropeAttachPoint.GetComponent<Rigidbody2D>();

            ropeLinks.Add(newLink);
        }

        // Instantiate the head
        GameObject head = Instantiate(headPrefab, transform);

        HingeJoint2D headJoint = head.GetComponent<HingeJoint2D>();
        headJoint.connectedBody = ropeLinks[linkCount - 1].GetComponent<Rigidbody2D>();
        
        head.transform.position = ropeHeadSpawnPoint.position;

        ropeLinks.Add(head);
    }

    #endregion
}
