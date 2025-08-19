using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runMultiplier = 2f;
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;

    [Header("Hotbar Setup")]
    public PlantSeed[] startingSeeds; 
    public HotbarUI hotbarUI;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private bool isRunning = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        SetupInitialHotbar();
    }

    private void SetupInitialHotbar()
    {
        if (hotbarUI != null && startingSeeds != null)
        {
            for (int i = 0; i < startingSeeds.Length && i < hotbarUI.hotbarSize; i++)
            {
                if (startingSeeds[i] != null)
                {
                    hotbarUI.SetItemToSlot(i, startingSeeds[i]);
                }
            }
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleCursorToggle();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Проверяем бег
        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = move.normalized * currentSpeed;

        // Простая гравитация
        if (!controller.isGrounded)
        {
            move.y = -9.81f;
        }

        controller.Move(move * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void AddSeedToHotbar(PlantSeed seed)
    {
        if (hotbarUI == null || seed == null) return;

        for (int i = 0; i < hotbarUI.hotbarSize; i++)
        {
            var slot = hotbarUI.GetSlot(i);
            if (slot != null && !slot.HasItem())
            {
                hotbarUI.SetItemToSlot(i, seed);
                Debug.Log($"Добавлено семя {seed.plantData.plantName} в слот {i}");
                return;
            }
        }

        Debug.Log("Нет свободных слотов в хотбаре");
    }
}