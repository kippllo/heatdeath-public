using UnityEngine;

public class DrawMeshTriangle {

    public Vector3 position; //This is the offset at which the triangle will be rendered.
    public Vector3[] vertices;
    public int[] order; //The order in which the vert make the triangle.

    public Color32 color; //For now, trinagles only have one color!


    public DrawMeshTriangle(): this(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 0, 0), 0,1,2, new Color32(255,255,255,255)) {
    }


    public DrawMeshTriangle(Vector3 vert0, Vector3 vert1, Vector3 vert2, Color32 triColor): this(vert0, vert1, vert2) {
        color = triColor;
    }


    public DrawMeshTriangle(Vector3 vert0, Vector3 vert1, Vector3 vert2) {
        //If no order is give, find the base left-hand rule order that will work!
        //NOTE: This method assumes the Z positions of all the verts are equal. This only work for flat planed triangles!

        /*  Left-Hand Triangulation Algorithm for any given three 2D points:
            1. Recalc Origin and local vert positions.
            2. Convert all into polar coordinates.
            3. Pick the greatest/biggest angle to be the starting point.
            4. Out of the two remaining point, pick the one with the biggest polar angles to be the second point.
            5. Pick the only point left as the last point of the triangle.

            Note: I think for the right handed method you would just pick the smallest angle in this step 4 and maybe the biggest angle in step 3.
        */
        
        //Holder setup...
        Vector3[] usePoints = new Vector3[3];
        Vector3 temp1 = Vector3.zero;
        Vector3 temp2 = Vector3.zero;



        // 1. Recalc Origin and local vert positions.
        vertices = new Vector3[] {vert0, vert1, vert2};
        recenterOrigin();
        
        
        // 2. Convert all into polar coordinates.
        float vert0Angle = Mathf.Atan2(vertices[0].y, vertices[0].x) * (180/Mathf.PI);
        vert0Angle = normalizeUnitCircleAngle(vert0Angle);
        float vert1Angle = Mathf.Atan2(vertices[1].y, vertices[1].x) * (180/Mathf.PI); //See link for cartesian to polar coordinates convert: https://www.mathsisfun.com/polar-cartesian-coordinates.html
        vert1Angle = normalizeUnitCircleAngle(vert1Angle);
        float vert2Angle = Mathf.Atan2(vertices[2].y, vertices[2].x) * (180/Mathf.PI);
        vert2Angle = normalizeUnitCircleAngle(vert2Angle);


        // 3. Pick the greatest/biggest angle to be the starting point.
        if(vert0Angle >= vert1Angle && vert0Angle >= vert2Angle){
            usePoints[0] = vertices[0];
            temp1 = vertices[1];
            temp2 = vertices[2];
        } else if(vert1Angle >= vert0Angle && vert1Angle >= vert2Angle){
            usePoints[0] = vertices[1];
            temp1 = vertices[0];
            temp2 = vertices[2];
        } else if(vert2Angle >= vert0Angle && vert2Angle >= vert1Angle){
            usePoints[0] = vertices[2];
            temp1 = vertices[0];
            temp2 = vertices[1];
        }

        // 4. Out of the two remaining point, pick the one with the biggest polar angles to be the second point.
        // 5. Pick the only point left as the last point of the triangle.

        // Recalc the polar angles since the point may have swapped places.
        vert1Angle = Mathf.Atan2(temp1.y, temp1.x) * (180/Mathf.PI);
        vert1Angle = normalizeUnitCircleAngle(vert1Angle);
        vert2Angle = Mathf.Atan2(temp2.y, temp2.x) * (180/Mathf.PI);
        vert2Angle = normalizeUnitCircleAngle(vert2Angle);

        if(vert1Angle >= vert2Angle){
            usePoints[1] = temp1;
            usePoints[2] = temp2;
        } else if(vert2Angle >= vert1Angle){
            usePoints[1] = temp2;
            usePoints[2] = temp1;
        }

        
        //Set the data back to the properties of this object.
        vertices = usePoints;
        order = new int[3] {0, 1, 2};
    }



    public DrawMeshTriangle(Vector3 vert0, Vector3 vert1, Vector3 vert2, int triOrder0, int triOrder1, int triOrder2, Color32 triColor) {
        vertices = new Vector3[] {vert0, vert1, vert2};
        order = new int[] {triOrder0, triOrder1, triOrder2};
        recenterOrigin();
        color = triColor;
    }

    
    //Recenter the position to be in the center of the verts.
    public void recenterOrigin(){

        Vector3[] globalVertsPos = getExportVerts();

        //Set them to a default value of the first vert.
        float xMin = globalVertsPos[0].x;
        float xMax = globalVertsPos[0].x;

        float yMin = globalVertsPos[0].y;
        float yMax = globalVertsPos[0].y;
        
        float zMin = globalVertsPos[0].z;
        float zMax = globalVertsPos[0].z;

        xMin = (globalVertsPos[1].x <= xMin) ? globalVertsPos[1].x : xMin;
        xMin = (globalVertsPos[2].x <= xMin) ? globalVertsPos[2].x : xMin;

        xMax = (globalVertsPos[1].x >= xMax) ? globalVertsPos[1].x : xMax;
        xMax = (globalVertsPos[2].x >= xMax) ? globalVertsPos[2].x : xMax;


        yMin = (globalVertsPos[1].y <= yMin) ? globalVertsPos[1].y : yMin;
        yMin = (globalVertsPos[2].y <= yMin) ? globalVertsPos[2].y : yMin;

        yMax = (globalVertsPos[1].y >= yMax) ? globalVertsPos[1].y : yMax;
        yMax = (globalVertsPos[2].y >= yMax) ? globalVertsPos[2].y : yMax;


        zMin = (globalVertsPos[1].z <= zMin) ? globalVertsPos[1].z : zMin;
        zMin = (globalVertsPos[2].z <= zMin) ? globalVertsPos[2].z : zMin;

        zMax = (globalVertsPos[1].z >= zMax) ? globalVertsPos[1].z : zMax;
        zMax = (globalVertsPos[2].z >= zMax) ? globalVertsPos[2].z : zMax;


        //Half way betweeen the min an max is the new origin.
        float xPos = (xMax - xMin)/2 + xMin;
        float yPos = (yMax - yMin)/2 + yMin;
        float zPos = (zMax - zMin)/2 + zMin;
        position = new Vector3(xPos, yPos, zPos);

        //Fix the vert positions.
        vertices[0].x -= xPos;
        vertices[1].x -= xPos;
        vertices[2].x -= xPos;

        vertices[0].y -= yPos;
        vertices[1].y -= yPos;
        vertices[2].y -= yPos;

        vertices[0].z -= zPos;
        vertices[1].z -= zPos;
        vertices[2].z -= zPos;
    }

    
    public Vector3[] getExportVerts() {
        Vector3[] exVerts = new Vector3[3]{
            new Vector3(vertices[0].x + position.x, vertices[0].y + position.y, vertices[0].z + position.z),
            new Vector3(vertices[1].x + position.x, vertices[1].y + position.y, vertices[1].z + position.z),
            new Vector3(vertices[2].x + position.x, vertices[2].y + position.y, vertices[2].z + position.z)
        };

        return exVerts;
    }


    //This flips which side of the triangle is rendered.
    public void flipTriangle(){
        int temp0 = order[0];
        int temp1 = order[1];
        order[0] = temp1;
        order[1] = temp0;
    }
    
    public int[] getExportOrder() {
        return order;
    }

    
    public Color32[] getExportColors() {
        return new Color32[3]{
            color,
            color,
            color
        };
    }


    private float normalizeUnitCircleAngle(float angle){
        if (angle < 360 && angle >= 0) { return angle; }

        angle = (angle >= 360) ? angle-360 : angle;
        angle = (angle < 0) ? angle+360 : angle;

        return normalizeUnitCircleAngle(angle);
    }


    //Make this return JSON later!
    override public string ToString(){
        string str = "";
        str += "position: " + position;
        str += ", vert 0: " + getExportVerts()[0]; //return global vert pos, not local...
        str += ", vert 1: " + getExportVerts()[1];
        str += ", vert 2: " + getExportVerts()[2];
        return str;
    }



    //Could make a "ToMesh()" function later, but for now just leave that to the "DrawMesh" script.

}
