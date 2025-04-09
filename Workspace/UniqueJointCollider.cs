using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueJointCollider : MonoBehaviour
{
    public UniqueJointCtr UniqueJointCtr;   //should be put
	WorkspaceCtr WorkspaceCtr;

	public void OnTriggerEnter(Collider col){
        GameObject g = col.gameObject;
        while(g.tag != "Item"){
            g = g.transform.parent.gameObject;
        }
        
        this.UniqueJointCtr.TriggerItem(g.name, g.GetComponent<ItemInfo>().item_type);
	}

    // Start is called before the first frame update
    void Start()
    {
        WorkspaceCtr = GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
    }
}
