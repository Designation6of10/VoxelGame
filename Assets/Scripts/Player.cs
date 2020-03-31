using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 6.0f;
    public float sprintSpeed = 12f;
    public float jumpForce = 10f;
    public float gravity = -19.6f;

    public float playerWidth = 0.65f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    public float minYRotation = -85.0f;
    public float maxYRotation = 90.0f;
    private float currentYRotation = 0.0f;
    private float currentXRotation = 0.0f;
    float sensitivity = 3f;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 16.0f;

    public Text selectedBlockText;
    public byte selectedBlockIndex = 1;

    private void Start() {

        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
        selectedBlockText.text = world.blocktypes[selectedBlockIndex].blockName + " block selected.";

    }

    private void FixedUpdate() {

        GetPlayerLookRotation();
        CalculateVelocity();
        if (jumpRequest)
            Jump();

        transform.Translate(velocity, Space.World);
    }

    private void Update() {

        GetPlayerInputs();
        placeCursorBlocks();

    }

    void Jump () {

        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }

    private void CalculateVelocity() {

        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping)
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);

    }

    private void GetPlayerInputs () {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButton("Jump"))
            jumpRequest = true;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll !=0) {

            if (scroll > 0)
                selectedBlockIndex++;
            else
                selectedBlockIndex--;

            if (selectedBlockIndex > (byte)(world.blocktypes.Length - 1))
                selectedBlockIndex = 1;
            if (selectedBlockIndex < 1)
                selectedBlockIndex = (byte)(world.blocktypes.Length - 1);

            selectedBlockText.text = world.blocktypes[selectedBlockIndex].blockName + " block selected.";

        }

        if (highlightBlock.gameObject.activeSelf) {

            //Destroy block
            if (Input.GetMouseButtonDown(0))
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);

            //Place block
            if (Input.GetMouseButtonDown(1))
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, selectedBlockIndex);

        }

    }

    private void placeCursorBlocks() {

        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach) {

            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos)) {

                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;

            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;

        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }

    private float checkDownSpeed (float downSpeed) {

        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
           ) {

            isGrounded = true;
            return 0;

        } else {
            isGrounded = false;
            return downSpeed;
        }

    }

    private float checkUpSpeed(float upSpeed) {

        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 4f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 4f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 4f + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 4f + upSpeed, transform.position.z + playerWidth))
           ) {

            isGrounded = true;
            return 0;

        }
        else {
            isGrounded = false;
            return upSpeed;
        }

    }

    public bool front {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    public bool back {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    public bool left {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }
    public bool right {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }

    private void GetPlayerLookRotation() {

        //Left and right
        currentXRotation += (mouseHorizontal * sensitivity);
        transform.eulerAngles = new Vector3(0, currentXRotation, 0);

        //Up and down
        currentYRotation += (-mouseVertical * sensitivity);
        currentYRotation = Mathf.Clamp(currentYRotation, minYRotation, maxYRotation);
        cam.transform.localEulerAngles = new Vector3(currentYRotation, 0, 0);

    }

}
