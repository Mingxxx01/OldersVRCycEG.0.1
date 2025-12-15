using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    private CharacterController cc;

    public float moveSpeed;

    public float jumpSpeed;

    private float horizontalMove,verticalMove;

    private Vector3 dir;

    public float gravity;

    public Vector3 velocity;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
    }
    private void Update()

    {
        horizontalMove = Input.GetAxis ("Horizontal")*moveSpeed;
        verticalMove = Input.GetAxis ("Vertical")*moveSpeed;

        dir = transform.forward * verticalMove + transform.right * horizontalMove;

        cc.Move(dir * Time.deltaTime);

        velocity.y -= gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

}




