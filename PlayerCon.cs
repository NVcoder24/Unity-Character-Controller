/*
 * Universal physics (Rigidbody) based player controller
 * Camera Movement, Player Movement, Jumping, Cursor state control,
 * Running, Slope detection, Wall running
 * For Unity Engine
 *
 * By NVcoder
 * My Github: https://github.com/NVcoder24
 * Version: 1.0
*/

// Libraries
using UnityEngine;
using TMPro;

// Main script`s class "PlayerCon" (mono behaviour)
public class PlayerCon : MonoBehaviour
{
    // A lot of customizable settings (serialized fields)
    [Header("Movement")]
    // Player movement stuff
    [SerializeField] float playerSpeed = 6f;
    [SerializeField] float playerSpeedRun = 12f;
    [SerializeField] float speedMul = 10f;
    [SerializeField] float speedMulAir = 2f;
    [SerializeField] Vector3 jumpForce = new Vector3(0f, 10f, 0f);
    [SerializeField] Vector3 jumpWallRun = new Vector3(15f, 7f, 15f);

    [Header("Camera")]
    // Camera stuff
    [SerializeField] Camera Camera = null;
    [SerializeField] float sensitivity = 8f;
    [SerializeField] float staticFov = 70f;
    [SerializeField] float wallRunFov = 80f;
    [SerializeField] float fovChangeTime = 5f;
    [SerializeField] float staticTilt = 0f;
    [SerializeField] float wallRunTilt = 10f;
    [SerializeField] float tiltChangeTime = 10f;
    [SerializeField] bool isLocked = true;

    [Header("Physics")]
    // Physics stuff
    [SerializeField] float drag = 6f;
    [SerializeField] float dragAir = 1f;
    [SerializeField] float wallRunGravity = 1f;
    [SerializeField] float wallRunJumpMul = 50f;

    [Header("Keybinds")]
    // Keybinds
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode runKey = KeyCode.LeftShift;

    [Header("Sounds")]
    // Sound stuff

    // Rigidbody
    Rigidbody rb = null;

    // Rigidbody setup
    void setupRB() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Start
    void Start() {
        setupRB();
    }

    // Predefining variables
    float yRotation;                           // X rotation
    float xRotation;                           // Y rotation
    float tilt;                                // Camera wall run tilt
    bool isGrounded = false;                   // Is grounded
    bool isSlope = false;                      // Is on slope
    bool wallLeft = false;                     // Wall left
    bool wallRight = false;                    // Wall right
    Vector3 slopeMoveDirection = Vector3.zero; // Slope move direction
    RaycastHit leftWallHit;                    // Wall left  raycast hit
    RaycastHit rightWallHit;                   // Wall right raycast hit

    // Rigidbody drag control
    void dragCon() {
        if (isGrounded) {
            rb.drag = drag;    // Rigidbody drag
        } else {
            rb.drag = dragAir; // In air Rigidbody drag
        }
    }

    // Camera rotation
    void cameraRotation() {
        float camX = Input.GetAxis("Mouse X"); // Mouse X
        float camY = Input.GetAxis("Mouse Y"); // Mouse Y

        yRotation -= camY * sensitivity;               // Y rotation
        yRotation = Mathf.Clamp(yRotation, -90f, 90f); // Clamp Y rotation

        xRotation += camX * sensitivity; // X rotation

        transform.rotation = Quaternion.Euler(0, xRotation, 0);                // Camera rotation
        Camera.transform.rotation = Quaternion.Euler(yRotation, xRotation, tilt); // Player rotation
    }

    // Jumping
    void jumping() {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.y + 0.2f); // Is grounded (raycast)

        if (isGrounded && Input.GetKeyDown(jumpKey)) {
            rb.AddForce(jumpForce, ForceMode.Impulse); // Jumping (impulse)
        }
    }

    // Cursor state control
    void cursorState() {
        if (isLocked) {
            Cursor.lockState = CursorLockMode.Locked;   // Lock cursor
        } else {
            Cursor.lockState = CursorLockMode.Confined; // Unlock cursor
        }
        Cursor.visible = !isLocked; // Cursor visibility
    }

    // Slope detection
    void slopeHandling() {
        RaycastHit slopeHit; // Raycast hit info
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, transform.localScale.y + 0.5f)) { // If something under player
            if (slopeHit.normal != Vector3.up) { // If slope not straight
                isSlope = true;  // Is slope true
            } else {
                isSlope = false; // false
            }
        } else {
            isSlope = false;     // false
        }

        slopeMoveDirection = Vector3.ProjectOnPlane(direction, slopeHit.normal); // Set slope move direction (project on plane)
    }

    // Wall run
    void wallRunning() {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, 1f);  // Raycasting for left  wall
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, 1f); // Raycasting for right wall

        if (!Physics.Raycast(transform.position, Vector3.down, transform.localScale.y + 0.2f)) { // If player in air
            if (wallLeft) {
                StartWR(); // Start wall run on left  wall
            } else if (wallRight) {
                StartWR(); // Start wall run on right wall
            } else {
                StopWR();  // Stop wall run
            }
        } else {
            StopWR();      // Stop wall run
        }
    }

    // Update loop
    void Update() {
        dragCon();
        cameraRotation();
        jumping();
        cursorState();
        slopeHandling();
        wallRunning();
    }

    // Start wall run
    void StartWR() {
        rb.useGravity = false; // Turn Rigidbody gravity

        Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, wallRunFov, fovChangeTime * Time.deltaTime); // Change field of view for effect

        if (wallLeft)
            tilt = Mathf.Lerp(tilt, -wallRunTilt, tiltChangeTime * Time.deltaTime); // Camera tilt for left  wall for effect
        else if (wallRight)
            tilt = Mathf.Lerp(tilt, wallRunTilt, tiltChangeTime * Time.deltaTime);  // Camera tilt for right wall for effect

        // Wall run settings control
        Vector3 wallRunJump = jumpWallRun; // Static wall run jump
        float mul = wallRunJumpMul;        // Static wall run jump multiplier
        float gravity = wallRunGravity;    // Static wall run gravity

        if (wallLeft && leftWallHit.collider.gameObject.GetComponent<CustomWallRunWall>() != null) {
            CustomWallRunWall wall = leftWallHit.collider.gameObject.GetComponent<CustomWallRunWall>(); // Custom wall
            wallRunJump = wall.wallRunJump; // Set wall run jump
            mul = wall.wallRunJumpMul;      // Set wall run jump multiplier
            gravity = wall.wallRunGravity;  // Set wall run gravity
        }

        if (wallRight && rightWallHit.collider.gameObject.GetComponent<CustomWallRunWall>() != null) {
            CustomWallRunWall wall = rightWallHit.collider.gameObject.GetComponent<CustomWallRunWall>(); // Custom wall
            wallRunJump = wall.wallRunJump; // Set wall run jump
            mul = wall.wallRunJumpMul;      // Set wall run jump multiplier
            gravity = wall.wallRunGravity;  // Set wall run gravity
        }

        // Wall run gravity
        rb.AddForce(Vector3.down * gravity, ForceMode.Force); // Add wall run gravity force

        // Jumping
        if (Input.GetKeyDown(jumpKey))
        {
            if (wallLeft)
            {
                Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal; // Get wall direction
                rb.AddForce(new Vector3(wallRunJump.x * wallRunJumpDirection.x, wallRunJump.y, wallRunJump.z * wallRunJumpDirection.z) * mul); // Add force
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDirection = transform.up + rightWallHit.normal; // Get wall direction
                rb.AddForce(new Vector3(wallRunJump.x * wallRunJumpDirection.x, wallRunJump.y, wallRunJump.z * wallRunJumpDirection.z) * mul); // Add force
            }
        }
    }

    // Stop wall run
    void StopWR() {
        rb.useGravity = true; // Turn Rigidbody gravity on

        Camera.fieldOfView = Mathf.Lerp(Camera.fieldOfView, staticFov, fovChangeTime * Time.deltaTime); // Reset camera field of view
        tilt = Mathf.Lerp(tilt, staticTilt, tiltChangeTime * Time.deltaTime);                           // Reset camera tilt
    }

    // Predefining variables
    Vector3 direction = Vector3.zero;

    // Player Movement
    void movement() {
        float movY = Input.GetAxisRaw("Horizontal"); // X axis
        float movX = Input.GetAxisRaw("Vertical");   // Y axis

        Vector2 mov = new Vector2(movX, movY).normalized; // Input vector

        direction = transform.forward * movX + transform.right * movY; // Force direction

        // Speed manager
        float mul = speedMul;  // Speed multiplier
        if (!isGrounded) {
            mul = speedMulAir; // In air speed multiplier
        }

        float speed = playerSpeed;  // Player speed
        if (Input.GetKey(runKey)) {
            speed = playerSpeedRun; // Run player speed
        }

        // Move Rigidbody
        if (!isSlope) {
            rb.AddForce(direction.normalized * speed * mul, ForceMode.Acceleration);          // Add force to Rigidbody          (acceleration)
        } else {
            rb.AddForce(slopeMoveDirection.normalized * speed * mul, ForceMode.Acceleration); // Add force to Rigidbody on slope (acceleration)
        }
    }

    // Update loop (fixed) for physics stuff
    void FixedUpdate() {
        movement();
    }
}
