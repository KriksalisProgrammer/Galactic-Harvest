using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 90f;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Camera playerCamera;
    private float verticalVelocity = 0f;
    private bool cursorLocked = true;

    // UI References
    private InventoryUI inventoryUI;
    private bool inventoryOpen = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerController: Camera not found in children!");
        }

        // Find UI components
        inventoryUI = FindObjectOfType<InventoryUI>();

        // Subscribe to inventory events
        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryToggled += OnInventoryToggled;
        }

        SetCursorState(true);
        InventoryUI inv = FindObjectOfType<InventoryUI>();
        HotbarUI hotbar = FindObjectOfType<HotbarUI>();

        Debug.Log($"InventoryUI found: {inv != null}");
        Debug.Log($"Inventory slots parent: {inv?.inventorySlotsParent != null}");
        Debug.Log($"HotbarUI found: {hotbar != null}");
        Debug.Log($"Hotbar slots parent: {hotbar?.hotbarSlotsParent != null}");
    }

    private void Update()
    {
        HandleInput();
        CheckInventoryState();

        if (cursorLocked && !inventoryOpen)
        {
            HandleMouseLook();
        }

        HandleMovement();
    }

    private void HandleInput()
    {
        // Handle ESC key for cursor toggle and closing inventory
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryOpen && inventoryUI != null)
            {
                inventoryUI.CloseInventory();
            }
            else
            {
                cursorLocked = !cursorLocked;
                SetCursorState(cursorLocked);
            }
        }
    }

    private void CheckInventoryState()
    {
        if (inventoryUI != null)
        {
            bool isOpen = inventoryUI.IsInventoryOpen();
            if (isOpen != inventoryOpen)
            {
                inventoryOpen = isOpen;
                SetCursorState(!inventoryOpen);
            }
        }
    }

    private void OnInventoryToggled(bool isOpen)
    {
        inventoryOpen = isOpen;
        SetCursorState(!isOpen);
    }

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
        cursorLocked = locked;
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        // Don't move when inventory is open
        if (inventoryOpen) return;

        // Don't move when in planting mode
        PlayerPlanting planting = GetComponent<PlayerPlanting>();
        if (planting != null && planting.IsInPlantingMode()) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = Vector3.ClampMagnitude(move, 1f);

        HandleJumping();

        move.y = verticalVelocity;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleJumping()
    {
        if (controller.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else
            {
                verticalVelocity = -0.5f; // Small downward force to keep grounded
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    public Vector3 GetPlayerRotation()
    {
        return transform.eulerAngles;
    }

    public void SetPlayerPosition(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    public void SetPlayerRotation(Vector3 rotation)
    {
        transform.eulerAngles = new Vector3(0, rotation.y, 0);
        verticalRotation = rotation.x;

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    public bool IsMovementDisabled()
    {
        return inventoryOpen ||
               (GetComponent<PlayerPlanting>()?.IsInPlantingMode() ?? false);
    }

    public Camera GetPlayerCamera()
    {
        return playerCamera;
    }

    private void OnDestroy()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryToggled -= OnInventoryToggled;
        }
    }
}