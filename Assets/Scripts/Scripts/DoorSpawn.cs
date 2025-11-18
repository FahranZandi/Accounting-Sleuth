using UnityEngine;

public class DoorSpawn : MonoBehaviour
{
    [Tooltip("ID unik untuk pintu ini. Cocokkan dengan DoorInteraction.doorID dari pintu pasangan.")]
    public string doorID;

    [Tooltip("Transform tempat player akan muncul ketika masuk lewat pintu pairing ini.")]
    public Transform spawnPoint;

    private void Reset()
    {
        // Jika belum ada spawnPoint, buat child empty otomatis (opsional)
        if (spawnPoint == null)
        {
            GameObject go = new GameObject("SpawnPoint");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            spawnPoint = go.transform;
        }
    }
}
