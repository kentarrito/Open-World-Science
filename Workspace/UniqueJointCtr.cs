using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is attached to either of the items involved in this unique joint
public class UniqueJointCtr : MonoBehaviour
{
    WorkspaceCtr WorkspaceCtr;
	Dictionary<string, UniqueJointInfo> UniqueJointInfoDict = new Dictionary<string, UniqueJointInfo>(); //key: item_type  

    public void TriggerItem(string name, string item_type){
        if(UniqueJointInfoDict.ContainsKey(item_type)){
            //Calculation of the state moving obj should be in.
            //For now, it's set to be first element of center_lposi_list, dir_lea_list

            Vector3 center_posi = UniqueJointInfoDict[item_type].center_lposi_list[0] + this.transform.position;

            Vector3[] dir_eA = new Vector3[2];
            dir_eA[0] = UniqueJointInfoDict[item_type].dir_lea_list[0][0] + this.transform.eulerAngles;
            dir_eA[1] = UniqueJointInfoDict[item_type].dir_lea_list[0][1] + this.transform.eulerAngles;

            //parent:this.gameObject, child:WorkspaceCtr.item_dict[name], so center_posi and dir_eA correspond to the position and EulerAngles respectively of child obj
            WorkspaceCtr.TriggerUniqueJointCollider(this.gameObject, WorkspaceCtr.item_dict[name], center_posi, dir_eA);  //dir_eA is the eulerAngles of the direction obj's axis should be oriented to
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.WorkspaceCtr = GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
    }
}
