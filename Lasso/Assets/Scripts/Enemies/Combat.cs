using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{

    protected AttackDetails attackDetails;
    public void Damage(AttackDetails attackDetails)
    {
        Debug.Log(attackDetails.damageAmount + " Damaged!");
    }
}
