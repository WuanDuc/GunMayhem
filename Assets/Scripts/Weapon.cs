using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponFireType
{
    SINGLE,
    MUTILPLE
}

public class Weapon : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private GameObject muzzleFlash;
    public WeaponFireType fireType;

    public float fireRate = 5f;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ShootAnimation()
    {
        animator.SetTrigger("Shoot");
    }    

}
