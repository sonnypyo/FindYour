using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManger : MonoBehaviour
{
    private bool Iskey = false;
    public GameObject closedDoor; // 닫힌 문 오브젝트
    public GameObject openDoor; // 열린 문 오브젝트
    public GameObject boss; // 보스 오브젝트
    public GameObject KeyBox;
    void OnCollisionEnter2D(Collision2D other) 
    {
        if(other.gameObject.tag == "Drink")
        {
            Debug.Log("HP+");
            Destroy(other.gameObject);
            
        }

        if(other.gameObject.tag == "Box")
        {
            Iskey=true;
            Debug.Log("Get Key");
            KeyBox.SetActive(true);
            Destroy(other.gameObject);
            
        }

        if(other.gameObject.tag == "Door" && Iskey == true)
        {
            Debug.Log("Door Open");
            if (boss == null && closedDoor.activeInHierarchy && Iskey == true)
        {
            closedDoor.SetActive(false); // 닫힌 문 비활성화
            openDoor.SetActive(true); // 열린 문 활성화
        }
        }
    } 
    void Update()
    {
        
    }
}
