using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBtnCtr : MonoBehaviour
{
    public GameObject obj;
    WorkspaceCtr WorkspaceCtr;

    // Start is called before the first frame update
    void Start()
    {
        this.WorkspaceCtr=GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Selected(){
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
            if(obj==this.WorkspaceCtr.CameraCenter || this.WorkspaceCtr.SelectedObjs.Contains(this.WorkspaceCtr.CameraCenter)){
                this.WorkspaceCtr.SelectObj(new List<GameObject>(){this.obj});
            }else{
                List<GameObject> new_SelectedObjs=new List<GameObject>(this.WorkspaceCtr.SelectedObjs);
                if(!new_SelectedObjs.Contains(obj)) new_SelectedObjs.Add(obj);
                this.WorkspaceCtr.SelectObj(new_SelectedObjs);
            }
        }else{
            this.WorkspaceCtr.SelectObj(new List<GameObject>(){this.obj});
        }
        
    }
}
