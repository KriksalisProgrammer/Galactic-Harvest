using System.Collections;
using System.Collections.Generic;
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

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Camera playerCamera;
    private float verticalVelocity = 0f;
    private bool cursorLocked = true;

    private bool inventoryOpen = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerController: Camera not found in children!");
        }

        SetCursorState(true);

        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            
        }
    }

    void Update()
    {
        HandleCursorToggle();
        CheckInventoryState();

        if (cursorLocked && !inventoryOpen)
        {
            HandleMouseLook();
        }

        HandleMovement();
    }

    private void CheckInventoryState()
    {
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            bool wasOpen = inventoryOpen;
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                inventoryOpen = !inventoryOpen;
                SetCursorState(!inventoryOpen);
            }
        }
    }

    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryOpen)
            {

                InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
                if (inventoryUI != null)
                {
                    inventoryUI.ToggleInventory();
                }
                inventoryOpen = false;
            }

            cursorLocked = !cursorLocked;
            SetCursorState(cursorLocked && !inventoryOpen);
        }
    }

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        if (inventoryOpen) return;

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
                verticalVelocity = -0.5f;
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
}