using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Camera playerCamera;

    public GameObject plantPrefab; 

    private float verticalVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerController: Camera not found in children!");
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
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

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

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
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPlantSeed();
        }

        move.y = verticalVelocity;

            controller.Move(move * moveSpeed * Time.deltaTime);
    }
    void TryPlantSeed()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                Vector3 plantPosition = hit.point;
                Instantiate(plantPrefab, plantPosition, Quaternion.identity);
                Debug.Log("Посадили растение!");
            }
        }
    }


}
