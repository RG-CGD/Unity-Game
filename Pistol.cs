using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pistol : MonoBehaviour
{
    RaycastHit hit;

    //Used to damage enemy
    [SerializeField]
    float damageEnemy = 10f;

    [SerializeField]
    Transform shootPoint;

    public Text currentAmmoText;
    public Text carriedAmmoText;
    public int currentAmmo = 12;
    public int maxAmmo = 12;
    public int carriedAmmo = 100;
    bool isReloading;


    //WEAPON EFFECTS
    //MuzzleFlash
    public ParticleSystem muzzleFlash;
    //Eject bullet casing
    public ParticleSystem bulletCasing;

    //Blood Effect
    public GameObject bloodEffect;

    //Gun Audio
    AudioSource gunAS;
    public AudioClip shootAC;
    public AudioClip dryFireAC;

    //Rate of fire
    [SerializeField]
    float rateOfFire;
    float nextFire = 0;

    [SerializeField]
    float weaponRange;

    Animator anim;

    [Header("Iron Sights")]
    public bool ironSightsOn = false;
    public GameObject crosshair;
    Camera mainCam;
    int fovNormal = 60;
    int fovIronSights = 30;
    float smoothZoom = 6f;

    void Start()
    {
        muzzleFlash.Stop();
        bulletCasing.Stop();
        gunAS = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        UpdateAmmoUI();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            ironSightsOn = true;
            crosshair.SetActive(false);
            anim.SetBool("IronSightsOn", true);
            
        }

        else if (Input.GetButtonUp("Fire2"))
        {
            ironSightsOn = false;
            crosshair.SetActive(true);
            anim.SetBool("IronSightsOn", false);
            
        }

        if(ironSightsOn)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, fovIronSights, smoothZoom * Time.deltaTime);
        }

        else if(!ironSightsOn)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, fovNormal, smoothZoom * Time.deltaTime);
        }

        if (Input.GetButton("Fire1")&& currentAmmo >0 && !ironSightsOn)
        {
            Shoot();
        }

        else if (Input.GetButton("Fire1") && currentAmmo > 0 && ironSightsOn)
        {
            ShootIronSights();
        }

        else if (Input.GetButton("Fire1") && currentAmmo <= 0)
        {
            DryFire();
        }

        else if (Input.GetKeyDown(KeyCode.R) && currentAmmo <= maxAmmo)
        {
            
            Reload();
        }
    }
    //HIP FIRE
    void Shoot()
    {
        if(Time.time > nextFire)
        {
            nextFire = 0f;


            nextFire = Time.time + rateOfFire;

            anim.SetTrigger("Shoot");

            currentAmmo--;

            ShootRay();

            UpdateAmmoUI();
        }
    }
    //IRON SIGHTS
    void ShootIronSights()
    {
        if (Time.time > nextFire)
        {
            nextFire = 0f;


            nextFire = Time.time + rateOfFire;

            anim.SetTrigger("IronSightsShoot");

            currentAmmo--;

            ShootRay();

            UpdateAmmoUI();
        }
    }

    void ShootRay()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, weaponRange))
        {
            if (hit.transform.tag == "Enemy")
            {
                //Debug.Log("Hit Enemy");
                EnemyHealth enemyHealthScript = hit.transform.GetComponent<EnemyHealth>();
                enemyHealthScript.DeductHealth(damageEnemy);
                Instantiate(bloodEffect, hit.point, transform.rotation);
            }
            else
            {
                Debug.Log(hit.transform.name);
            }
        }
    }

    void DryFire()
    {
        if(!isReloading)
        {
            if (Time.time > nextFire)
            {
                nextFire = 0f;

                nextFire = Time.time + rateOfFire;

                anim.SetTrigger("DryFire");

                gunAS.PlayOneShot(dryFireAC);
            }
        }
    }

    void Reload()
    {
        if(!isReloading)
        {
            anim.SetTrigger("Reload");
            if (carriedAmmo <= 0) return;
            StartCoroutine(ReloadCountdown(1.6f));
        }
    }

    void UpdateAmmoUI()
    {
        currentAmmoText.text = currentAmmo.ToString();
        carriedAmmoText.text = carriedAmmo.ToString();
    }

    IEnumerator ReloadCountdown(float timer)
    {
        while(timer > 0f)
        {
            isReloading = true;
            timer -= Time.deltaTime;

            yield return null;
        }
        if(timer <= 0f)
        {
            isReloading = false;
            int bulletsNeededToFillMag = maxAmmo - currentAmmo;
            int bulletsToDeduct = (carriedAmmo >= bulletsNeededToFillMag) ? bulletsNeededToFillMag : carriedAmmo;

            carriedAmmo -= bulletsToDeduct;
            currentAmmo += bulletsToDeduct;
            UpdateAmmoUI();
        }
    }
    //CALLED THROUGH ANIMATION
    IEnumerator PistolSoundAndMuzzleFlash()
    {
        muzzleFlash.Play();
        gunAS.PlayOneShot(shootAC);
        yield return new WaitForEndOfFrame();
        muzzleFlash.Stop();
    }
    //CALLED THROUGH ANIMATION
    IEnumerator EjectCasing()
    {
        bulletCasing.Play();
        yield return new WaitForEndOfFrame();
        bulletCasing.Stop();
    }
}
