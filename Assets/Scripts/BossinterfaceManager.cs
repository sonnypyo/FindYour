using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




    public class BossInterfaceManager : MonoBehaviour
    {
        [SerializeField] GameObject bossPanel;
        [SerializeField] Image bossHealthBar;

        void Awake()
        {
            bossPanel.gameObject.SetActive(false);
        }

        public void SetBossHealth(int HP, int bossMaxHP)
        {
            bossHealthBar.fillAmount = (float)HP/ bossMaxHP;
        }

        public void OpenBossInterface()
        {
            bossPanel.gameObject.SetActive(true);
        }

}

