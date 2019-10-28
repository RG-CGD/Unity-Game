using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AKM : MonoBehaviour
{
    public static AKM instance;
    RaycastHit hit;
    
    [SerializeField]
    float damageEnemy = 10f;
    [SerializeField]
    float headShotDamage = 100f;

    [SerializeField]
    Transform shootPoint;

    public Text currentAmmoText;
    public Text carriedAmmoText;
   
    bool isReloading;

    //WEAPON EFFECTS
    //Muzzleflash
    public ParticleSystem muzzleflash;
    //Eject bullet casing
    public ParticleSystem bulletCasing;

    //Blood Effect
    public GameObject bloodEffect;
    //Bullet Holes
    public GameObject metalBulletHole;

    [Header ("Audio")]
    AudioSource gunAS;
    public AudioClip shootAC;
    public AudioClip dryFireAC;
    public AudioClip reloadAC;
    public AudioClip headShotAC;
    public AudioClip shootMetalAC;

    //Rate Of Fire
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
    int fovIronSights = 45;
    float smoothZoom = 6f;

    [Header("Layers Affected")]
    public LayerMask layer;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        muzzleflash.Stop();
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
        if (ironSightsOn)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, fovIronSights, smoothZoom * Time.deltaTime);
        }
        else if (!ironSightsOn)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, fovNormal, smoothZoom * Time.deltaTime);
        }

        if (Input.GetButton("Fire1") && GlobalVariables.akmCurrentAmmo > 0 && !ironSightsOn)
        {
            Shoot();
        }
        else if (Input.GetButton("Fire1") && GlobalVariables.akmCurrentAmmo > 0 && ironSightsOn)
        {
            ShootIronSights();
        }

        else if (Input.GetButton("Fire1") && GlobalVariables.akmCurrentAmmo <= 0 && !isReloading)
        {
            DryFire();
        }

        if (Input.GetKeyDown(KeyCode.R) && GlobalVariables.akmCurrentAmmo <= GlobalVariables.akmMaxCurrentAmmo && !isReloading)
        {
            isReloading = true;
            Reload();
        }

    }

    //HIP FIRE
    void Shoot()
    {
        if (Time.time > nextFire)
        {
            nextFire = 0f;

            nextFire = Time.time + rateOfFire;

            StartCoroutine(MuzzleFlash());
            StartCoroutine(EjectCasing());
            gunAS.PlayOneShot(shootAC);
            //anim.SetTrigger("Shoot");

            GlobalVariables.akmCurrentAmmo--;

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
            StartCoroutine(MuzzleFlash());
            StartCoroutine(EjectCasing());
            gunAS.PlayOneShot(shootAC);
            //anim.SetTrigger("IronSightsShoot");

            GlobalVariables.akmCurrentAmmo--;

            ShootRay();

            UpdateAmmoUI();

        }
    }

    void ShootRay()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, weaponRange, layer))
        {

            if (hit.transform.tag == "Enemy")
            {
                EnemyHealth enemyHealthScript = hit.transform.GetComponent<EnemyHealth>();
                enemyHealthScript.DeductHealth(damageEnemy);
                Instantiate(bloodEffect, hit.point, transform.rotation);
            }
            else if (hit.transform.tag == "Head")
            {
                EnemyHealth enemyHealthScript = hit.transform.GetComponentInParent<EnemyHealth>();
                enemyHealthScript.DeductHealth(headShotDamage);
                gunAS.PlayOneShot(headShotAC);
                Instantiate(bloodEffect, hit.point, transform.rotation);
                hit.transform.gameObject.SetActive(false);
            }
            else if (hit.transform.tag == "Metal")
            {
                gunAS.PlayOneShot(shootMetalAC);
                Instantiate(metalBulletHole, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            }
            else
            {
                Debug.Log(hit.transform.name);
            }
        }
    }

    void DryFire()
    {
        if (Time.time > nextFire)
        {
            nextFire = 0f;

            nextFire = Time.time + rateOfFire;

            anim.SetTrigger("DryFire");

            gunAS.PlayOneShot(dryFireAC);

        }
    }

    void Reload()
    {
        if (GlobalVariables.akmCarriedAmmo <= 0) return;
        anim.SetTrigger("Reload");
        gunAS.PlayOneShot(reloadAC);
        StartCoroutine(ReloadCountdown(2f));
    }

    public void UpdateAmmoUI()
    {
        currentAmmoText.text = GlobalVariables.akmCurrentAmmo.ToString();
        carriedAmmoText.text = GlobalVariables.akmCarriedAmmo.ToString();
    }

    IEnumerator ReloadCountdown(float timer)
    {
        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            yield return null;
        }
        if (timer <= 0f)
        {
            int bulletsNeededToFillMag = GlobalVariables.akmMaxCurrentAmmo - GlobalVariables.akmCurrentAmmo;
            int bulletsToDeduct = (GlobalVariables.akmCarriedAmmo >= bulletsNeededToFillMag) ? bulletsNeededToFillMag : GlobalVariables.akmCarriedAmmo;

            GlobalVariables.akmCarriedAmmo -= bulletsToDeduct;
            GlobalVariables.akmCurrentAmmo += bulletsToDeduct;
            isReloading = false;
            UpdateAmmoUI();
        }
    }

    IEnumerator MuzzleFlash()
    {
        muzzleflash.Play();        
        yield return new WaitForEndOfFrame();
        muzzleflash.Stop();
    }

    IEnumerator EjectCasing()
    {
        bulletCasing.Play();
        yield return new WaitForEndOfFrame();
        bulletCasing.Stop();
    }
}
