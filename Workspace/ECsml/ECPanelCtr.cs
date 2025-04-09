using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ECPanelCtr : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    ECsml ECsml;

    // Start is called before the first frame update
    void Start()
    {
        this.ECsml=GameObject.Find("Controller").GetComponent<ECsml>();
    }

    public void OnPointerEnter(PointerEventData data){
    }

    public void OnPointerExit(PointerEventData data){
    }

    public void OnPointerDown(PointerEventData data){
        int[] lc=this.ECsml.GetLC(data.position,this.ECsml.ECEs);
        Debug.Log(lc[0]+","+lc[1]);
    }

    public void OnPointerUp(PointerEventData data){
    }

    public void OnDrag(PointerEventData data){
    }

    public void OnEndDrag(PointerEventData data){
    }
}
