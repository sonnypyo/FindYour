using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatManager : MonoBehaviour
{
    public GameObject[] enemies; // 주위의 적들을 담는 배열
    public float force = 10f; // 주인공이 날아가는 힘

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 게임 오브젝트의 태그를 확인
        if (collision.gameObject.CompareTag("Attack"))
        {
            Debug.Log("Enemy와 충돌!");
            ActivateEnemies();
            FlyAway(collision.transform.position);
        }
    }

    // 주위의 적들을 활성화하는 함수
    private void ActivateEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(true);
        }
    }

    // 주인공이 날아가는 함수
    private void FlyAway(Vector2 collisionPosition)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 forceDirection = (transform.position - (Vector3)collisionPosition).normalized;
            rb.AddForce(forceDirection * force, ForceMode2D.Impulse);
        }
    }
}
