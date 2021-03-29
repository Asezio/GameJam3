using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator anim;

    private CharacterStats characterStats;
    private Rigidbody rb;
    private GameObject attackTarget;
    private float lastAttackTime;
    public Collider attackPoint;
    private bool isDead;
    public float jumpVelocity;
    public float speed;

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        attackPoint.enabled = false;

    }

    private void Start()
    {
        GameManager.Instance.RigisterPlayer(characterStats);
    }

    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if (!isDead)
        {
            lastAttackTime -= Time.deltaTime;

            if (IsGrounded())
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
                    anim.SetTrigger("Jump");
                    //SoundManager.instance.PlaySound("Jump");
                }

                Vector3 movement;
                // float h = Input.GetAxisRaw("Horizontal")* speed * Time.deltaTime;
                // float v = Input.GetAxisRaw("Vertical")* speed * Time.deltaTime;
                 
                
                float horizontalInput = Input.GetAxis("Horizontal") * speed;
                float verticalInput = Input.GetAxis("Vertical") * speed;
                
                movement=new Vector3(horizontalInput, 0f, verticalInput)*Time.deltaTime;
                rb.MovePosition(transform.position+movement);
                //We keep our y-velocity the same (if we set it to 0, we can't fall/jump anymore);
                Vector3 input = new Vector3(horizontalInput, rb.velocity.y, verticalInput);
                //We rotate the input based on the rotation of our unit.
                //This way, when the character is rotated, he moves in his own directions
                Vector3 rotatedInput = transform.TransformVector(input);
                rb.velocity = input;
                anim.SetFloat("Speed", rb.velocity.magnitude);
            }
        }
        else
            GameManager.Instance.NotifyObservers();
        
       
    } 
    

    //Animator Event
    void Hit()
    {
        var targetStats = attackTarget.GetComponent<CharacterStats>();

        targetStats.TakeDamage(characterStats, targetStats);
        anim.SetTrigger("Hit");
    }
    
    protected bool IsGrounded()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit, 0.5f))
        {
            if(hit.collider.tag == "Ground")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    
}
