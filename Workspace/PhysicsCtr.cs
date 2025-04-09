using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCtr : MonoBehaviour
{
    //WorkspaceCtr WorkspaceCtr;

    List<GameObject> ObjsInContact=new List<GameObject>();
    MoldInfo MoldInfo;

    public Dictionary<string,double> parameters= new Dictionary<string,double>();

    public string step_eqs;

    public Dictionary<string, string> step_eqs_with_joint = new Dictionary<string,string>();  //key: the name of the obj jointed to this obj, value: eqs
    
    // Start is called before the first frame update
    void Start()
    {
        //this.WorkspaceCtr=GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
        this.MoldInfo = this.gameObject.GetComponent<MoldInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col){
        ObjsInContact.Add(col.collider.gameObject);
    }

    void OnCollisionExit(Collision col){
        ObjsInContact.Remove(col.collider.gameObject);
    }

    /*

    void OnTriggerEnter(Collider collider){  //triggerはオブジェ生成の時だけしか使ってはいけない
        this.WorkspaceCtr.num_trigger_obj+=1;
    }

    void OnTriggerStay(Collider collider){

    }

    void OnTriggerExit(Collider collider){
        this.WorkspaceCtr.num_trigger_obj-=1;

    }
    */
}
