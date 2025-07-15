using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSelection : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("Clicked on object: " + gameObject.name);
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    GameManager.Instance.SelectItem(gameObject);
                }
            }
        }
    }
}
