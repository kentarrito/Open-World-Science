using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereColliderCtr : MonoBehaviour
{
    public GameObject parent_obj;
    List<string> MoldNamesInsertableToHole=new List<string>(){"Pipe","Pillar"};
    public WorkspaceCtr WorkspaceCtr;

    public Vector3 hole_normal;
    public float hole_depth;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider){
        if(MoldNamesInsertableToHole.Contains(collider.gameObject.GetComponent<MoldInfo>().mold_name)){
            this.WorkspaceCtr.is_moving_obj_in_hole_start=true;
            this.WorkspaceCtr.hole_normal=this.hole_normal;
            this.WorkspaceCtr.hole_posi = this.transform.position;

            this.WorkspaceCtr.HoleObj = parent_obj;
            this.WorkspaceCtr.ObjInHole = collider.gameObject;
        }
    }

    void OnTriggerExit(Collider collider){

    }
}
