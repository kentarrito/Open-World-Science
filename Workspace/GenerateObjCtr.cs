using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//Warning: このスクリプトはMoldBtnPfbにアタッチする

public class GenerateObjCtr : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    WorkspaceCtr WorkspaceCtr;
    //bool is_obj_putted=false;
    bool is_mold_selected;
    bool is_obj_generated=false;
    GameObject GeneratingObj;
    Vector3 generating_point;
    float hit_to_camera;

    bool is_update_count=false;
    int update_count=0;
    float coe_generating=0.01f;

    // Start is called before the first frame update
    void Start()
    {
        this.WorkspaceCtr=GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.is_update_count){
            this.update_count+=1;
        }

        /*
        if(this.is_obj_putted){
            if(this.update_count>1){
                Debug.Log(this.WorkspaceCtr.num_trigger_obj);
                if(this.WorkspaceCtr.num_trigger_obj!=0){
                    this.GeneratingObj.transform.position-=this.ray.direction*CameraCtr.cam_len*this.coe_generating;
                }else{
                    this.WorkspaceCtr.SetPhysics(this.GeneratingObj,true,false,true);
                    this.WorkspaceCtr.SetObjListPanel(this.WorkspaceCtr.objects,this.GeneratingObj);
                    this.count_off();
                    this.is_obj_putted=false;
                }
            }
        }
        */
    }

    public void OnPointerDown(PointerEventData data){
        this.is_mold_selected=true;
        transform.parent.GetChild(0).gameObject.SetActive(true);
        
    }

    public void OnPointerUp(PointerEventData data){
        this.is_mold_selected=false;
        transform.parent.GetChild(0).gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData data){
        if(this.is_mold_selected){
            if(CameraCtr.is_pointer_on_camera_window){
                if(!is_obj_generated){  // when obj is generated
                    this.GeneratingObj = Instantiate(vars.mold_pfb_dict[transform.parent.gameObject.name], this.WorkspaceCtr.objects.transform);
                    this.GeneratingObj.name = SetName(this.GeneratingObj.GetComponent<ItemInfo>().item_type);
                    WorkspaceCtr.item_dict.Add(this.GeneratingObj.name, this.GeneratingObj);
                    this.WorkspaceCtr.SetPhysics(this.GeneratingObj,false,false,true);
                    is_obj_generated=true;
                    generating_point=this.GeneratingObj.transform.position;
                    hit_to_camera = (generating_point - Camera.main.transform.position).magnitude;

                    this.WorkspaceCtr.SelectObj(new List<GameObject>(){this.GeneratingObj});  //SelectOffばこれがやってくれる.
                }

                this.WorkspaceCtr.MoveObj(new List<GameObject>(){this.GeneratingObj}, out generating_point, generating_point, hit_to_camera);
            }
        }
    }

    string SetName(string item_type){
        string name = item_type + "-1";
        int i = 1;
        while(WorkspaceCtr.item_dict.ContainsKey(name)){
            i += 1;
            name = item_type + "-" + i;
        }
        
        return name;
    }

    public void OnEndDrag(PointerEventData data){
        //this.WorkspaceCtr.SelectOff();
        //this.WorkspaceCtr.SelectObj(this.GeneratingObj);
        //CameraCtr.OutlineOff();
        //CameraCtr.OutlineObj(new List<GameObject>(){this.GeneratingObj});
        this.is_obj_generated=false;
        this.is_mold_selected=false;
        transform.parent.GetChild(0).gameObject.SetActive(false);
    }

    void count_on(){
        this.update_count=0;
        this.is_update_count=true;
    }

    void count_off(){
        this.update_count=0;
        this.is_update_count=false;
    }
}
