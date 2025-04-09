using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldingSearchSphereCtr : MonoBehaviour
{
    public List<GameObject> ObjsOnTrigger = new List<GameObject>();
    //WorkspaceCtr WorkspaceCtr;
    // Start is called before the first frame update
    void Start()
    {
        //WorkspaceCtr=GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        ObjsOnTrigger.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other){
        ObjsOnTrigger.Remove(other.gameObject);
    }
}
