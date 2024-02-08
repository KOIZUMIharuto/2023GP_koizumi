using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ray_cast : MonoBehaviour
{
	private GameObject lastHitObject;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null)
            {
                lastHitObject = hit.collider.gameObject;
            }
        }
    }

	public GameObject GetLastHitObject()
    {
        return lastHitObject;
    }
}
