using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestraintBtnCtr : MonoBehaviour
{
    WorkspaceCtr WorkspaceCtr;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BtnClick(){
        GetComponent<Image>().color=new Color32(255,156,0,255);
    }
}
