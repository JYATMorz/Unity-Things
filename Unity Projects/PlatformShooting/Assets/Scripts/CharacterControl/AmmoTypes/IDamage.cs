using UnityEngine;
using System;
using System.Collections;

public interface IDamage
{
    protected static void DirectAttack(Rigidbody owner, GameObject contact, int damage)
    {
        if (Array.Exists(ConstantSettings.aliveTags, tag => tag == contact.tag))
        {
            if (!ConstantSettings.AreBothNeutral(contact, owner))
            {
                damage = Mathf.Clamp(damage, 0, 30);

                contact.GetComponent<HealthControl>().ReceiveDamage(damage, owner);
                CausedDamage(damage, owner);
            }
        }
    }

    protected static IEnumerator ExplosionAttack(Rigidbody owner, Vector3 explodePos, float explodeRadius, int damage, float forceScalar = 0f)
    {
        foreach (Collider character in Physics.OverlapSphere(explodePos, explodeRadius, ConstantSettings.characterLayer))
        {
            if (Physics.Linecast(explodePos, character.transform.position, ~ConstantSettings.floorLayer))
            {
                if (!character.CompareTag(ConstantSettings.deadTag) && !ConstantSettings.AreBothNeutral(character, owner))
                {
                    int rangeDamage = ConstantSettings.ExplosionDamage(damage, explodePos, character.transform.position, explodeRadius);
                    rangeDamage = Mathf.Clamp(rangeDamage, 0, 30);

                    character.GetComponent<HealthControl>().ReceiveDamage(rangeDamage, owner);
                    CausedDamage(rangeDamage, owner);
                }
                character.attachedRigidbody.AddExplosionForce(damage, explodePos, explodeRadius, forceScalar, ForceMode.Impulse);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private static void CausedDamage(int damage, Rigidbody owner)
    {
        if (!owner.CompareTag(ConstantSettings.deadTag))
        {
            owner.GetComponent<WeaponControl>().TotalStat.NewDamage(damage);
        }
    }

}
