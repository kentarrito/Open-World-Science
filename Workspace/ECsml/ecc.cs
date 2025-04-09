using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ecc : MonoBehaviour
{
    public string name;
    public string type;
    public Dictionary<int,List<ecc>> connect_ecc_list=new Dictionary<int,List<ecc>>();  //arg: endlc index   return: connected eccs
    //public Dictionary<string,ecc> connect_ecc_dict=new Dictionary<string,ecc>();  //argument is like res1, cap3, or something like that
    
    //general variables
    public List<float> var=new List<float>();  //"res":[R,G] "cap":[C] "ind":[L]
    public List<int[]> endlc=new List<int[]>();  //endpoints of lattice coordinate
    public List<string> endlc_str=new List<string>();
    
    //variables for ecc
    public int[] lc;
    public int dir=0;  //assign 0,1,2,3 counterclockwise
    public GameObject btn;

    public ecc(string name,string type,int[] lc,GameObject ecc_btn,GameObject ECEs){  //constructor for ecc
        this.name=name;
        this.type=type;
        this.lc=lc;
        this.btn=ecc_btn;
        this.btn.GetComponent<ECPanelBtnCtr>().ecc=this;

        //this.dir=dir;
        //lc_str=this.lc[0]+","+this.lc[1];
        /*
        string[] lc_str_arr=lc_str.Split(',');
        this.lc[0]=Int32.Parse(lc_str_arr[0]);
        this.lc[1]=Int32.Parse(lc_str_arr[1]);
        */

        //Note: 
        if(type=="res" || type=="cap" || type=="ind"){
            this.endlc.Add(new int[2]{lc[0],lc[1]+2});
            this.endlc.Add(new int[2]{lc[0],lc[1]-2});
            this.endlc_str.Add(this.endlc[0][0]+","+this.endlc[0][1]);
            this.endlc_str.Add(this.endlc[1][0]+","+this.endlc[1][1]);
        }else if(type=="voltage" || type=="current"){  //Note: high voltage or down side of current must be added first, also note it when creating rotation function
            this.endlc.Add(new int[2]{lc[0],lc[1]+2});
            this.endlc.Add(new int[2]{lc[0],lc[1]-2});
            this.endlc_str.Add(this.endlc[0][0]+","+this.endlc[0][1]);
            this.endlc_str.Add(this.endlc[1][0]+","+this.endlc[1][1]);
        }else if(type=="ground"){
            this.endlc.Add(new int[2]{lc[0],lc[1]+2});
            this.endlc_str.Add(this.endlc[0][0]+","+this.endlc[0][1]);
        }else Debug.Log("error");

/*
        this.btn=Instantiate(ECPanelBtnPfb,ECEs.transform);
        this.btn.GetComponent<Image>().sprite=ECsml.ece_spr_dict[type];
        this.btn.transform.localPosition=new Vector3(lc[0]*25f,lc[1]*25f,0);

*/

        /*
        foreach(var rclc in this.clc_raw){
            if(dir==0){
                this.clc.Add(new int[]{rclc[0],rclc[1]});
              x  this.connect_ece_dict.Add((rclc[0]+lc[0])+","+(rclc[1]+lc[1]),null);
            }else if(dir==1){
                this.clc.Add(new int[]{-rclc[1],rclc[0]});
                this.connect_ece_dict.Add((-rclc[1]+lc[0])+","+(rclc[0]+lc[1]),null);
            }else if(dir==2){
                this.clc.Add(new int[]{-rclc[0],-rclc[1]});
                this.connect_ece_dict.Add((-rclc[0]+lc[0])+","+(-rclc[1]+lc[1]),null);
            }else{
                this.clc.Add(new int[]{rclc[1],-rclc[0]});
                this.connect_ece_dict.Add((rclc[0]+lc[0])+","+(-rclc[1]+lc[1]),null);
            }
        }
        */
    }

/*
    public ecc(string type,int[] lc,List<int[]> clc){  //Directionが定義できないもの
        this.type=type;
        this.lc=lc;
        this.clc=clc;
        this.lc_str=lc[0]+","+lc[1];
        this.dir=4;  //これらはdirを4で管理

        foreach(var clc1 in clc){
            this.connect_ece_dict.Add((clc1[0]+lc[0])+","+(clc1[1]+lc[1]),null);
        }

        if(type=="wire"){
            if(clc.Count==2){
                if(clc[0][0]==-clc[1][0] && clc[0][1]==-clc[1][1]){
                    if(clc[0][0]==0){
                    }
                }
            }
        }
    }
*/

    //still need to make system to designate the parent obj for line drawing
    public ecc(string name,string type, List<int[]> endlc){   //constructor for wire
        this.name=name;
        this.type=type;
        this.endlc=endlc;
        this.endlc_str.Add(this.endlc[0][0]+","+this.endlc[0][1]);
        this.endlc_str.Add(this.endlc[1][0]+","+this.endlc[1][1]);
    }

    public float j(List<int[]> endlc_dir){
        if(this.type=="current"){
            switch(this.dir){
                case 0:
                    if(endlc_dir[0][1]<endlc_dir[1][1]) return this.var[0];
                    else return -this.var[0];
                case 1:
                    if(endlc_dir[0][0]>endlc_dir[1][0]) return this.var[0];
                    else return -this.var[0];
                case 2:
                    if(endlc_dir[0][1]>endlc_dir[1][1]) return this.var[0];
                    else return -this.var[0];
                case 3:
                    if(endlc_dir[0][0]<endlc_dir[1][0]) return this.var[0];
                    else return -this.var[0];
                default:
                    Debug.Log("error");
                    return 0f;
            }
        }else{
            return 0f;
        }
    }

    public float e(List<string> endlc_dir){
        if(this.type=="voltage"){
            switch(this.dir){
                case 0:
                    if(endlc_dir[0][1]<endlc_dir[1][1]) return this.var[0];
                    else return -this.var[0];
                case 1:
                    if(endlc_dir[0][0]>endlc_dir[1][0]) return this.var[0];
                    else return -this.var[0];
                case 2:
                    if(endlc_dir[0][1]>endlc_dir[1][1]) return this.var[0];
                    else return -this.var[0];
                case 3:
                    if(endlc_dir[0][0]<endlc_dir[1][0]) return this.var[0];
                    else return -this.var[0];
                default:
                    Debug.Log("error");
                    return 0f;
            }
        }else{
            return 0f;
        }
    }

    /*

    public void SetValue(int[] lc,int dir){   //eceの位置と向きのみで決まる値を設定  注：他のeceの値は一切関係ない
        this.lc=lc;
        this.lc_str=lc[0]+","+lc[1];
        this.dir=dir;

        this.connect_ece_dict=new Dictionary<string,ece>();
        this.clc=new List<int[]>();
        foreach(var rclc in this.clc_raw){
            if(this.dir==0){
                this.clc.Add(new int[]{rclc[0],rclc[1]});
                this.connect_ece_dict.Add((rclc[0]+lc[0])+","+(rclc[1]+lc[1]),null);
                this.btn.transform.localEulerAngles=new Vector3(0,0,0);
            }else if(this.dir==1){
                this.clc.Add(new int[]{-rclc[1],rclc[0]});
                this.connect_ece_dict.Add((-rclc[1]+lc[0])+","+(rclc[0]+lc[1]),null);
                this.btn.transform.localEulerAngles=new Vector3(0,0,90);
            }else if(this.dir==2){
                this.clc.Add(new int[]{-rclc[0],-rclc[1]});
                this.connect_ece_dict.Add((-rclc[0]+lc[0])+","+(-rclc[1]+lc[1]),null);
                this.btn.transform.localEulerAngles=new Vector3(0,0,180);
            }else{
                this.clc.Add(new int[]{rclc[1],-rclc[0]});
                this.connect_ece_dict.Add((rclc[0]+lc[0])+","+(-rclc[1]+lc[1]),null);
                this.btn.transform.localEulerAngles=new Vector3(0,0,-90);
            }
        }
    }

    public void SetValue2(Dictionary<string,ece> ece_dict){  //connect_ece_dictの値を設定 他のeceの値を考慮
        List<string> clc_list=new List<string>();
        foreach(var clc_str in this.connect_ece_dict.Keys){
            if(ece_dict.ContainsKey(clc_str)){
                if(ece_dict[clc_str].connect_ece_dict.ContainsKey(this.lc_str)){
                    clc_list.Add(clc_str);
                    //ece.connect_ece_dict[clc_str]=ece_dict[clc_str];
                    //ece_dict[clc_str].connect_ece_dict[ece.lc_str]=ece;
                }
            }
        }

        foreach(var clc_str in clc_list){  //わざわざこうしないと上のforeachでエラー。foreachでkeyに使ったdictの値は変えてはいけないらしい。
            this.connect_ece_dict[clc_str]=ece_dict[clc_str];
            ece_dict[clc_str].connect_ece_dict[this.lc_str]=this;
        }
    }
    

    public int GetDir(int[] clc,int[] lc){  //clcをlcのところに持ってくるような回転をした後のdirを返す
        int[] lc_rel=new int[2]{lc[0]-this.lc[0],lc[1]-this.lc[1]};
        if(clc[0]==-lc_rel[1] && clc[1]==lc_rel[0]){
            return (this.dir+1)%4;
        }else if(clc[0]==-lc_rel[0] && clc[1]==-lc_rel[1]){
            return (this.dir+2)%4;
        }else if(clc[0]==lc_rel[1] && clc[1]==-lc_rel[0]){
            return (this.dir+3)%4;
        }else{
            return this.dir;
        }
    }

    */
}
