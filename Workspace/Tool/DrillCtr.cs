using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillCtr : MonoBehaviour
{
    //Note: this.gameObject should be a child of Camera.main and set the localEulerAngles at (0,-90,74)
    public GameObject DrillBit;
    public GameObject Chip;
    bool anim=false;
    int num_anim=0;
    int frame=0;

    bool is_holding=false;
    bool is_holdoff=false;

    Vector3 wait_point=new Vector3(2.26f, -3f, 4.88f);
    Vector3 hold_point=new Vector3(2.26f, -1.42f, 4.88f);  //localPosition of drill at its holding point in camera
    Quaternion hold_lrot;
    
    Vector3 surface_posi;
    Vector3 surface_normal;

    int setting_num_frame=20;
    bool is_setting=false;
    bool is_setoff=false;
    Vector3 move_rot;
    Vector3 move_posi;
    Quaternion q_aim;

    bool is_drilling=false;
    public bool is_drill_progressed=false;
    float rot_speed;
    

    // Start is called before the first frame update
    void Start()
    {
        transform.localEulerAngles=new Vector3(0,-90,74);  //localEulerAngles of drill at its holding point in camera
        hold_lrot=transform.localRotation;
    }

    // Update is called once per frame
    void Update(){
        if(num_anim!=0){
            if(is_holding){
                if(frame<5){
                    transform.Translate(0, 0.3f, 0);
                    frame+=1;
                }else{
                    num_anim-=1;
                    //anim=false;
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
                    //anim=false;
                    is_holdoff=false;
                    frame=0;
                    transform.localPosition = wait_point;
                    this.gameObject.SetActive(false);
                }
            }
            
            if(is_setting){
                if(frame<setting_num_frame){
                    //transform.localEulerAngles+=move_rot;
                    //transform.rotation=transform.rotation*move_q;
                    transform.rotation=Quaternion.RotateTowards(transform.rotation, q_aim, 10);
                    transform.position+=move_posi;
                    frame+=1;
                }else{
                    num_anim-=1;
                    //anim=false;
                    is_setting=false;
                    frame=0;
                }
            }else if(is_setoff){
                if(frame<setting_num_frame){
                    transform.localRotation=Quaternion.RotateTowards(hold_lrot, transform.localRotation, 10);
                    //transform.localEulerAngles+=move_rot;
                    transform.localPosition+=move_posi;
                    frame+=1;
                }else{
                    num_anim-=1;
                    //anim=false;
                    is_setoff=false;
                    frame=0;
                }
            }

            if(is_drilling){
                this.DrillBit.transform.localEulerAngles+=new Vector3(0,rot_speed,0);
            }

            if(is_drill_progressed){
                System.Random r=new System.Random();

                if(r.Next(0,10)==0){
                    int x=r.Next(-10,11);
                    int z=r.Next(-10,11);
                    Vector2 xz=new Vector2(x,z).normalized;

                    GameObject chip=Instantiate(this.Chip);
                    chip.GetComponent<ChipCtr>().posi=this.surface_posi;
                    chip.GetComponent<ChipCtr>().dir_to_fly=new Vector3(xz.x, 1, xz.y);
                    chip.GetComponent<ChipCtr>().rot_to_normal=Quaternion.FromToRotation(new Vector3(0,1,0), this.surface_normal);
                }
            }
        }
    }

    public void HoldDrill(){

        this.gameObject.SetActive(true);

        transform.parent = Camera.main.transform;
        transform.localPosition = wait_point;
        transform.localRotation = hold_lrot;

        //transform.localEulerAngles=new Vector3(0,-90,74);
        //anim=true;
        num_anim+=1;
        SetMove(0);
        frame=0;

    }

    public void HoldOff(){

        transform.parent=Camera.main.transform;
        transform.localPosition = hold_point;
        //anim=true;
        num_anim+=1;
        SetMove(1);
        frame=0;

    }

    public void SetDrillToSurface(Vector3 posi, Vector3 normal){
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
        */

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

    void SetMove(int mode){  //同時に二つのmoveが起こるのを防ぐため
        switch(mode){
            case 0:
                is_holding=true;
                is_holdoff=false;
                is_setting=false;
                is_setoff=false;
                break;

            case 1:
                is_holding=false;
                is_holdoff=true;
                is_setting=false;
                is_setoff=false;
                break;

            case 2:
                is_holding=false;
                is_holdoff=false;
                is_setting=true;
                is_setoff=false;
                break;

            case 3:
                is_holding=false;
                is_holdoff=false;
                is_setting=false;
                is_setoff=true;
                break;

            case 4:
                is_holding=false;
                is_holdoff=false;
                is_setting=false;
                is_setoff=false;
                break;
        }
    }

    public void RotateBit(float rot_speed){

        this.rot_speed=rot_speed;
        if(rot_speed==0){
            is_drilling=false;
            //anim=false;
            num_anim-=1;
        }else{
            //anim=true;
            num_anim+=1;
            is_drilling=true;
        }
    }

    public void DrillProgress(bool progress){
        anim=progress;
        is_drill_progressed=progress;
    }
}
