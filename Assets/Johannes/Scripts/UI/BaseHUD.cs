using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseHUD : MonoBehaviour
{
    public ControlPC pc;
    public PUN_ControlPC PUNpc;
    public Slider health;
    public Text ammo;

    private void Start()
    {
        health.maxValue = PUNpc.maxHealth; //health.maxValue = pc.maxHealth;
        StartCoroutine(SpawnHealthLerp());
    }

    IEnumerator SpawnHealthLerp()
    {
        print("BaseHUD/ SpawnHealthLerp()");
        float lerpLength = .5f;
        float timer = 0;
        float progress = 0;
        while (progress < 1)
        {
            timer += Time.deltaTime;
            progress = timer / lerpLength;
            health.value = Mathf.Lerp(0, health.maxValue, progress);
            yield return null;
        }
    }

    public void SetHealth(int newHealth)
    {
        print("BaseHUD/ SetHealth() to: " + newHealth);
        health.value = newHealth;
    }

    public void SetAmmoCount(int newAmmoCount)
    {
        if (newAmmoCount > 0)
        {
            if (ammo) ammo.text = newAmmoCount.ToString();
        }
        else
        {
            if (ammo) ammo.text = "0";
        }
    }

    public void OnRespawn()
    {
        Destroy(this.gameObject);
    }
}
