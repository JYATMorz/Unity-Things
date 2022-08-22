using UnityEngine;
using System;
using System.Collections;

public class TemplateBullet : MonoBehaviour
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
        GameObject contact = other.gameObject;
        if (Array.Exists(ConstantSettings.aliveTags, tag => tag == contact.tag))
        {
            if (!ConstantSettings.AreBothNeutral(contact, _ownerBody))
            {
                contact.GetComponent<HealthControl>().ReceiveDamage(_ammoDamage, _ownerBody);
            }
        }

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
