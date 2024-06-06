using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int health = 10;
    public float speed = 2.0f;
    public float attackRange = 1.0f;
    public int damage = 10;
    public float attackCooldown = 2.0f; // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½Å¸ï¿½ï¿½ï¿½ï¿½ 2ï¿½Ê·ï¿½ ï¿½ï¿½ï¿½ï¿½
    public float detectionRange = 5.0f;

    public Transform attackPoint; // °ø°Ý ¹üÀ§ÀÇ Áß½ÉÁ¡
    public float attackRadius = 0.5f; // °ø°Ý ¹üÀ§ÀÇ ¹ÝÁö¸§
    public float contactDamageCooldown = 1f; // Àû°ú Á¢ÃË ½Ã µ¥¹ÌÁö¸¦ ÀÔÈ÷´Â °£°Ý
    public float knockbackForce = 5.0f; // ³Ë¹é Èû

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private float lastAttackTime;
    private bool isAttacking = false; // ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½Î¸ï¿½ È®ï¿½ï¿½ï¿½Ï´ï¿½ ï¿½ï¿½ï¿½ï¿½
    private float lastContactDamageTime; // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½Ã°ï¿½
    private bool isFacingRight = true; // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Ù¶óº¸°ï¿½ ï¿½Ö´ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½Î¸ï¿½ È®ï¿½ï¿½ï¿½Ï´ï¿½ ï¿½ï¿½ï¿½ï¿½
    private BoxCollider2D boxCollider;
    public int maxHealth = 10; // ìµœë? ì²´ë ¥
    private BossInterfaceManager bossInterfaceManager;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (attackPoint == null)
        {
            Debug.LogError("Attack Point is not assigned in the Inspector");
        }

        // ï¿½Ê±ï¿½ centerOfMass ï¿½ï¿½ï¿½ï¿½
        Vector2 colliderCenter = boxCollider.bounds.center;
        Vector2 localColliderCenter = transform.InverseTransformPoint(colliderCenter);
        rb.centerOfMass = localColliderCenter;
        bossInterfaceManager = FindObjectOfType<BossInterfaceManager>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < attackRadius)
        {
            if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackPlayer());
                lastAttackTime = Time.time;
            }
        }
        else if (distanceToPlayer < detectionRange)
        {
            FollowPlayer();
        }
        else
        {
            StopFollowingPlayer();
        }
    }

    void FollowPlayer()
    {
        if (isAttacking) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);
        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);

        // ï¿½Ìµï¿½ ï¿½ï¿½ï¿½â¿¡ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Å´
        if ((direction.x > 0 && isFacingRight) || (direction.x < 0 && !isFacingRight))
        {
            Flip(direction.x);
        }
    }

    void StopFollowingPlayer()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero; // ï¿½ï¿½ï¿½ï¿½ ï¿½ß¿ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ ï¿½Êµï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        animator.SetBool("isAttacking", true);
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(0.5f); // ï¿½ï¿½ï¿½ï¿½ ï¿½Ö´Ï¸ï¿½ï¿½Ì¼ï¿½ ï¿½ï¿½ï¿½Û±ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿?

        // ï¿½Ã·ï¿½ï¿½Ì¾î¸¦ ï¿½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerController playerController = playerCollider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }


        yield return new WaitForSeconds(0.2f); // °ø°Ý ÈÄ 0.2ÃÊ µ¿¾È ´ë±â
        //isAttacking trueÀÏ ¶§, FlipµÇÁö ¾Ê°Ô ÇÏ±â À§ÇÔÀÓ.

        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    void Flip(float directionX)
    {
        isFacingRight = !isFacingRight;

        // Rigidbody2Dï¿½ï¿½ ï¿½ß½ï¿½ï¿½ï¿½ ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Å´
        Vector2 colliderCenter = boxCollider.bounds.center;
        Vector2 localColliderCenter = transform.InverseTransformPoint(colliderCenter);
        rb.centerOfMass = localColliderCenter;

        // ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½ï¿½Æ® ï¿½ï¿½ï¿½ï¿½
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        // Collider ¹ÝÀü (BoxCollider2D »ç¿ë½Ã ÇÊ¿äÇÒ ¼ö ÀÖÀ½)
        boxCollider.offset = new Vector2(boxCollider.offset.x * -1, boxCollider.offset.y);

        // ½ºÇÁ¶óÀÌÆ® ¹ÝÀü ÈÄÀÇ À§Ä¡ Á¶Á¤
        Vector2 newPosition = transform.position;
        newPosition.x += directionX * 0.1f; // ÀÌµ¿ ¹æÇâ¿¡ ¸ÂÃç Á¶±Ý ÀÌµ¿
        transform.position = newPosition;
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection)
    {
        health -= damage;
        Debug.Log("Enemy took damage. Current health: " + health);
        

        // ³Ë¹é Àû¿ë
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (health <= 0)
        {
            Die();
        }
         UpdateHealthUI();
    }
    void UpdateHealthUI()
    {
        if (bossInterfaceManager != null)
        {
            bossInterfaceManager.SetBossHealth(health, maxHealth);
        }
    }

    void Die()
    {
        Debug.Log("Enemy died");
        animator.SetBool("isDead", true);
        rb.velocity = Vector2.zero;
        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastContactDamageTime + contactDamageCooldown)
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damage);
                    lastContactDamageTime = Time.time;
                }
            }
        }
    }
}
