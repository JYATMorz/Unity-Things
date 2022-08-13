using UnityEngine;
using System;

public class ResetCharacter : MonoBehaviour
{
    private readonly string[] _characterTags = { "Neutral", "BlueTeam", "RedTeam" };
    private const string _bulletTag = "Bullet";
    private const float _freeHeightY = 23.5f;
    private const float _outOfBoundX = 49.5f;

    public GameMenu gameMenu;

    void OnTriggerEnter(Collider other)
    {
        GameObject contact = other.gameObject;

        if (Array.Exists(_characterTags, tag => contact.CompareTag(tag)))
        {
            if (Mathf.Abs(contact.transform.position.x) > _outOfBoundX)
            {
                // In case character die in accident
                gameMenu.CharacterDie(contact.tag);
                Destroy(contact);
            }
            else contact.transform.position = new Vector3(contact.transform.position.x, _freeHeightY, 0);
        }
        else Destroy(contact);
    }
}
