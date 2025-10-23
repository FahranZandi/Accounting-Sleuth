using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    public GameObject interactUI;        // drag Canvas (child dari door)
    public TextMeshProUGUI option1Text;  // drag TMP text "Masuk"
    public TextMeshProUGUI option2Text;  // drag TMP text "Batal"

    [Header("Scene Settings")]
    public string targetSceneName = "";

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

    private void Start()
    {
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
        // Debug bantu cek apakah trigger bekerja
        Debug.Log("Trigger Enter: " + other.name);

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
        Debug.Log("Trigger Exit: " + other.name);

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
        // Ganti pilihan dengan scroll atau panah
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

        // Konfirmasi pilihan
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (selectedIndex == 0)
            {
                // Masuk ke scene target
                if (Application.CanStreamedLevelBeLoaded(targetSceneName))
                    SceneManager.LoadScene(targetSceneName);
                else
                    Debug.LogError($"Scene '{targetSceneName}' belum ditambahkan ke Build Settings!");
            }
            else
            {
                // Batal
                interactUI.SetActive(false);
                isNearby = false;
            }
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
