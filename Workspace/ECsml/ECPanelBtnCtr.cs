using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.EventSystems;

public class ECPanelBtnCtr : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    ECsml ECsml;
    public ecc ecc;
    public List<GameObject> JointBtn;

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
        this.ECsml.is_ece_grabbed=true;
        this.ECsml.lc_now=this.ECsml.GetLC(data.position,this.ECsml.ECEs);
    }

    public void OnPointerUp(PointerEventData data){
        this.ECsml.is_ece_grabbed=false;
    }

    public void OnDrag(PointerEventData data){
        int[] lc=this.ECsml.GetLC(data.position,this.ECsml.ECEs);
        //this.ECsml.MoveECE(this.ece,lc);
    }

    public void OnEndDrag(PointerEventData data){

    }

    public void Joint0(){

    }
}
