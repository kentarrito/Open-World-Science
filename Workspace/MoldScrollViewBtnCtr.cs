using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoldScrollViewBtnCtr : MonoBehaviour
{
    WorkspaceCtr WorkspaceCtr;


    void Start()
    {
        this.WorkspaceCtr=GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select(){
        //this.WorkspaceCtr.SelectedMoldPfb=this.MoldPfb;
        this.WorkspaceCtr.SelectMold(this.name);
    }
}
