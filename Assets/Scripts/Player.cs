using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    float sensitivity = 1f;

    private void Start() {

        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

    }

    private void Update() {

        GetPlayerInputs();
        GetPlayerLookRotation();

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * walkSpeed;
        velocity += Vector3.up * gravity * Time.deltaTime;

        velocity.y = checkDownSpeed(velocity.y);

        transform.Translate(velocity, Space.World);

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

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequest = true;

    }

    private float checkDownSpeed (float downSpeed) {

        if (
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)
           ) {

            isGrounded = true;
            return 0;

        } else {
            isGrounded = false;
            return downSpeed;
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
