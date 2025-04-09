using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineDrawer : MaskableGraphic
{
    List<List<Vector2>> line_coor=new List<List<Vector2>>();
    List<Color> line_color=new List<Color>();
    
    void Start(){
/*
        line_color.Add(Color.red);
        line_color.Add(Color.blue);

        List<Vector2> coor1=new List<Vector2>();
        coor1.Add(new Vector2(0,0));
        coor1.Add(new Vector2(0,-5));
        coor1.Add(new Vector2(-200,-5));
        coor1.Add(new Vector2(-200,0));

        List<Vector2> coor2=new List<Vector2>();
        coor2.Add(new Vector2(0,0));
        coor2.Add(new Vector2(5,0));
        coor2.Add(new Vector2(5,200));
        coor2.Add(new Vector2(0,200));

        line_coor.Add(coor1);
        line_coor.Add(coor2);
*/
    }
    // Start is called before the first frame update
    /*
    public void redraw() {
        SetAllDirty();
    }
    */

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();         // それまで描画したのはクリアして再描画する
        UIVertex vertex = UIVertex.simpleVert;  // simpleVert は標準的なVertex

        int i=0;
        int j=0;
        foreach(var line1_coor in line_coor){
            vertex.color=line_color[j];
            j+=1;
            foreach(var coor in line1_coor){
                vertex.position=coor;
                vh.AddVert(vertex);
                i+=1;
            }
            vh.AddTriangle(i-4, i-3, i-2);
            vh.AddTriangle(i-4, i-2, i-1);
        }

    

/*
        vertex.position = new Vector2(0, 0);
        vh.AddVert(vertex);             // vertex登録。
        vertex.position = new Vector2(200, 0);
        vh.AddVert(vertex);
        vertex.position = new Vector2(0, 200);
        vh.AddVert(vertex);
        vh.AddTriangle(0, 1, 2);        // vertexの番号は登録順（０はじまり）
*/
    }

    public void AddVHLine(bool is_vertical, Vector2 start, float length, float width, Color color){  //Vertical or Horizonal
        List<Vector2> coor=new List<Vector2>();

        if(is_vertical){
            coor.Add(start+new Vector2(-width/2,0));
            coor.Add(start+new Vector2(width/2,0));
            coor.Add(start+new Vector2(width/2,length));
            coor.Add(start+new Vector2(-width/2,length));
        }else{
            coor.Add(start+new Vector2(0,width/2));
            coor.Add(start+new Vector2(length,width/2));
            coor.Add(start+new Vector2(length,-width/2));
            coor.Add(start+new Vector2(0,-width/2));
        }
        
        line_coor.Add(coor);
        line_color.Add(color);

        SetAllDirty();
    }

    public void DrawVHLine(List<bool> is_verticals, List<Vector2> starts, List<float> lengthes, List<float> widthes, List<Color> colors){
        line_coor=new List<List<Vector2>>();
        line_color=new List<Color>();

        for(int i=0;i<is_verticals.Count;i++){
            bool is_vertical=is_verticals[i];
            Vector2 start=starts[i];
            float length=lengthes[i];
            float width=widthes[i];
            Color color=colors[i];

            List<Vector2> coor=new List<Vector2>();

            if(is_vertical){
                coor.Add(start+new Vector2(-width/2,0));
                coor.Add(start+new Vector2(width/2,0));
                coor.Add(start+new Vector2(width/2,length));
                coor.Add(start+new Vector2(-width/2,length));
            }else{
                coor.Add(start+new Vector2(0,width/2));
                coor.Add(start+new Vector2(length,width/2));
                coor.Add(start+new Vector2(length,-width/2));
                coor.Add(start+new Vector2(0,-width/2));
            }
            
            line_coor.Add(coor);
            line_color.Add(color);
        }

        SetAllDirty();
    }

}