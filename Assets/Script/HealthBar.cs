using System;
using UnityEngine;
public class HealthBar3D : MonoBehaviour {
    public Transform foreground;
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    void Start() {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage) {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();
    }

    void UpdateHealthBar() {
        float ratio = currentHealth / maxHealth;
        foreground.localScale = new Vector3(ratio, 1, 1);
    }

    public void Update()
    {
        UpdateHealthBar();
    }
}
