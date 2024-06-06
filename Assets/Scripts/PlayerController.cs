using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    public float attackMoveDistance = 0.5f;
    public float attackMoveDelay = 0.1f;
    public float attackMoveDuration = 0.5f;
    public float dodgeDistance = 3.0f;
    public float dodgeDuration = 0.5f;
    public float dodgeCooldown = 5.0f;
    public int attackDamage = 1;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int maxHealth = 100;
    private int currentHealth;
    public float contactDamageCooldown = 1f; // 적과 접촉 시 데미지를 입히는 간격

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = true;
    private bool isFacingRight = true;
    private bool isAttacking = false;
    private bool isDodging = false;
    private bool isInvincible = false;
    private float lastDodgeTime;
    private float lastContactDamageTime; // 마지막으로 접촉 데미지를 입힌 시간

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer; // Ground 레이어 설정

    [SerializeField] private BossInterfaceManager bossInterfaceManager;
    public int bossMaxHealth = 100; // 보스 최대 체력
    private int bossCurrentHealth; // 보스 현재 체력

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (attackPoint == null)
        {
            Debug.LogError("Attack Point is not assigned in the Inspector");
        }
        else
        {
            Debug.Log("Attack Point assigned successfully. Position: " + attackPoint.position);
        }

        if (groundCheck == null)
        {
            Debug.LogError("Ground Check is not assigned in the Inspector");
        }
    }

    void Update()
    {
        CheckGrounded();

        if (!isAttacking && !isDodging)
        {
            HandleMovement();
            HandleJump();
        }

        HandleAttack();
        HandleDodge();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        Vector2 movement = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        rb.velocity = new Vector2(movement.x, rb.velocity.y);

        bool isMoving = horizontal != 0;
        animator.SetBool("isMoving", isMoving);

        if (horizontal > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontal < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            animator.SetBool("isJumping", true);
            animator.SetBool("isGrounded", false);
        }
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isAttacking && isGrounded)
        {
            isAttacking = true;
            animator.SetTrigger("isAttacking");
            animator.SetBool("isGrounded", true);

            if (attackPoint == null)
            {
                Debug.LogError("Attack Point is not assigned.");
                return;
            }

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, 1 << enemyLayer);
            foreach (Collider2D enemy in hitEnemies)
            {
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                    enemyComponent.TakeDamage(attackDamage, knockbackDirection);
                }
            }

            StartCoroutine(AttackMoveCoroutine());
        }
    }

    IEnumerator AttackMoveCoroutine()
    {
        yield return new WaitForSeconds(attackMoveDelay);

        float elapsedTime = 0f;
        while (elapsedTime < attackMoveDuration)
        {
            float moveStep = (isFacingRight ? attackMoveDistance : -attackMoveDistance) * Time.deltaTime / attackMoveDuration;
            rb.MovePosition(rb.position + new Vector2(moveStep, 0));
            elapsedTime += 0.8f;
            yield return null;
        }

        isAttacking = false;
    }

    void HandleDodge()
    {
        if (Input.GetKeyDown(KeyCode.X) && isGrounded && !isDodging && Time.time - lastDodgeTime > dodgeCooldown)
        {
            isDodging = true;
            lastDodgeTime = Time.time;
            animator.SetTrigger("isDodging");

            StartCoroutine(DodgeMoveCoroutine());
            StartCoroutine(DodgeInvincibilityCoroutine());
        }
    }

    IEnumerator DodgeMoveCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < dodgeDuration)
        {
            float moveStep = (isFacingRight ? dodgeDistance : -dodgeDistance) * Time.deltaTime / dodgeDuration;
            rb.MovePosition(rb.position + new Vector2(moveStep, 0));
            elapsedTime += 0.8f;
                
            yield return null;
        }

        isDodging = false;
    }

    IEnumerator DodgeInvincibilityCoroutine()
    {
        float invincibilityTime = 2f; // 무적 유지 시간
        float blinkInterval = 0.2f; // 깜빡임 간격
        float elapsedTime = 0f;
        isInvincible = true;

        while (elapsedTime < invincibilityTime)
        {
            elapsedTime += 0.8f;

            // 깜빡이는 효과
            spriteRenderer.enabled = !spriteRenderer.enabled;

            // 무적 상태에서도 방향 전환을 처리
            float horizontal = Input.GetAxis("Horizontal");
            if (horizontal > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontal < 0 && isFacingRight)
            {
                Flip();
            }

            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true; // 깜빡임 종료 후 스프라이트 다시 보이게 설정
        isInvincible = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isAttacking && collision.gameObject.CompareTag("Enemy"))
        {
            if (Time.time >= lastContactDamageTime + contactDamageCooldown)
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    if (!isInvincible)
                    {
                        TakeDamage(enemy.damage);
                        lastContactDamageTime = Time.time;
                    }
                }
            }
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        if (groundCheck == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currentHealth -= damage;
            Debug.Log("Player took damage. Current health: " + currentHealth);
            if (currentHealth <= 0)
            {
                Debug.Log("Player died");
                // 게임 오버 처리
            }
            else
            {
                StartCoroutine(InvincibilityAfterDamageCoroutine());
            }
        }
    }

    IEnumerator InvincibilityAfterDamageCoroutine()
    {
        isInvincible = true;
        float invincibilityTime = 1f; // 무적 유지 시간
        float blinkInterval = 0.2f; // 깜빡임 간격
        float elapsedTime = 0f;

        while (elapsedTime < invincibilityTime)
        {
            elapsedTime += 0.8f;

            // 깜빡이는 효과
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);
        }

        spriteRenderer.enabled = true; // 깜빡임 종료 후 스프라이트 다시 보이게 설정
        isInvincible = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Warning"))
        {
            Debug.Log("asdsad");
            FindObjectOfType<BossInterfaceManager>().OpenBossInterface();
        }
    }
}
