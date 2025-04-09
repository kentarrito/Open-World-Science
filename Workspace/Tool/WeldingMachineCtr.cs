using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldingMachineCtr : MonoBehaviour
{
    WorkspaceCtr WorkspaceCtr;
    ParticleSystem Spark;
    int num_anim=0;
    int frame=0;

    bool is_holding=false;
    bool is_holdoff=false;

    Vector3 wait_point=new Vector3(2.26f, -3f, 4.88f);
    Vector3 hold_point=new Vector3(2.26f, -1.42f, 4.88f);
    Quaternion hold_lrot;

    Vector3 surface_posi;
    Vector3 surface_normal;

    Vector3 move_posi;

    int setting_num_frame=20;
    int welding_num_frame=80;
    bool is_welding=false;

    // Start is called before the first frame update
    void Start()
    {
        this.WorkspaceCtr=GameObject.Find("Controller").GetComponent<WorkspaceCtr>();

        this.Spark=transform.Find("Spark").GetComponent<ParticleSystem>();
        this.Spark.Stop();

        transform.localEulerAngles=new Vector3(0,-90,74);  //localEulerAngles of drill at its holding point in camera
        hold_lrot=transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(num_anim!=0){
            if(is_holding){
                if(frame<5){
                    transform.Translate(0, 0.3f, 0);
                    frame+=1;
                }else{
                    num_anim-=1;
                    is_holding=false;
                    frame=0;
                    transform.localPosition = hold_point;
                }
            }else if(is_holdoff){
                if(frame<5){
                    transform.Translate(0, -0.3f, 0);
                    frame+=1;
                }else{
                    num_anim-=1;
                    is_holdoff=false;
                    frame=0;
                    transform.localPosition = wait_point;
                    this.gameObject.SetActive(false);
                }
            }
            
            if(is_welding){
                if(frame<setting_num_frame){
                    transform.position+=move_posi;
                    frame+=1;
                }else if(frame>=setting_num_frame && frame<setting_num_frame+welding_num_frame){
                    this.Spark.Play();
                    frame+=1;
                }else if(frame>=setting_num_frame+welding_num_frame && frame<setting_num_frame*2+welding_num_frame){
                    this.Spark.Stop();
                    transform.position-=move_posi;
                    frame+=1;
                }else{
                    this.WorkspaceCtr.WeldingEnd();
                    num_anim-=1;
                    is_welding=false;
                    frame=0;
                }
            }
        }
    }

    public void HoldWeldM(){

        this.gameObject.SetActive(true);

        transform.parent = Camera.main.transform;
        transform.localPosition = wait_point;
        transform.localRotation = hold_lrot;

        num_anim+=1;
        SetMove(0);
        frame=0;

    }

    public void HoldOff(){

        transform.parent=Camera.main.transform;
        transform.localPosition = hold_point;
        num_anim+=1;
        SetMove(1);
        frame=0;

    }


    public void Welding(Vector3 posi, Vector3 normal){
        Debug.Log("welding start");
        Debug.Log(posi+" "+normal);
        this.gameObject.SetActive(true);

        this.surface_posi=posi;
        this.surface_normal=normal;

        move_posi=(posi-transform.position)/setting_num_frame;

        num_anim+=1;
        SetMove(2);
        frame=0;
        
    }

    /*
    public void SetWeldMToSurface(Vector3 posi, Vector3 normal){
        this.gameObject.SetActive(true);

        this.surface_posi=posi;
        this.surface_normal=normal;

        q_aim = Quaternion.FromToRotation(new Vector3(0,1,0), normal);
        //move_q=q/setting_num_frame;

/*
        Vector3 origin_leA=transform.localEulerAngles;
        transform.rotation=Quaternion.FromToRotation(new Vector3(0,1,0), normal);
        Debug.Log("aim up: "+transform.up);
        move_rot=(transform.localEulerAngles-origin_leA)/setting_num_frame;
        transform.localEulerAngles = origin_leA;

        move_posi=(posi-transform.position)/setting_num_frame;

        //anim=true;
        num_anim+=1;
        SetMove(2);
        frame=0;
    }

    public void SetOff(){
        this.gameObject.SetActive(true);
        //move_rot=(new Vector3(0,-90,74)-transform.localEulerAngles)/setting_num_frame;

        move_posi=(hold_point-transform.localPosition)/setting_num_frame;
        //anim=true;
        num_anim+=1;
        SetMove(3);
        frame=0;
    }
*/

    void SetMove(int mode){  //同時に二つのmoveが起こるのを防ぐため
        switch(mode){
            case 0:
                is_holding=true;
                is_holdoff=false;
                is_welding=false;
                break;

            case 1:
                is_holding=false;
                is_holdoff=true;
                is_welding=false;
                break;

            case 2:
                is_holding=false;
                is_holdoff=false;
                is_welding=true;
                break;

            case 3:
                is_holding=false;
                is_holdoff=false;
                is_welding=false;
                break;
        }
    }

}
