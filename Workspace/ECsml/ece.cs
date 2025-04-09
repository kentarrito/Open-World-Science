using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ece : MonoBehaviour
{
    public string type;
    public Dictionary<string,ece> connect_ece_dict=new Dictionary<string,ece>();

    public string lc_str;  // lc1+","+lc2
    public int[] lc;

    public List<int[]> clc_raw=new List<int[]>();  //相対座標 向きの情報は含まない
    public List<int[]> clc=new List<int[]>();  //相対座標 向きの情報を含む

    public int dir=0;  //反時計回転の順に0,1,2,3
    public GameObject btn;

    public ece(string type,int[] lc,int dir,GameObject ECPanelBtnPfb,GameObject ECEs){  //,GameObject ECPanelBtnPfb,GameObject ECEs
        this.type=type;
        this.lc=lc;
        this.dir=dir;

        lc_str=this.lc[0]+","+this.lc[1];
        /*
        string[] lc_str_arr=lc_str.Split(',');
        this.lc[0]=Int32.Parse(lc_str_arr[0]);
        this.lc[1]=Int32.Parse(lc_str_arr[1]);
        */

        if(type=="res" || type=="cap" || type=="wire"){
            this.clc_raw.Add(new int[2]{0,1});
            this.clc_raw.Add(new int[2]{0,-1});
        }

        this.btn=Instantiate(ECPanelBtnPfb,ECEs.transform);
        this.btn.GetComponent<Image>().sprite=ECsml.ece_spr_dict[type];
        this.btn.transform.localPosition=new Vector3(lc[0]*100f,lc[1]*100f,0);

        foreach(var rclc in this.clc_raw){
            if(dir==0){
                this.clc.Add(new int[]{rclc[0],rclc[1]});
                this.connect_ece_dict.Add((rclc[0]+lc[0])+","+(rclc[1]+lc[1]),null);
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
    }

    public ece(string type,int[] lc,List<int[]> clc){  //Directionが定義できないもの
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
}
