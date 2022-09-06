using UnityEngine;
using System;
using System.Collections;

public class TemplateBullet : MonoBehaviour, IDamage
{
    public GameObject bulletImpact;

    private const int _ammoDamage = 10;

    private Rigidbody _ownerBody;

    void Start()
    {
        _ownerBody = GetComponentsInParent<Rigidbody>()[2];

        StartCoroutine(SelfDestruction());
    }

    void OnCollisionEnter(Collision other)
    {       
        IDamage.DirectAttack(_ownerBody, other.gameObject, _ammoDamage);

        if (GeneralLoadMenu.Instance.IsLoadingScene) return;

        Instantiate(bulletImpact, transform.position, transform.rotation * Quaternion.FromToRotation(Vector3.forward, Vector3.down));

        Destroy(gameObject);
    }

    IEnumerator SelfDestruction()
    {
        yield return new WaitForSeconds(ConstantSettings.ammoSelfDestruction);
        Destroy(gameObject);
    }
}
