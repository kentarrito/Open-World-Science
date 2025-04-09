using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ECsml : MonoBehaviour
{
    #region variables

    public GameObject ECPanel;
    public GameObject ECPanel2;
    public GameObject ECEs;
    public GameObject LineDrawer_obj;
    LineDrawer LineDrawer;

    //general
    public int[] lc_now;

    //Generating
    public GameObject ContentECScrollView;
    public GameObject ECScrollViewBtnPfb;
    public GameObject ECPanelBtnPfb;
    public bool is_ece_selected;
    public bool is_ece_generated;
    public string selected_ece_type;
    public GameObject generating_ece_btn;
    public ece generating_ece;
    public int[] pointer_lc_gene;

    //Grabbing ECE
    public bool is_ece_grabbed;

    //Move Joint
    int[] start_lc;
    int[] tip_lc;
    string moving_wire_name;
    public bool is_wiring;  //maybe not used anymore
    public GameObject JointBtnPfb;
    //List<GameObject> joints=new List<GameObject>();
    public GameObject joints;

    public Dictionary<string,List<int[]>> wire_dict=new Dictionary<string,List<int[]>>(); //needed to draw line
    public Dictionary<string,ecc> ecc_dict=new Dictionary<string,ecc>();///arg:res1,cap3,wire1...
    public Dictionary<string,List<ecc>> endlc_ecc_dict=new Dictionary<string,List<ecc>>();//arg: endlc_str, return: [ecc1,ecc2,...]
    public Dictionary<string,int> num_type_dict=new Dictionary<string,int>(){{"wire",0}};//arg:res,cap  return:0,1,2...  //don't trust this number since it can be decrased by deleting the components

    //Set Param
    Dictionary<string,float> V;  //ALL endlc_str => V
    Dictionary<int,float> I;  //ALL sc_id => I
    Dictionary<int,List<string>> sc_ne;  //serial connections id => nodal or ecc name
    Dictionary<string,List<int>> ne_sc; //nodal or ecc name => serial connection id
    List<string> ground;

    bool loop=true;
    List<string> rest_ne;

    //MNA
    List<float> v;
    List<string> v_id;
    List<float> i;
    List<string[]> i_endlc;
    List<int> i_id;
    //List<float> j;
    //List<float> e;
    List<ecc> stamp_ecc;

    float dt;
    float[,] a;
    float[] j;
    float[] e;
    

    //Dictionary<int,string[]> dl;
    //Dictionary<int,List<ecc>> dlc;

    public static Dictionary<string,Sprite> ece_spr_dict=new Dictionary<string,Sprite>();
    public static bool is_initialized=false;
    public Sprite res_spr;
    public Sprite cap_spr;
    public Sprite ind_spr;
    public Sprite ground_spr;


    
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //  we must compare two objects not as List<int> but as int

        //set_connect_lc_dict();
        this.LineDrawer=this.LineDrawer_obj.GetComponent<LineDrawer>();
        this.ECPanel2.SetActive(false);
        set_ece_spr_dict();
        SetECPanel();
    }

    #region ECPanel
    public void SetECPanel(){
        for(int j=0;j<this.ContentECScrollView.transform.childCount;j++) Destroy(this.ContentECScrollView.transform.GetChild(0).gameObject);

        int i=0;
        foreach(var key in ECsml.ece_spr_dict.Keys){
            GameObject set=Instantiate(this.ECScrollViewBtnPfb, this.ContentECScrollView.transform);
            set.name=key;
            set.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-10-70*i);
            set.transform.GetChild(1).GetComponent<Image>().sprite=ECsml.ece_spr_dict[key];
            set.transform.GetChild(1).GetComponent<GenerateECECtr>().ece_type=key;
            i+=1;
        }
        this.ContentECScrollView.GetComponent<RectTransform>().sizeDelta=new Vector2(0,20+70*i);
    }

    #region Generate ECE

    public void SelectGeneratingECE(string ece_type){   //ece_type comes from ece_spr_dict
        this.ECPanel2.SetActive(true);
        this.is_ece_selected=true;
        this.selected_ece_type=ece_type;
    }

    public void SelectOffGeneratingECE(){
        this.ECPanel2.SetActive(false);
        this.is_ece_selected=false;
    }

    public void PointerEnterECPanel2(Vector2 pos){
        if(this.is_ece_selected){
            this.is_ece_generated=true;
            int[] lc=GetLC(pos,this.ECEs);
            //this.generating_ece=new ece(this.selected_ece_type,lc,0,this.ECanelBtnPfb,this.ECEs);
            //this.generating_ece_btn=this.generating_ece.btn;
            this.generating_ece_btn=Instantiate(this.ECPanelBtnPfb,this.ECEs.transform);
            this.generating_ece_btn.GetComponent<Image>().sprite=ECsml.ece_spr_dict[this.selected_ece_type];
            this.generating_ece_btn.transform.localPosition=new Vector3(lc[0]*25f,lc[1]*25f,0);
            this.generating_ece_btn.name=this.selected_ece_type;
        }
    }

    public void PointerExitECPanel2(){
        this.is_ece_generated=false;
        Destroy(this.generating_ece_btn);
    }

    public void PointerDragOnECPanel2(Vector2 pos){
        if(this.is_ece_selected && this.is_ece_generated){
            int[] lc=GetLC(pos,this.ECEs);
            if(this.pointer_lc_gene!=lc){
                this.generating_ece_btn.transform.localPosition=new Vector3(lc[0]*25f,lc[1]*25f,0);
                this.pointer_lc_gene=lc;
            }
        }
    }

    public void DragEndOnECPanel2(Vector2 pos){
        if(this.is_ece_generated){
            int[] lc=GetLC(pos,this.ECEs);
            this.generating_ece_btn.transform.localPosition=new Vector3(lc[0]*25f,lc[1]*25f,0);
            string name=GetName(this.selected_ece_type);
            ecc ecc=new ecc(name,this.selected_ece_type,lc,this.generating_ece_btn,this.ECEs);
            //this.generating_ece.SetValue(lc,0);
            FixingWireWhenAddingECC(ecc);
            AddECCDict(ecc);
            SetJointBtn();
        }
        this.ECPanel2.SetActive(false);
        this.is_ece_selected=false;
        this.is_ece_generated=false;
    }

    #endregion

    public void MoveECE(ecc ecc,int[] lc){
        if(this.lc_now!=lc){
            string lc_str=lc[0]+","+lc[1];
            if(ecc_dict.ContainsKey(lc_str)==false){
                lc_now=lc;
                //ResetECE(ecc,lc,ecc.dir);
                ecc.btn.transform.localPosition=new Vector3(lc[0]*25f,lc[1]*25f,0);
            }
        }
    }

    public void JointGrabbed(int[] lc,GameObject joint){
        start_lc=lc;
        //moving_wire_name="wire"+(num_type_dict["wire"]+1);
        moving_wire_name=GetName("wire");
        wire_dict.Add(moving_wire_name,null);
    }

    public void MoveJoint(Vector2 pos,int[] lc, GameObject joint){
        if(Math.Abs(pos.x-this.ECEs.transform.position.x-start_lc[0]*25f)>Math.Abs(pos.y-this.ECEs.transform.position.y-start_lc[1]*25f)){
            tip_lc=new int[2]{lc[0],start_lc[1]};
        }else{
            tip_lc=new int[2]{start_lc[0],lc[1]};
        }

        wire_dict[moving_wire_name]=new List<int[]>(){start_lc,tip_lc};

        joint.transform.localPosition=new Vector3(tip_lc[0]*25f,tip_lc[1]*25f,0);
        joint.GetComponent<JointBtnCtr>().lc=tip_lc;
        
        MakeWireLine();
    }

    public void MoveEndJoint(GameObject joint){
        MakeWireLine();

        wire_dict.Remove(moving_wire_name);
        num_type_dict["wire"]-=1;

        if(start_lc[0]!=tip_lc[0] || start_lc[1]!=tip_lc[1]){
            bool did_specific_situation_happen=false;

            foreach(var wire_name in wire_dict.Keys){  //for the case tip_lc is on another wire already existing
                List<ecc> eccs_to_remove=new List<ecc>();
                List<ecc> eccs_to_add=new List<ecc>();

                if(wire_dict[wire_name][0][0]==wire_dict[wire_name][1][0]){
                    if(tip_lc[0]==wire_dict[wire_name][0][0]){
                        if(start_lc[0]==tip_lc[0]){  //for the case existing wire and new wire are connected in parallel
                            if((tip_lc[1]>wire_dict[wire_name][0][1] && tip_lc[1]<wire_dict[wire_name][1][1]) || (tip_lc[1]<wire_dict[wire_name][0][1] && tip_lc[1]>wire_dict[wire_name][1][1])){
                                List<int> lcs=new List<int>(){start_lc[1],tip_lc[1],wire_dict[wire_name][0][1],wire_dict[wire_name][1][1]};
                                lcs.Sort();
                                
                                eccs_to_remove.Add(ecc_dict[wire_name]);
                                
                                if(lcs[0]==start_lc[1]){
                                    tip_lc[1]=lcs[3];
                                }else if(lcs[3]==start_lc[1]){
                                    tip_lc[1]=lcs[0];
                                }else Debug.Log("error");

                                /*
                                string name=GetName("wire");
                                List<int[]> endlc=new List<int[]>();
                                endlc.Add(new int[]{tip_lc[0],lcs[0]});
                                endlc.Add(new int[]{tip_lc[0],lcs[3]});

                                ecc ecc=new ecc(name,"wire",endlc);

                                eccs_to_add.Add(ecc);
                                */
                                did_specific_situation_happen=true;
                                break;
                            }
                        }else{  //for the case tip_lc is connected on existing wire perpendicularly
                            if((tip_lc[1]>wire_dict[wire_name][0][1] && tip_lc[1]<wire_dict[wire_name][1][1]) || (tip_lc[1]<wire_dict[wire_name][0][1] && tip_lc[1]>wire_dict[wire_name][1][1])){
                                eccs_to_remove.Add(ecc_dict[wire_name]);

                                string name=GetName("wire");
                                List<int[]> endlc=new List<int[]>();
                                endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                                endlc.Add(new int[]{tip_lc[0],wire_dict[wire_name][0][1]});
                                ecc ecc=new ecc(name,"wire",endlc);
                                eccs_to_add.Add(ecc);

                                name=GetName("wire");
                                endlc=new List<int[]>();
                                endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                                endlc.Add(new int[]{tip_lc[0],wire_dict[wire_name][1][1]});
                                ecc=new ecc(name,"wire",endlc);
                                eccs_to_add.Add(ecc);

                                did_specific_situation_happen=true;
                                break;
                            }
                        }
                    }
                    /*
                    else if(tip_lc[1]==wire_dict[wire_name][0][1]  && start_lc[1]==wire_dict[wire_name][0][1]){  //the case existed wire endlc is on new wire
                        if((tip_lc[0]>wire_dict[wire_name][0][0] && start_lc[0]<wire_dict[wire_name][0][0]) || (tip_lc[0]<wire_dict[wire_name][0][0] && start_lc[0]>wire_dict[wire_name][0][0])){
                            string name=GetName("wire");
                            List<int[]> endlc=new List<int[]>();
                            endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                            endlc.Add(new int[]{wire_dict[wire_name][0][0],tip_lc[1]});
                            ecc ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);


                            name=GetName("wire");
                            endlc=new List<int[]>();
                            endlc.Add(new int[]{start_lc[0],tip_lc[1]});
                            endlc.Add(new int[]{wire_dict[wire_name][0][0],tip_lc[1]});
                            ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);
                            did_specific_situation_happen=true;
                        }
                    }else if(tip_lc[1]==wire_dict[wire_name][1][1]  && start_lc[1]==wire_dict[wire_name][1][1]){  //the case existed wire endlc is on new wire
                        if((tip_lc[0]>wire_dict[wire_name][1][0] && start_lc[0]<wire_dict[wire_name][1][0]) || (tip_lc[0]<wire_dict[wire_name][1][0] && start_lc[0]>wire_dict[wire_name][1][0])){
                            string name=GetName("wire");
                            List<int[]> endlc=new List<int[]>();
                            endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                            endlc.Add(new int[]{wire_dict[wire_name][1][0],tip_lc[1]});
                            ecc ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);


                            name=GetName("wire");
                            endlc=new List<int[]>();
                            endlc.Add(new int[]{start_lc[0],tip_lc[1]});
                            endlc.Add(new int[]{wire_dict[wire_name][1][0],tip_lc[1]});
                            ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);

                            did_specific_situation_happen=true;
                        }
                    }
                    */
                }else if(wire_dict[wire_name][0][1]==wire_dict[wire_name][1][1]){
                    if(tip_lc[1]==wire_dict[wire_name][0][1]){
                        if(start_lc[1]==tip_lc[1]){
                            if((tip_lc[0]>wire_dict[wire_name][0][0] && tip_lc[0]<wire_dict[wire_name][1][0]) || (tip_lc[0]<wire_dict[wire_name][0][0] && tip_lc[0]>wire_dict[wire_name][1][0])){
                                List<int> lcs=new List<int>(){start_lc[0],tip_lc[0],wire_dict[wire_name][0][0],wire_dict[wire_name][1][0]};
                                lcs.Sort();
                                
                                eccs_to_remove.Add(ecc_dict[wire_name]);
                                
                                if(lcs[0]==start_lc[0]){
                                    tip_lc[0]=lcs[3];
                                }else if(lcs[3]==start_lc[0]){
                                    tip_lc[0]=lcs[0];
                                }else Debug.Log("error");
                                /*
                                string name=GetName("wire");
                                List<int[]> endlc=new List<int[]>();
                                endlc.Add(new int[]{lcs[0],tip_lc[1]});
                                endlc.Add(new int[]{lcs[3],tip_lc[1]});

                                ecc ecc=new ecc(name,"wire",endlc);

                                eccs_to_add.Add(ecc);
                                */
                                did_specific_situation_happen=true;
                                break;
                            }
                        }else{
                            if((tip_lc[0]>wire_dict[wire_name][0][0] && tip_lc[0]<wire_dict[wire_name][1][0]) || (tip_lc[0]<wire_dict[wire_name][0][0] && tip_lc[0]>wire_dict[wire_name][1][0])){
                                eccs_to_remove.Add(ecc_dict[wire_name]);

                                string name=GetName("wire");
                                List<int[]> endlc=new List<int[]>();
                                endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                                endlc.Add(new int[]{wire_dict[wire_name][0][0],tip_lc[1]});
                                ecc ecc=new ecc(name,"wire",endlc);
                                eccs_to_add.Add(ecc);

                                name=GetName("wire");
                                endlc=new List<int[]>();
                                endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                                endlc.Add(new int[]{wire_dict[wire_name][1][0],tip_lc[1]});
                                ecc=new ecc(name,"wire",endlc);
                                eccs_to_add.Add(ecc);

                                did_specific_situation_happen=true;
                                break;
                            }
                        }
                    }
                    /*
                    else if(tip_lc[0]==wire_dict[wire_name][0][0]  && start_lc[0]==wire_dict[wire_name][0][0]){  //the case existed wire endlc is on new wire
                        if((tip_lc[1]>wire_dict[wire_name][0][1] && start_lc[1]<wire_dict[wire_name][0][1]) || (tip_lc[1]<wire_dict[wire_name][0][1] && start_lc[1]>wire_dict[wire_name][0][1])){
                            string name=GetName("wire");
                            List<int[]> endlc=new List<int[]>();
                            endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                            endlc.Add(new int[]{tip_lc[0],wire_dict[wire_name][0][1]});
                            ecc ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);


                            name=GetName("wire");
                            endlc=new List<int[]>();
                            endlc.Add(new int[]{tip_lc[0],start_lc[1]});
                            endlc.Add(new int[]{tip_lc[0],wire_dict[wire_name][0][1]});
                            ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);
                            did_specific_situation_happen=true;
                        }
                    }else if(tip_lc[0]==wire_dict[wire_name][1][0]  && start_lc[0]==wire_dict[wire_name][1][0]){  //the case existed wire endlc is on new wire
                        if((tip_lc[1]>wire_dict[wire_name][1][1] && start_lc[1]<wire_dict[wire_name][1][1]) || (tip_lc[1]<wire_dict[wire_name][1][1] && start_lc[1]>wire_dict[wire_name][1][1])){
                            string name=GetName("wire");
                            List<int[]> endlc=new List<int[]>();
                            endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                            endlc.Add(new int[]{tip_lc[0],wire_dict[wire_name][1][1]});
                            ecc ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);


                            name=GetName("wire");
                            endlc=new List<int[]>();
                            endlc.Add(new int[]{tip_lc[0],start_lc[1]});
                            endlc.Add(new int[]{tip_lc[0],wire_dict[wire_name][1][1]});
                            ecc=new ecc(name,"wire",endlc);

                            eccs_to_add.Add(ecc);
                            did_specific_situation_happen=true;
                        }
                    }
                    */
                }

                ResetECC(eccs_to_remove,eccs_to_add);
            }

            
            //for the case already existing wire or ecc is on the new wire, otherwise just add new wire (new wire must be added here)
            if(tip_lc[0]==start_lc[0]){
                List<ecc> eccs_to_add=new List<ecc>();
                int[] check_point_endlc=start_lc;
                string name;
                List<int[]> endlc;
                ecc ecc;

                List<int> lcs=new List<int>(){start_lc[1],tip_lc[1]};
                lcs.Sort();
                for(int i=lcs[0]+1;i<lcs[1];i++){
                    string endlc_str=tip_lc[0]+","+i;
                    if(endlc_ecc_dict.ContainsKey(endlc_str)){
                        name=GetName("wire");
                        endlc=new List<int[]>();
                        endlc.Add(check_point_endlc);
                        endlc.Add(new int[]{tip_lc[0],i});
                        ecc=new ecc(name,"wire",endlc);
                        eccs_to_add.Add(ecc);

                        check_point_endlc=new int[]{tip_lc[0],i};
                    }
                }

                name=GetName("wire");
                endlc=new List<int[]>();
                endlc.Add(check_point_endlc);
                endlc.Add(tip_lc);
                ecc=new ecc(name,"wire",endlc);
                eccs_to_add.Add(ecc);

                ResetECC(new List<ecc>(),eccs_to_add);

            }else{
                List<ecc> eccs_to_add=new List<ecc>();
                int[] check_point_endlc=start_lc;
                string name;
                List<int[]> endlc;
                ecc ecc;

                List<int> lcs=new List<int>(){start_lc[0],tip_lc[0]};
                lcs.Sort();
                for(int i=lcs[0]+1;i<lcs[1];i++){
                    string endlc_str=i+","+tip_lc[1];
                    if(endlc_ecc_dict.ContainsKey(endlc_str)){
                        name=GetName("wire");
                        endlc=new List<int[]>();
                        endlc.Add(check_point_endlc);
                        endlc.Add(new int[]{i,tip_lc[1]});
                        ecc=new ecc(name,"wire",endlc);
                        eccs_to_add.Add(ecc);

                        check_point_endlc=new int[]{i,tip_lc[1]};
                    }
                }

                name=GetName("wire");
                endlc=new List<int[]>();
                endlc.Add(check_point_endlc);
                endlc.Add(tip_lc);
                ecc=new ecc(name,"wire",endlc);
                eccs_to_add.Add(ecc);

                ResetECC(new List<ecc>(),eccs_to_add);
            }

            /*
            if(did_specific_situation_happen==false){
                string name=GetName("wire");
                List<int[]> endlc=new List<int[]>();
                endlc.Add(new int[]{tip_lc[0],tip_lc[1]});
                endlc.Add(new int[]{start_lc[0],start_lc[1]});

                ecc ecc=new ecc(name,"wire",endlc);

                AddECC(ecc);
            }
            */
        }
/*
        else{
            wire_dict.Remove(moving_wire_name);
            num_type_dict["wire"]-=1;
        }
*/
        SetJointBtn();
    }

    public void MakeWireLine(){  //make wire line only by using wire_dict
        List<bool> is_verticals=new List<bool>();
        List<Vector2> starts=new List<Vector2>();
        List<float> lengths=new List<float>();
        List<float> widths=new List<float>();
        List<Color> colors=new List<Color>();

        foreach(var key in wire_dict.Keys){
            if(wire_dict[key][0][0]==wire_dict[key][1][0]){
                is_verticals.Add(true);
                starts.Add(new Vector2(wire_dict[key][0][0]*25f,wire_dict[key][0][1]*25f));
                lengths.Add((wire_dict[key][1][1]-wire_dict[key][0][1])*25f);
                widths.Add(2f);
                colors.Add(Color.black);
            }else if(wire_dict[key][0][1]==wire_dict[key][1][1]){
                is_verticals.Add(false);
                starts.Add(new Vector2(wire_dict[key][0][0]*25f,wire_dict[key][0][1]*25f));
                lengths.Add((wire_dict[key][1][0]-wire_dict[key][0][0])*25f);
                widths.Add(2f);
                colors.Add(Color.black);
            }else Debug.Log("error");
        }

        this.LineDrawer.DrawVHLine(is_verticals,starts,lengths,widths,colors);
    }


    #region functions

    public int[] GetLC(Vector2 pos,GameObject ECEs){
        int[] lc=new int[2];
        lc[0]=(int)Math.Round((pos.x-ECEs.transform.position.x)/25f);    //data.position is the length from (0,0,0) point in 3d space, same as x,y components of transform.position
        lc[1]=(int)Math.Round((pos.y-ECEs.transform.position.y)/25f);
        return lc;
    }

    //should be collected for 3 endpoint ecc
    public int[] GetLC2(string lc_str){
        string[] lc_str_arr=lc_str.Split(',');
        
        int num=lc_str_arr.Length;
        int[] lc_arr=new int[num];
        for(int i=0;i<num;i++){
            lc_arr[i]=Int32.Parse(lc_str_arr[i]);
        }
        return lc_arr;
    }

    public string GetName(string type){  //op>num_type_dict
        if(num_type_dict.ContainsKey(type)){
            num_type_dict[type]+=1;
        }else{
            num_type_dict.Add(type,1);
        }
        return type+num_type_dict[type];
    }

    void SetJointBtn(){  //set all joint btns only by endlc_ecc_dict
        for(int i=0;i<joints.transform.childCount;i++){
            Destroy(joints.transform.GetChild(i).gameObject);
        }

        foreach(var endlc_str in endlc_ecc_dict.Keys){
            if(endlc_ecc_dict[endlc_str]!=null){
                //endlc_ecc_dict[end_lc][0].btn.GetComponent<ECPanelBtnCtr>().JointBtn=new List<GameObject>();
                int[] lc=GetLC2(endlc_str);
                
                GameObject joint=Instantiate(this.JointBtnPfb,joints.transform);
                joint.transform.localPosition=new Vector3(25f*lc[0],25f*lc[1],0);
                joint.GetComponent<JointBtnCtr>().lc=lc;
            }
        }

/*
        foreach(var ece in ece_dict.Values){
            ece.btn.GetComponent<ECPanelBtnCtr>().JointBtn=new List<GameObject>();
            int i=0;
            foreach(var clc in ece.clc){
                if(ece.connect_ece_dict[(clc[0]+ece.lc[0])+","+(clc[1]+ece.lc[1])]==null){
                    GameObject joint=Instantiate(this.JointBtnPfb,ece.btn.transform);
                    joint.transform.localPosition=new Vector3(50f*ece.clc_raw[i][0],50f*ece.clc_raw[i][1],0);
                    joint.GetComponent<JointBtnCtr>().ece=ece;
                    joint.GetComponent<JointBtnCtr>().clc=clc;
                    joints.Add(joint);
                    ece.btn.GetComponent<ECPanelBtnCtr>().JointBtn.Add(joint);
                }
                i+=1;
            }
        }
*/
    }

    /*
    void SetJointBtnKeepingFor(GameObject KeepingJoint,ece eece,int[] eclc){
        foreach(var joint in joints){
            if(joint!=KeepingJoint) Destroy(joint);
        }
        joints=new List<GameObject>();

        foreach(var ece in ece_dict.Values){
            ece.btn.GetComponent<ECPanelBtnCtr>().JointBtn=new List<GameObject>();
            int i=0;
            foreach(var clc in ece.clc){
                if(ece.connect_ece_dict[(clc[0]+ece.lc[0])+","+(clc[1]+ece.lc[1])]==null){
                    if(ece==eece && clc[0]==eclc[0] && clc[1]==eclc[1]){
                        KeepingJoint.transform.parent=eece.btn.transform;
                        KeepingJoint.transform.localPosition=new Vector3(50f*ece.clc_raw[i][0],50f*ece.clc_raw[i][1],0);
                        KeepingJoint.GetComponent<JointBtnCtr>().ece=ece;
                        KeepingJoint.GetComponent<JointBtnCtr>().clc=clc;
                        joints.Add(KeepingJoint);
                        ece.btn.GetComponent<ECPanelBtnCtr>().JointBtn.Add(KeepingJoint);
                    }else{
                        GameObject joint=Instantiate(this.JointBtnPfb,ece.btn.transform);
                        joint.transform.localPosition=new Vector3(50f*ece.clc_raw[i][0],50f*ece.clc_raw[i][1],0);
                        joint.GetComponent<JointBtnCtr>().ece=ece;
                        joint.GetComponent<JointBtnCtr>().clc=clc;
                        joints.Add(joint);
                        ece.btn.GetComponent<ECPanelBtnCtr>().JointBtn.Add(joint);
                    }
                }
                i+=1;
            }
        }
    }
    */

    void FixingWireWhenAddingECC(ecc ecc){
        List<ecc> eccs_to_remove=new List<ecc>();
        List<ecc> eccs_to_add=new List<ecc>();

        foreach(var wire_name in wire_dict.Keys){  //for the case tip_lc is on another wire already existing
            List<int[]> endlcs=new List<int[]>();
            List<int> lcs=new List<int>();
            bool is_vertical=false;

            foreach(var endlc in ecc.endlc){
                if(wire_dict[wire_name][0][0]==wire_dict[wire_name][1][0]){
                    if(endlc[0]==wire_dict[wire_name][0][0] && ((endlc[1]<wire_dict[wire_name][0][1] && endlc[1]>wire_dict[wire_name][1][1]) || (endlc[1]>wire_dict[wire_name][0][1] && endlc[1]<wire_dict[wire_name][1][1]))){
                        endlcs.Add(endlc);
                        lcs.Add(endlc[1]);
                        is_vertical=true;
                    }
                }else{
                    if(endlc[1]==wire_dict[wire_name][0][1] && ((endlc[0]<wire_dict[wire_name][0][0] && endlc[0]>wire_dict[wire_name][1][0]) || (endlc[0]>wire_dict[wire_name][0][0] && endlc[0]<wire_dict[wire_name][1][0]))){
                        Debug.Log("a: "+endlc[1]+","+wire_dict[wire_name][0][1]+","+endlc[0]+","+wire_dict[wire_name][0][0]+","+endlc[0]+","+wire_dict[wire_name][1][0]+","+endlc[0]+","+wire_dict[wire_name][0][0]+","+endlc[0]+","+wire_dict[wire_name][1][0]);
                        endlcs.Add(endlc);
                        lcs.Add(endlc[0]);
                        is_vertical=false;
                    }
                }
            }

            if(endlcs.Count>1){
                Debug.Log("endlcs.Count>1");
                eccs_to_remove.Add(ecc_dict[wire_name]);

                if(is_vertical){
                    lcs.Add(wire_dict[wire_name][0][1]);
                    lcs.Add(wire_dict[wire_name][1][1]);

                    lcs.Sort();

                    string name=GetName("wire");
                    List<int[]> lc=new List<int[]>();
                    lc.Add(new int[]{wire_dict[wire_name][0][0],lcs[0]});
                    lc.Add(new int[]{wire_dict[wire_name][0][0],lcs[1]});
                    ecc ecc1=new ecc(name,"wire",lc);
                    eccs_to_add.Add(ecc1);

                    name=GetName("wire");
                    lc=new List<int[]>();
                    lc.Add(new int[]{wire_dict[wire_name][0][0],lcs[lcs.Count-2]});
                    lc.Add(new int[]{wire_dict[wire_name][0][0],lcs[lcs.Count-1]});
                    ecc1=new ecc(name,"wire",lc);
                    eccs_to_add.Add(ecc1);

                }else{
                    lcs.Add(wire_dict[wire_name][0][0]);
                    lcs.Add(wire_dict[wire_name][1][0]);

                    lcs.Sort();

                    string name=GetName("wire");
                    List<int[]> lc=new List<int[]>();
                    lc.Add(new int[]{lcs[0],wire_dict[wire_name][0][1]});
                    lc.Add(new int[]{lcs[1],wire_dict[wire_name][0][1]});
                    ecc ecc1=new ecc(name,"wire",lc);
                    eccs_to_add.Add(ecc1);

                    name=GetName("wire");
                    lc=new List<int[]>();
                    lc.Add(new int[]{lcs[lcs.Count-2],wire_dict[wire_name][0][1]});
                    lc.Add(new int[]{lcs[lcs.Count-1],wire_dict[wire_name][0][1]});
                    ecc1=new ecc(name,"wire",lc);
                    eccs_to_add.Add(ecc1);
                }


            }else if(endlcs.Count==1){
                Debug.Log("endlcs.Count==1");
                eccs_to_remove.Add(ecc_dict[wire_name]);

                string name=GetName("wire");
                List<int[]> lc=new List<int[]>();
                lc.Add(endlcs[0]);
                lc.Add(wire_dict[wire_name][0]);
                ecc ecc1=new ecc(name,"wire",lc);
                eccs_to_add.Add(ecc1);

                string test=ecc1.name;

                name=GetName("wire");
                lc=new List<int[]>();
                lc.Add(endlcs[0]);
                lc.Add(wire_dict[wire_name][1]);
                ecc1=new ecc(name,"wire",lc);
                eccs_to_add.Add(ecc1);

                test+=", "+ecc1.name;
                Debug.Log(test);
            }
        }

        ResetECC(eccs_to_remove,eccs_to_add);
    }

    public void AddECCDict(ecc ecc){ //op>ece_dict,endlc_ecc_dict
        ecc_dict.Add(ecc.name,ecc);

        //process of endlc_ecc_dict
        for(int i=0;i<ecc.endlc_str.Count;i++){
            if(endlc_ecc_dict.ContainsKey(ecc.endlc_str[i])){
                endlc_ecc_dict[ecc.endlc_str[i]].Add(ecc);
            }else{
                List<ecc> list=new List<ecc>();
                list.Add(ecc);
                endlc_ecc_dict.Add(ecc.endlc_str[i],list);
            }
        }

        if(ecc.type=="wire"){   //this is needed at some part. leave this here just in case.
            if(!wire_dict.ContainsKey(ecc.name)){
                wire_dict.Add(ecc.name,ecc.endlc);
            }
        }
    }

    public void RemoveECCDict(ecc ecc){
        ecc_dict.Remove(ecc.name);
        
        foreach(var endlc_str in ecc.endlc_str){
            if(endlc_ecc_dict[endlc_str].Count==1){
                endlc_ecc_dict.Remove(endlc_str);
            }else{
                endlc_ecc_dict[endlc_str].Remove(ecc);
            }
        }

        if(ecc.type=="wire"){
            if(wire_dict.ContainsKey(ecc.name)){
                wire_dict.Remove(ecc.name);
            }
        }
    }

    public void ResetECC(List<ecc> eccs_to_remove, List<ecc> eccs_to_add){
        foreach(var ecc in eccs_to_remove){
            RemoveECCDict(ecc);
        }

        foreach(var ecc in eccs_to_add){
            AddECCDict(ecc);
        }
    }

    /*
    public void RemoveECE(ece ece){
        //Destroy(ece.btn);
        string remove_lc_str="";
        List<string> lc_str_list=new List<string>();
        List<string> lc_str2_list=new List<string>();

        foreach(var lc_str in ece_dict.Keys){
            if(ece==ece_dict[lc_str]){
                remove_lc_str=lc_str;
            }else{
                foreach(var lc_str2 in ece_dict[lc_str].connect_ece_dict.Keys){
                    if(ece_dict[lc_str].connect_ece_dict[lc_str2]==ece){
                        lc_str_list.Add(lc_str);
                        lc_str2_list.Add(lc_str2);
                        //ece_dict[lc_str].connect_ece_dict.Remove(lc_str2);  //eceそのもののclcはSetValueでリセット
                    }
                }
            }
        }

        ece_dict.Remove(remove_lc_str);
        for(int i=0;i<lc_str_list.Count;i++){
            ece_dict[lc_str_list[i]].connect_ece_dict.Remove(lc_str2_list[i]);
        }
    }

    public void ResetECE(ece ece,int[] lc,int dir){
        ece_dict.Remove(ece.lc[0]+","+ece.lc[1]);
        ece_dict.Add(lc[0]+","+lc[1],ece);

        List<ece> ece_dict_vals=new List<ece>();
        foreach(var ece1 in ece_dict.Values){  //making data set
            ece_dict_vals.Add(ece1);
        }

        for(int i=0;i<ece_dict_vals.Count;i++){  //SetValue
            if(ece_dict_vals[i]==ece){
                ece_dict_vals[i].SetValue(lc,dir);
            }else{
                ece_dict_vals[i].SetValue(ece_dict_vals[i].lc,ece_dict_vals[i].dir);
            }
        }

        for(int i=0;i<ece_dict_vals.Count;i++){  //SetValue2
            ece_dict_vals[i].SetValue2(ece_dict);
        }

        SetJointBtn();
    }

    public void ResetECE_notSetJoint(ece ece,int[] lc,int dir){
        ece_dict.Remove(ece.lc[0]+","+ece.lc[1]);
        ece_dict.Add(lc[0]+","+lc[1],ece);

        List<ece> ece_dict_vals=new List<ece>();
        foreach(var ece1 in ece_dict.Values){  //making data set
            ece_dict_vals.Add(ece1);
        }

        for(int i=0;i<ece_dict_vals.Count;i++){  //SetValue
            if(ece_dict_vals[i]==ece){
                ece_dict_vals[i].SetValue(lc,dir);
            }else{
                ece_dict_vals[i].SetValue(ece_dict_vals[i].lc,ece_dict_vals[i].dir);
            }
        }

        for(int i=0;i<ece_dict_vals.Count;i++){  //SetValue2
            ece_dict_vals[i].SetValue2(ece_dict);
        }
    }

    public void RotateBtn(int dir,GameObject btn){
        if(dir==0){
            btn.transform.localEulerAngles=new Vector3(0,0,0);
        }else if(dir==1){
            btn.transform.localEulerAngles=new Vector3(0,0,90);
        }else if(dir==2){
            btn.transform.localEulerAngles=new Vector3(0,0,180);
        }else if(dir==3){
            btn.transform.localEulerAngles=new Vector3(0,0,-90);
        }
    }
    */

    #endregion

    #endregion

    #region FirstSet

    void set_ece_spr_dict(){
        if(!ECsml.is_initialized){
            ECsml.ece_spr_dict.Add("res",res_spr);
            ECsml.ece_spr_dict.Add("cap",cap_spr);
            ECsml.ece_spr_dict.Add("ground",ground_spr);
            ECsml.ece_spr_dict.Add("ind",ind_spr);
            ECsml.is_initialized=true;
        }
    }
    #endregion

    #region simulation

    public void RunSML(){
    }

    public void SetParam(){
        ground=new List<string>();
        string start_ground="";

        bool error=true;
        foreach(var ecc in ecc_dict.Values){
            if(ecc.type=="ground"){
                if(error){
                    start_ground=ecc.name;
                    ground.Add(ecc.endlc_str[0]);
                    error=false;
                }else{
                    ground.Add(ecc.endlc_str[0]);
                }
            }
        }

        V=new Dictionary<string,float>(){{ground[0],0f}};
        //V.Add(ground[0],new List<float>(){0f});
        I=new Dictionary<int,float>(){{0,0f}};  //arg:dl_id
        sc_ne=new Dictionary<int,List<string>>(){{0,new List<string>(){ground[0]}}};  //serial connections id => nodal or ecc name
        ne_sc=new Dictionary<string,List<int>>(){{ground[0],new List<int>(){0}}};

        loop=true;
        rest_ne=new List<string>(ecc_dict.Keys);
        rest_ne.AddRange(new List<string>(endlc_ecc_dict.Keys));
        rest_ne.Remove(ground[0]);
        rest_ne.Remove(start_ground);

        int max_sc_id=1;

        if(!error){
            while(loop)
            {
                List<string> eccs=new List<string>();
                //List<string> nodals=new List<string>();
                List<int> sc_ids=new List<int>();
                
                //List<string> remove_sc_ids=new List<string>();

                Debug.Log("rest_ne: "+DebugStringList(rest_ne));

                string sc_ne_test="sc_ne:\r\n";
                foreach(var key in sc_ne.Keys){
                    sc_ne_test+=" "+key+":";
                    foreach(var key2 in sc_ne[key]){
                        sc_ne_test+=key2+",";
                    }
                    sc_ne_test+="\r\n";
                }
                Debug.Log(sc_ne_test);

                //Prepare the lists used in Process functions
                List<int> exception_sc_ids=new List<int>();
                foreach(var sc_id in sc_ne.Keys){
                    string lc_str=sc_ne[sc_id][sc_ne[sc_id].Count-1];
                    if(lc_str!=";"){
                        foreach(var ecc in endlc_ecc_dict[lc_str]){
                            if(rest_ne.Contains(ecc.name)){
                                if(eccs.Contains(ecc.name)){  //Exception: when the two path connected at ecc
                                    int i=eccs.IndexOf(ecc.name);
                                    sc_ne[sc_ids[i]].Add(ecc.name);
                                    sc_ne[sc_id].Reverse();
                                    sc_ne[sc_ids[i]].AddRange(sc_ne[sc_id]);  //sc_ids[i] already existed
                                    sc_ne[sc_ids[i]].Add(";");

                                    ne_sc.Add(ecc.name,new List<int>(){sc_ids[i]});
                                    foreach(var ne_to_change in sc_ne[sc_id]){
                                        ne_sc[ne_to_change].Remove(sc_id);
                                        ne_sc[ne_to_change].Add(sc_ids[i]);
                                    }

                                    //sc_ne.Remove(sc_id);   //this is prohibitted, so the next code is the substitution
                                    exception_sc_ids.Add(sc_id);  //since there is no ecc which has more than 3 points, this is the last sc_id related to this exception. that's the reason I can do this.
                                    eccs.Remove(ecc.name);
                                    sc_ids.Remove(sc_ids[i]);  //this shouldn't be removed?

                                    rest_ne.Remove(ecc.name);
                                    //remove_sc_ids.Add(sc_ids[i]);
                                }else{  //rest_ne should be processed here
                                    eccs.Add(ecc.name);
                                    sc_ids.Add(sc_id);
                                    rest_ne.Remove(ecc.name);
                                }
                                break; //at nodal point, there can be two sc_ids started from the nodal. then each sc_id should correspond to one process. this is the reason of this break
                            }
                        }
                    }
                }
                foreach(var sc_id in exception_sc_ids){
                    sc_ne.Remove(sc_id);
                }

                string sc_ids_test="sc_ids : ";
                foreach(var key in sc_ids){
                    sc_ids_test+=key+",";
                }
                Debug.Log(sc_ids_test);


                if(eccs.Count==0){   //the case where graph is not connected
                    if(rest_ne.Count!=0){
                        Debug.Log("The whole graph is not connected.");
                        loop=false;
                    }
                }

                List<int> sc_ids_to_remove=new List<int>();

                //Process of eccs and nodals
                for(int i=0;i<sc_ids.Count;i++){

                    //sc_ne, ne_sc process for ecc
                    if(sc_ne.ContainsKey(sc_ids[i])){
                        sc_ne[sc_ids[i]].Add(eccs[i]);
                    }else{
                        Debug.Log("error");
                        //sc_ne.Add(sc_ids[i],new List<string>(){eccs[i]});
                    }
                    ne_sc.Add(eccs[i],new List<int>(){sc_ids[i]});

                    //Nodals process
                    foreach(var endlc_str in ecc_dict[eccs[i]].endlc_str){
                        if(sc_ne[sc_ids[i]].Contains(endlc_str)==false){   //if there is a ecc which has 3 nodals, this should be corrected
                            if(rest_ne.Contains(endlc_str)){  //normal point
                                if(endlc_ecc_dict[endlc_str].Count>2){  //the point is nodal
                                    Debug.Log("nodal process case 1");
                                    V.Add(endlc_str,0f);
                                    sc_ne[sc_ids[i]].Add(endlc_str);
                                    sc_ne[sc_ids[i]].Add(";");

                                    ne_sc.Add(endlc_str,new List<int>(){sc_ids[i]});

                                    foreach(var ecc in endlc_ecc_dict[endlc_str]){
                                        if(rest_ne.Contains(ecc.name)){
                                            sc_ne.Add(max_sc_id,new List<string>(){endlc_str});
                                            ne_sc[endlc_str].Add(max_sc_id);
                                            max_sc_id+=1;
                                        }
                                    }

                                    rest_ne.Remove(endlc_str);

                                }else if(endlc_ecc_dict[endlc_str].Count==2){  //the point is not a nodal, but just a endlc
                                    Debug.Log("nodal process case 2");
                                    V.Add(endlc_str,0f);
                                    sc_ne[sc_ids[i]].Add(endlc_str);
                                    ne_sc.Add(endlc_str,new List<int>(){sc_ids[i]});

                                    rest_ne.Remove(endlc_str);
                                }
                            }else{  //other node also reached the nodal at the same time
                                if(endlc_ecc_dict[endlc_str].Count>2){
                                    Debug.Log("nodal process case 3");
                                    sc_ne[sc_ids[i]].Add(endlc_str);
                                    sc_ne[sc_ids[i]].Add(";");

                                    ne_sc[endlc_str].Add(sc_ids[i]);

                                    foreach(var ecc in endlc_ecc_dict[endlc_str]){
                                        if(rest_ne.Contains(ecc.name)){
                                            sc_ne.Add(max_sc_id,new List<string>(){endlc_str});
                                            ne_sc[endlc_str].Add(max_sc_id);
                                            max_sc_id+=1;
                                        }else{
                                            //ne_sc[ecc.name][0]  //don't delete
                                            int delete_sc_id=0;
                                            foreach(var sc_id1 in ne_sc[endlc_str]){
                                                if(sc_ne[sc_id1].Count==1){
                                                    sc_ne.Remove(sc_id1);
                                                    delete_sc_id=sc_id1;
                                                    break;
                                                }
                                            }
                                            ne_sc[endlc_str].Remove(delete_sc_id);
                                        }
                                    }
                                }else if(endlc_ecc_dict[endlc_str].Count==2){
                                    Debug.Log("nodal process case 4");
                                    int other_sc_id=ne_sc[endlc_str][0];  //ne_sc[endlc_str] should have only one sc_id
                                    sc_ne[sc_ids[i]].Reverse();
                                    sc_ne[other_sc_id].AddRange(sc_ne[sc_ids[i]]);
                                    sc_ne[other_sc_id].Add(";");

                                    foreach(var ne_to_change in sc_ne[sc_ids[i]]){
                                        ne_sc[ne_to_change].Remove(sc_ids[i]);
                                        ne_sc[ne_to_change].Add(other_sc_id);
                                    }

                                    //sc_ne.Remove(sc_ids[i]);  //this causes a problem. almost same as the exception_sc_ids
                                    sc_ids_to_remove.Add(sc_ids[i]);
                                }
                            }
                        }
                    }
                }

                foreach(var sc_id in sc_ids_to_remove){
                    sc_ne.Remove(sc_id);
                }

                if(rest_ne.Count==0){
                    loop=false;
                }
            }

            string text="sc_ne:\r\n";
            foreach(var key1 in sc_ne.Keys){
                text+=key1+": ";
                foreach(var key2 in sc_ne[key1]){
                    text+=key2+",";
                }
                text+="\r\n";
            }
            Debug.Log(text);

        }else Debug.Log("There are some errors.");
    }

    public void Analysis(){
        string text="ecc dict : ";
        foreach(var key1 in ecc_dict.Keys){
            text+=key1+",";
        }
        Debug.Log(text);

        text="wire dict : ";
        foreach(var key1 in wire_dict.Keys){
            text+=key1+",";
        }
        Debug.Log(text);

        SetParam();

        //PrepareMNA();
    }

    /*

    void PrepareMNA(){
        v=new List<float>();
        v_id=new List<string>();
        i=new List<float>();
        i_endlc=new List<string[]>(); //endlcs of ecc at the i
        i_id=new List<int>();  //ecc name
        
        //j=new List<float>();
        //e=new List<float>();

        foreach(var endlc in V.Keys){
            if(!ground.Contains(endlc)){  //remove obvious endlcs connected to others with wire to make the calculation easier
                v.Add(V[endlc]);
                v_id.Add(endlc);
            }
        }

        foreach(var sc_id in sc_ne.Keys){
            for(int j=0;j<sc_ne[sc_id].Count;j++){
                if(ecc_dict[sc_ne[sc_id][j]].type=="voltage"){
                    i.Add(0f);
                    i_id.Add(sc_ne[sc_id][j]);
                    i_endlc.Add(new string[]{sc_ne[sc_id][j-1],sc_ne[sc_id][j+1]}); //nodals behind the voltage and ahead the voltage respectively
                }else if(ecc_dict[sc_ne[sc_id][j]].type=="ind"){
                    i.Add(0f);
                    i_id.Add(sc_ne[sc_id][j]);
                    i_endlc.Add(new string[]{sc_ne[sc_id][j-1],sc_ne[sc_id][j+1]});
                }
            }
        }

        //Note: every ecc should have only 2 endlc here (change all 3 endpoints components into equivalent circuit only with 2 endpoints ecc)

        //Make Lists necessarily for OneStepMNA
        stamp_ecc_type=new List<string>();
        stamp_ecc_var=new List<List<float>>();
        stamp_id=new List<int[]>();

        int n=v.Count;
        int m=i.Count;

        foreach(var ne in ne_sc.Keys){
            if(!ne.Contains(",")){  //only ecc
                stamp_ecc_type.Add(ecc_dict[ne].type);
                stamp_ecc_var.Add(ecc_dict[ne].var);

                string sc_id=ne_sc[ne][0];
                //sc_ne.Valu
                if(sc_ne[sc_id].IndexOf(ecc_dict[ne].endlc_str[0])<sc_ne[sc_id].IndexOf(ecc_dict[ne].endlc_str[1])){
                    stamp_id.Add(new int[]{v_id.IndexOf(endlc_str[0]),v_id.IndexOf(endlc_str[1])});
                }else if(sc_ne[sc_id].IndexOf(ecc_dict[ne].endlc_str[0])>sc_ne[sc_id].IndexOf(ecc_dict[ne].endlc_str[1])){
                    stamp_id.Add(new int[]{v_id.IndexOf(endlc_str[1]),v_id.IndexOf(endlc_str[0])});
                }else Debug.Log("error");

                if(ecc_dict[ne].type=="voltage"){
                    stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[0]));
                    stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[1]));
                    if(i_endlc[i_id.IndexOf(ne)].IndexOf(ecc_dict[ne].endlc_str[0])==0){
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[0]));
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[1]));
                    }else{
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[1]));
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[0]));
                    }
                    stamp_id.Add(n+i_id.IndexOf(ne));
                }else if(ecc_dict[ne].type=="current"){
                    stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[1]));  //use this notation
                    stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[0]));
                }else if(ecc_dict[ne].type=="ind"){
                    if(i_endlc[i_id.IndexOf(ne)].IndexOf(ecc_dict[ne].endlc_str[0])==0){
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[0]));
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[1]));
                    }else{
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[1]));
                        stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[0]));
                    }
                    stamp_id.Add(n+i_id.IndexOf(ne));
                }else if(ecc_dict[ne].type=="res" || ecc_dict[ne].type=="cap"){
                    stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[0]));
                    stamp_id.Add(v_id.IndexOf(ecc_dict[ne].endlc_str[1]));
                }
            }
        }

        //to do: it's better to split a into time dependent component and time independent one to make MNA faster
        a=new float[m+n,m+n];
        rs=new float[m+n];

        //to make first condition
    }

    void OneStepMNA(){  //make
        for(int k=0;k<stamp_ecc_type.Count;k++){
            switch(stamp_ecc_type[k]){
                case "res":
                    a[stamp_id[k][0],stamp_id[k][0]]+=stamp_ecc_var[k][1];
                    a[stamp_id[k][0],stamp_id[k][1]]+=-stamp_ecc_var[k][1];
                    a[stamp_id[k][1],stamp_id[k][0]]+=-stamp_ecc_var[k][1];
                    a[stamp_id[k][1],stamp_id[k][1]]+=stamp_ecc_var[k][1];
                case "cap":
                    a[stamp_id[k][0],stamp_id[k][0]]+=stamp_ecc_var[k][0]/dt;
                    a[stamp_id[k][0],stamp_id[k][1]]+=-stamp_ecc_var[k][0]/dt;
                    a[stamp_id[k][1],stamp_id[k][0]]+=-stamp_ecc_var[k][0]/dt;
                    a[stamp_id[k][1],stamp_id[k][1]]+=stamp_ecc_var[k][0]/dt;
                    rs[stamp_id[k][0]]+=stamp_ecc_var[k][0]*(v[stamp_id[k][0]]-v[stamp_id[k][1]])/dt;
                    rs[stamp_id[k][1]]+=stamp_ecc_var[k][0]*(v[stamp_id[k][1]]-v[stamp_id[k][0]])/dt;
                case "ind":
                    a[stamp_id[k][2],stamp_id[k][0]]+=dt/stamp_ecc_var[k][0];
                    a[stamp_id[k][2],stamp_id[k][1]]+=-dt/stamp_ecc_var[k][0];
                    a[stamp_id[k][0],stamp_id[k][2]]+=1f;
                    a[stamp_id[k][1],stamp_id[k][1]]+=-1f;
                    a[stamp_id[k][2],stamp_id[k][2]]+=-1f;
                    rs[stamp_id[k][2]]+=i[stamp_id[k][2]];
                case "current":
                    rs[stamp_id[k][0]]+=stamp_ecc_var[k][0];  //should take AC current into account
                    rs[stamp_id[k][0]]+=-stamp_ecc_var[k][0];
                case "voltage":
                    a[stamp_id[k][4],stamp_id[k][0]]+=1f;
                    a[stamp_id[k][4],stamp_id[k][1]]+=-1f;
                    a[stamp_id[k][2],stamp_id[k][4]]+=1f;
                    a[stamp_id[k][3],stamp_id[k][4]]+=1f;
                    rs[stamp_id[k][4]]+=stamp_ecc_var[k][0]; //should take AC voltage into account
            }
        }
    }

    float[] LUdecomposition(float[,] a, float[] rs, int len){
        float[,] l=new float[len,len];
        float[,] u=new float[len,len];
        
        for(int k1=0;k1<len;k1++){
            for(int k2=0;k2<len-k1;k2++){
                l[k2,k1]=a[k2,k1];
                if(k2==0){
                    u[k1,k2]=1f;
                }else{
                    //u[k1,k2]=a[];
                }
            }
        }
    }

    */

    public void ShowData(){
        string info="";
        foreach(var ecc in ecc_dict.Values){
            info+=ecc.name+": ";
            foreach(var str in ecc.endlc_str){
                info+=str+"  ";
            }
            info+="\r\n";
        }
        Debug.Log(info);

        info="";
        foreach(var endlc_str in endlc_ecc_dict.Keys){
            info+=endlc_str+": ";
            foreach(var ecc in endlc_ecc_dict[endlc_str]){
                info+=ecc.name+"  ";
            }
            info+="\r\n";
        }
        Debug.Log(info);

        info="";
        foreach(var wire_name in wire_dict.Keys){
            info+=wire_name+": ";
            foreach(var lc in wire_dict[wire_name]){
                info+=lc[0]+","+lc[1]+"  ";
            }
            info+="\r\n";
        }
        Debug.Log(info);

    }

    string DebugStringList(List<string> list){
        string text="";
        foreach(var key in list){
            text+=key+" ";
        }
        return text;
    }

    #endregion
}


//to do 
//1, ground  ok 
//2, when connecting wire into capacitor, 2 joints appeared instead of the two disappearing  ok(perhaps)
//3, wire should be reconstructed everytime it is put
//4, joint btn should be at everypoints

//5, added wire also can be splited by already existing endlc   ok
//6, change the code for the case nothing happended at MoveEndJoint  ok

//7, change the code for when existing wire or ecc is on new wire, into the one which search every endlc along the new wire  ok
//8, fix error

//9, the case new endlc is on existing wire should have been processed in AddECC => maybe not true
//10, the case new ecc's endlc is on existing wire is not considered yet   ok

//11, wire dict become uncorrect for some reasons when existing endlc is on new wire

// probably there is no problem up to showdata
// there still be problem at whole data connected part in analysis  => I'll fix it next time I find it

//12, change process of V in 1,every ecc has 2 nodals connected directly to itself, 2,wire shouldn't be taken as a component in this process
//13, 