using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyArcher : MonoBehaviour
{
    // Editor Properties
    [Header("Enemy")]
    public Transform GroundChecker;
    public LayerMask GroundLayer;
    public float WalkSpeed = 30;
    public float JumpPower = 100;
    public int MaxHealth = 100;
    public int currentHealth;

    [Header("Weapon")]
    public Transform LaunchPoint;
    public WeaponProjectile EnemyProjectile;
    public Transform EffectPoint;
    public WeaponEffect Effect;

    // Script Properties
    public int EnemyHealth { get; private set; }
    public bool IsDead { get { return this.EnemyHealth <= 0; } }

    // Members
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
        this.EnemyHealth = this.MaxHealth;
        this.ApplyEnemyDamage(0);
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

        
    }
    /// <summary>
    /// Reduce the health of the character by the specified amount
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>True if the character dies from this damage, False if it remains alive</returns>
    public void ApplyEnemyDamage(int damage)
    {
        if (!this.IsDead)
        {
            this.EnemyHealth -= damage;
            currentHealth = MaxHealth - damage;
        }
        if (damage >= 1 && damage <= 10)
        {
            // Show the hurt animation
            TriggerAction("TriggerHurt");
        }
        if(damage >= 20)
        {
            StartCoroutine(this.Wait(0.5f));
            TriggerAction("TriggerHurt");
        }

        if (this.EnemyHealth <= 0)
        {
            gameObject.layer = LayerMask.NameToLayer("other");
            // Since the player is dead, remove the corpse
            animatorObject.SetBool("Dies", true);
            StartCoroutine(this.DestroyAfter(2));
        }
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

    }

    private IEnumerator DestroyAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        
        GameObject.Destroy(this.gameObject);
    }
    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        TriggerAction("TriggerHurt");

    }


    public void ApplyDamage(int damage)
    {
        if(EnemyHealth > 0)
        {
            this.ApplyEnemyDamage(damage);
        }
        
    }
   /* public void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.name == "player")
        {
            this.gameObject.tag == "Other";
        }
    }*/

}
