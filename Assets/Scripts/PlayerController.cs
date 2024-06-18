using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 15f;
    public float terminalVelocity = 15f;
    public float gravity = 0.5f;
    private float fallingSpeed;

    public float jumpForceTotal = 200f;
    public float jumpForceStart = 50f;

    private Rigidbody rb;
    private bool grounded;

    private bool canDash;
    private bool canDashCooldown;
    public float dashForce; 
    private bool facingRight;

    private bool canWallJump;
    private bool canWallJumpCooldown;
    private String lastWallJump;

    // Start is called before the first frame update
    void Start()
    {   
        fallingSpeed = 0f;
        grounded = false;
        rb = GetComponent<Rigidbody>();
        canDash = true;
        canDashCooldown = false;
        
        canWallJump = false;
        canWallJumpCooldown = true;
        lastWallJump = "None";
    }

    void FixedUpdate()
    {
        Movement();
    }

    private void Movement(){
        CheckMovement();
        CheckJump();
        CheckDash();
        GravityMethod();
    }

    //I think last time each time I updated the mvoement I zeroed the y and z components.
    //So need to keep them the same as their prev values (rb.velocity.y, rb.velocity.z)
    private void CheckMovement(){
        //InputManager has settings about acceleration
        float moveHorizontal = Input.GetAxis("Horizontal");

        //Need to get direction for dashing
        if(moveHorizontal > 0){
            facingRight = true;
        }
        if(moveHorizontal < 0){
            facingRight = false;
        }

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
            StartCoroutine(SmoothJumping(false));
            grounded = false;
        }

        if(Input.GetButton("Jump") && canWallJump){
            StartCoroutine(SmoothJumping(true));
            canWallJump = false;
        }
    }

    private void CheckDash(){
        if((Input.GetAxis("Dash") > 0 || Input.GetButton("DashKeyboard")) && canDash){
            canDash = false;
            canDashCooldown = true;
            if(facingRight){
                rb.AddForce(Vector3.right * dashForce, ForceMode.Impulse);
            }else{
                rb.AddForce(Vector3.left * dashForce, ForceMode.Impulse);
            }
        }
    }


    //Using own method as other gravity options wouldn't accelerate to a terminal velocity.
    private void GravityMethod(){
        if(!grounded && !canWallJump){
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
            lastWallJump = "None";
            if(canDashCooldown && Input.GetAxis("Dash") <= 0 && !Input.GetButton("DashKeyboard")){
                StartCoroutine(DashCooldown());
            }
		}else if(layer.value == 7){
            if(!grounded && canWallJumpCooldown){
                StartCoroutine(WallJumpCooldown());
            }
        }
    }

    private void OnCollisionExit(Collision collision){
        LayerMask layer = collision.gameObject.layer;
        if(layer.value == 6){
            grounded = false;
        }else if(layer.value == 7){
            canWallJump = false;
            canWallJumpCooldown = true;
        }
    }

    //Using Impulse mode made all the force get applied at the same time
    //Made it like a dash
    //So will decrease force applied over time
    private IEnumerator SmoothJumping(bool isWallJump){
        if(isWallJump){
            AdjustForWallJump();
        }

        float currentJumpForce = jumpForceStart;
        //Keep track of how much jumpForce has been applied
        float jumpForceCumul = currentJumpForce;

        while(jumpForceCumul < jumpForceTotal){
            currentJumpForce /= 1.3f;
            jumpForceCumul += currentJumpForce;

            rb.AddForce(Vector3.up * currentJumpForce, ForceMode.VelocityChange);
            //Need to limit horizontal addition to just left + right as otherwise it may add a force going down.
            if(rb.velocity.x < -0.1f){
                //Currently going left
                rb.AddForce(Vector3.left * currentJumpForce * 0.5f, ForceMode.VelocityChange);
            }else if(rb.velocity.x > 0.1f){
                //Currently going right
                rb.AddForce(Vector3.right * currentJumpForce * 0.5f, ForceMode.VelocityChange);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    //Get obj to go away from the wall a bit so it doesn't just get stuck
    //The lastWallJump keeps track of the direction of the last wall jump.
    //If you're facing right, the wall jump will be left, so lastWallJump is left
    //Need to alternate directions if you're chaining wall jumps
    private void AdjustForWallJump(){
        if(facingRight){
            if(lastWallJump != "Left"){
                rb.AddForce(Vector3.left * jumpForceTotal, ForceMode.Impulse);
                rb.AddForce(Vector3.up * jumpForceTotal * 0.5f, ForceMode.Impulse);
                lastWallJump = "Left";
            }
        }else{
            if(lastWallJump != "Right"){
                rb.AddForce(Vector3.right * jumpForceTotal, ForceMode.Impulse);
                rb.AddForce(Vector3.up * jumpForceTotal * 0.5f, ForceMode.Impulse);
                lastWallJump = "Right";
            }
        }
    }

    //Need this canCooldown so this coroutine isn't called many times
    private IEnumerator DashCooldown(){
        canDashCooldown = false;
		yield return new WaitForSeconds(1f);
		canDash = true;
    }

    private IEnumerator WallJumpCooldown(){
        canWallJumpCooldown = false;
        canWallJump = true;
        yield return new WaitForSeconds(1f);
        canWallJump = false;
    }

    
}
