using UnityEngine;

public class BindingSpriteMapHolder : MonoBehaviour
{
    public static BindingSpriteMapHolder Instance { get; private set; }

    public BindingSpriteMap BindingSpriteMap;

    void Awake()
    {
        Instance = this;
    }
}
