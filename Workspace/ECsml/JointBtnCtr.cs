using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JointBtnCtr : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int[] lc;
    //public ecc ecc;
    //public int[] clc;

    ECsml ECsml;
    bool is_grabbed;

    int[] last_lc;

    // Start is called before the first frame update
    void Start()
    {
        this.ECsml=GameObject.Find("Controller").GetComponent<ECsml>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData data){
        GetComponent<Image>().color=new Color32(255,0,0,255);
    }

    public void OnPointerExit(PointerEventData data){
        if(is_grabbed==false) GetComponent<Image>().color=new Color32(0,0,0,255);
    }

    public void OnPointerDown(PointerEventData data){
        Debug.Log(data.position.x+","+data.position.y);
        Debug.Log(this.gameObject.transform.position.x+","+this.gameObject.transform.position.y);
        this.is_grabbed=true;
        this.ECsml.JointGrabbed(this.lc,this.gameObject);
        last_lc=this.ECsml.GetLC(data.position,this.ECsml.ECEs);
    }

    public void OnPointerUp(PointerEventData data){
        this.is_grabbed=false;
    }

    public void OnDrag(PointerEventData data){
        int[] lc=this.ECsml.GetLC(data.position,this.ECsml.ECEs);
        if(lc[0]!=last_lc[0] || lc[1]!=last_lc[1]){
            this.ECsml.MoveJoint(data.position,lc,this.gameObject);
        }
    }

    public void OnEndDrag(PointerEventData data){
        //int[] lc=this.ECsml.GetLC(data.position,this.ECsml.ECEs);
        is_grabbed=false;
        GetComponent<Image>().color=new Color32(0,0,0,255);
        this.ECsml.MoveEndJoint(this.gameObject);
    }
}
