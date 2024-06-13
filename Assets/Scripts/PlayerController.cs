
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float terminalVelocity = 10f;
    public float gravity = 0.5f;
    private float fallingSpeed;

    public float jumpForceTotal = 200f;
    public float jumpForceStart = 50f;

    private Rigidbody rb;
    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {   
        fallingSpeed = 0f;
        grounded = false;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Movement();
    }

    private void Movement(){
        CheckMovement();
        CheckJump();
        GravityMethod();
    }

    //I think last time each time I updated the mvoement I zeroed the y and z components.
    //So need to keep them the same as their prev values (rb.velocity.y, rb.velocity.z)
    private void CheckMovement(){
        //InputManager has settings about acceleration
        float moveHorizontal = Input.GetAxis("Horizontal");
        Vector3 movement;
        //Player is jumping, need to set rb.velocity.y to 0 to stop it eventually
        if(rb.velocity.y > 0){
            movement = new Vector3(moveHorizontal, 0, 0);
        }else{
            movement = new Vector3(moveHorizontal, rb.velocity.y, rb.velocity.z);
        }
        
        rb.velocity = movement * moveSpeed;
    }

    private void CheckJump(){
        //GetButtonDown didn't work. May need to change this in future as currently you can hold it and jump a load
        if(Input.GetButton("Jump") && grounded){
            StartCoroutine(SmoothJumping());
            grounded = false;
        }
    }


    //Using own method as other gravity options wouldn't accelerate to a terminal velocity.
    private void GravityMethod(){
        if(!grounded){
            if(fallingSpeed <= terminalVelocity){
                fallingSpeed += gravity;
            }
            rb.velocity = new Vector3(rb.velocity.x, -fallingSpeed, rb.velocity.z);
        }else{
            fallingSpeed = 0;
        }
    }


    private void OnCollisionStay(Collision collision) {
		LayerMask layer = collision.gameObject.layer;
		if (layer.value == 6) {
			grounded = true;
		}
    }

    private void OnCollisionExit(Collision collision){
        LayerMask layer = collision.gameObject.layer;
        if(layer.value == 6){
            grounded = false;
        }
    }

    //Using Impulse mode made all the force get applied at the same time
    //Made it like a dash
    //So will decrease force applied over time
    private IEnumerator SmoothJumping(){
        Debug.Log("HERE");
        float currentJumpForce = jumpForceStart;
        //Keep track of how much jumpForce has been applied
        float jumpForceCumul = currentJumpForce;

        while(jumpForceCumul < jumpForceTotal){
            currentJumpForce /= 1.5f;
            jumpForceCumul += currentJumpForce;

            rb.AddForce(Vector3.up * currentJumpForce, ForceMode.VelocityChange);
            //Need to limit this to just lfet + right as otherwise it may add a force going down.
            rb.AddForce(rb.GetRelativePointVelocity(this.transform.position).normalized * currentJumpForce, ForceMode.VelocityChange);

            yield return new WaitForFixedUpdate();
        }
    }
}
