using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipCtr : MonoBehaviour
{
    public Vector3 posi;
    public Vector3 dir_to_fly;
    public Quaternion rot_to_normal;
    float speed_coe=0.02f;
    float rot_speed=70;
    int frame=0;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = posi;
        transform.rotation = Quaternion.FromToRotation(new Vector3(0,1,0), - dir_to_fly) * rot_to_normal;
    }

    // Update is called once per frame
    void Update()
    {
        if(frame<20){
            transform.position -= transform.up * speed_coe;
            //transform.localEulerAngles += new Vector3(0, rot_speed, 0); 
            transform.rotation*=Quaternion.AngleAxis(rot_speed, Vector3.up);  //注：ここのaxisはlocalから見たaxisになるっぽいから常にy-axis
            frame+=1;
        }else{
            Destroy(this.gameObject);
        }
    }
}
