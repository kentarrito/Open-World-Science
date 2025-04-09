using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GenerateECECtr : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    ECsml ECsml;
    public string ece_type;
    // Start is called before the first frame update
    void Start()
    {
        this.ECsml=GameObject.Find("Controller").GetComponent<ECsml>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData data){
        this.ECsml.SelectGeneratingECE(this.ece_type);
        transform.parent.GetChild(0).gameObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData data){
        this.ECsml.SelectOffGeneratingECE();
        transform.parent.GetChild(0).gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData data){
        this.ECsml.PointerDragOnECPanel2(data.position);
        /*
        if(this.WorkspaceCtr.is_ece_selected){
            if(CameraCtr.is_pointer_on_camera_window){
                //ここで生成のアイコンを表示させる
            }
        }
        */
    }

    public void OnEndDrag(PointerEventData data){
        this.ECsml.DragEndOnECPanel2(data.position);
        transform.parent.GetChild(0).gameObject.SetActive(false);
    }
}