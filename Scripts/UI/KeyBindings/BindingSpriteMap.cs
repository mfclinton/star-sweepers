using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BindingSpritePair
{
    public string bindingPath;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "NewBindingSpriteMap", menuName = "BindingSpriteMap")]
public class BindingSpriteMap : ScriptableObject
{
    public List<BindingSpritePair> pairs = new List<BindingSpritePair>();

    // Not serialized; initialized on demand
    private Dictionary<string, Sprite> bindingSpriteDictionary;

    public Dictionary<string, Sprite> BindingSpriteDictionary
    {
        get
        {
            // Initialize the dictionary if it hasn't been already
            if (bindingSpriteDictionary == null)
            {
                bindingSpriteDictionary = new Dictionary<string, Sprite>();

                foreach (var pair in pairs)
                {
                    bindingSpriteDictionary.Add(pair.bindingPath, pair.sprite);
                }
            }

            return bindingSpriteDictionary;
        }
    }
}
