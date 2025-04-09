using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ECPanel2Ctr : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    ECsml ECsml;
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
        this.ECsml.PointerEnterECPanel2(data.position);
    }

    public void OnPointerExit(PointerEventData data){
        this.ECsml.PointerExitECPanel2();
    }
}