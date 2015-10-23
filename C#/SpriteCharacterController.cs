using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class SpriteCharacterController : MonoBehaviour
{
    // Editor Properties
    [Header("Character")]
    public Transform GroundChecker;
    public LayerMask GroundLayer;
    public float experience = 0.0f;
    public float level = 0.0f;
    public float WalkSpeed = 30;
    public float JumpPower = 100;
    public int MaxHealth = 100;

    [Header("Weapon")]
    public Transform LaunchPoint;
    public WeaponProjectile Projectile;
    public Transform EffectPoint; 
    public WeaponEffect Effect;

    // Script Properties
    public int CurrentHealth { get; private set; }
    public bool IsDead { get { return this.CurrentHealth <= 0; } }

    // Members
    public GameObject EnemyArcher;
    public double maxDistance = .1938712;
    public float distance;
    public float lnextAttack = 0.0f;
    public float mnextAttack = 0.0f;
    public float snextAttack = 0.0f;
    public float lAttackRate = .85f;
    public float mAttackRate = 3.5f;
    public float sAttackRate = 10.0f;
    private Animator animatorObject;
    private Rigidbody2D body;
    private bool isGrounded = true;
    private float groundRadius = 0.04f;
    private Direction currentDirection = Direction.Right;
    private WeaponEffect activeEffect;

    private enum Direction
    {
        Left,
        Right
    }

    void Start()
    {
        // Grab the editor objects
        this.animatorObject = this.GetComponent<Animator>();
        this.body = this.GetComponent<Rigidbody2D>();

        // Setup the character
        this.CurrentHealth = this.MaxHealth;
        this.ApplyDamage(0);
    }

    void FixedUpdate()
    {

        if (this.animatorObject != null)
        {
            this.isGrounded = Physics2D.OverlapCircle(GroundChecker.position, this.groundRadius, this.GroundLayer);
            this.animatorObject.SetBool("IsGrounded", this.isGrounded);

            if (this.animatorObject.GetCurrentAnimatorStateInfo(0).IsName("Stopped") ||
                this.animatorObject.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                float horizontal = Input.GetAxis("Horizontal");
                this.body.velocity = new Vector2(horizontal * this.WalkSpeed * Time.deltaTime, this.body.velocity.y);
                this.animatorObject.SetFloat("VelocityX", Mathf.Abs(this.body.velocity.x));

                if (!Mathf.Approximately(this.body.velocity.x, 0))
                {
                    this.ChangeDirection(this.body.velocity.x < 0 ? Direction.Left : Direction.Right);
                }
            }
        }
     
    }

    void Update()
    {

        // Check for keyboard input for the different actions
        // Nut only when we are on the ground
        if (this.isGrounded && !this.IsDead)
        {
            levelUp(experience);
            if (Input.GetButtonDown("Jump"))
            {
                this.animatorObject.SetTrigger("TriggerJump");
                this.body.AddForce(new Vector2(0, this.JumpPower));
            }
            else if (Input.GetKeyDown(KeyCode.J) && Time.time > lnextAttack)
            {
                lnextAttack = Time.time + lAttackRate;
                this.TriggerAction("TriggerQuickAttack");
                distance = Vector2.Distance(GameObject.FindWithTag("Enemy").transform.position, GameObject.FindWithTag("Player").transform.position);
                if (distance < .1938712 && currentDirection == Direction.Right)
                {
                    EnemyDamage(true, 10);
                }
            }
            else if (Input.GetKeyDown(KeyCode.K) && Time.time > mnextAttack)
            {
                mnextAttack = Time.time + mAttackRate;
                this.TriggerAction("TriggerAttack");
                distance = Vector2.Distance(GameObject.FindWithTag("Enemy").transform.position, GameObject.FindWithTag("Player").transform.position);
                if (distance < .23 && currentDirection == Direction.Right)
                {
                    EnemyDamage(true, 20);
                }
            }
            else if (Input.GetKeyDown(KeyCode.L) && Time.time > snextAttack)
            {
                snextAttack = Time.time + sAttackRate;
                this.TriggerAction("TriggerCast");
                distance = Vector2.Distance(GameObject.FindWithTag("Enemy").transform.position, GameObject.FindWithTag("Player").transform.position);
                if (distance < .3)
                {
                    EnemyDamage(true, 50);
                }
              
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                
                this.ApplyDamage(10);
            }
        }
    }

    /// <summary>
    /// Reduce the health of the character by the specified amount
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>True if the character dies from this damage, False if it remains alive</returns>

    public bool ApplyDamage(int damage)
    {
        if (!this.IsDead)
        {
            // Update the health
            this.CurrentHealth = Mathf.Clamp(this.CurrentHealth - damage, 0, this.MaxHealth);
            this.animatorObject.SetInteger("Health", this.CurrentHealth);

            if (damage != 0)
            {
                // Show the hurt animation
                this.TriggerAction("TriggerHurt");
            }

            if (this.CurrentHealth <= 0)
            {
                // Since the player is dead, remove the corpse
                StartCoroutine(this.DestroyAfter(2));
            }
        }

        return this.IsDead;
    }

    private void TriggerAction(string action)
    {
        this.animatorObject.SetTrigger(action);

        // Stop the character from moving while we do the animation
        this.body.velocity = new Vector2(0, this.body.velocity.y);
    }

    private void ChangeDirection(Direction newDirection)
    {
        if (this.currentDirection == newDirection)
        {
            return;
        }

        // Swap the direction of the sprites
        Vector3 scale = this.transform.localScale;
        scale.x = -scale.x;
        this.transform.localScale = scale;
        this.currentDirection = newDirection;
    }

    private void OnCastEffect()
    {
        // If we have an effect start it now
        if (this.Effect != null)
        {
            this.activeEffect = WeaponEffect.Create(this.Effect, this.EffectPoint);
        }
    }

    private void OnCastComplete()
    {
        // Stop the active effect once we cast
        if (this.activeEffect != null)
        {
            this.activeEffect.Stop();
        }

        // Create the projectile
        if (this.Projectile != null)
        {
            WeaponProjectile.Create(
                this.Projectile,
                this,
                this.LaunchPoint,
                (this.currentDirection == Direction.Left ? -1 : 1));
        }
    }

    private IEnumerator DestroyAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        GameObject.Destroy(this.gameObject);
    }
    //Apply damage
    public void EnemyDamage(bool ifhit, int damage)
    {
        //EnemyArcher archer = gameObject.GetComponent<EnemyArcher>();
        EnemyArcher archer = GameObject.FindObjectOfType<EnemyArcher>();
        archer.ApplyDamage(damage);
    }
    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);

    }
    public void gainExp(float enemyExperience)
    {
        EnemyArcher archer = GameObject.FindObjectOfType<EnemyArcher>();
        if (archer.IsDead == true)
        {
            this.experience += enemyExperience;
        }
    }
    public void levelUp(float experience)
    {
        float nextLevel = level * 50;
        if(level == 0 && experience >= 20)
        {
            level += 1;
            //nextLevel(true);
        }
        else if(experience >= nextLevel)
        {
            level += 1;
            //nextLevel(true);
        }
    }
    public void nextLevel(bool leveled)
    {
        float statPts = level * 1;
        if(statPts > 0)
        {
            //Level up GUI here
        }
    }
  
    
}
