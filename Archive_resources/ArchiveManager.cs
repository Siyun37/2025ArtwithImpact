using UnityEngine;
using System.Collections.Generic;

public class ArchiveManager : MonoBehaviour
{
    public static ArchiveManager Instance;
    public List<ArtworkMetadata> allMetadata = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public List<ArtworkMetadata> GetMetadataByType(string type)
    {
        return allMetadata.FindAll(m => m.type == type);
    }
}
