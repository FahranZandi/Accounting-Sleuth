using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    public GameObject interactUI;
    public TextMeshProUGUI option1Text;
    public TextMeshProUGUI option2Text;

    [Header("Scene Settings")]
    public string targetSceneName = "";

    [Header("Door Settings")]
    public string doorID = "";

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset sceneAsset;
    private void OnValidate()
    {
        if (sceneAsset != null)
            targetSceneName = sceneAsset.name;
    }
#endif

    private Transform player;
    private int selectedIndex = 0;
    private bool isNearby = false;
    private Collider triggerZone;

    private void Start()
    {
        triggerZone = GetComponent<Collider>();

        if (!triggerZone.isTrigger)
            triggerZone.isTrigger = true;

        if (interactUI != null)
            interactUI.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        if (!isNearby || player == null) return;

        HandleInput();
        FaceUIToCamera();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isNearby = true;

            if (interactUI != null)
                interactUI.SetActive(true);

            UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearby = false;
            player = null;

            if (interactUI != null)
                interactUI.SetActive(false);
        }
    }

    private void HandleInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + 2) % 2;
            UpdateUI();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % 2;
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedIndex == 0)
                ExecuteDoorAction();
            else
            {
                if (interactUI != null)
                    interactUI.SetActive(false);
                isNearby = false;
            }
        }
    }

    private void ExecuteDoorAction()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool sameScene = string.IsNullOrEmpty(targetSceneName) || targetSceneName == currentScene;

        if (!sameScene)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.lastDoorID = doorID;
                Debug.Log($"[DoorInteraction] lastDoorID set = {doorID}");
            }

            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            // --- Perbaikan: API baru ---
            DoorSpawn[] spawns = Object.FindObjectsByType<DoorSpawn>(FindObjectsSortMode.None);

            foreach (var ds in spawns)
            {
                if (ds.doorID == doorID)
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.TeleportPlayerTo(ds);
                    return;
                }
            }

            Debug.LogWarning($"[DoorInteraction] DoorSpawn dengan ID '{doorID}' tidak ditemukan.");
        }
    }

    private void UpdateUI()
    {
        if (option1Text != null)
            option1Text.text = (selectedIndex == 0 ? "> Masuk" : "  Masuk");

        if (option2Text != null)
            option2Text.text = (selectedIndex == 1 ? "> Batal" : "  Batal");
    }

    private void FaceUIToCamera()
    {
        if (interactUI == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 dir = interactUI.transform.position - cam.transform.position;
        interactUI.transform.rotation = Quaternion.LookRotation(dir);
    }
}
