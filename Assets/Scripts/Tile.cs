using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public enum TileType
{
    Normal,
    Row,
    Col,
    Special
}
public class Tile : MonoBehaviour
{
    public TileType tileType;
    public Material normalMat;
    public Material rowMat;
    public Material colMat;
    public Material specialMat;
    public int xIndex;
    public int yIndex;
    public string type;
    private float moveTime = 0.3f;
    private MeshRenderer quad1;
    private MeshRenderer quad2;
    private GameController gameController;
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private Coroutine moveDown_Croroutine;


    private void Awake() {
        gameController = GameObject.FindObjectOfType<GameController>();
        tileType = TileType.Normal;
        quad1 = this.transform.GetChild(0).GetComponent<MeshRenderer>();
        quad2 = this.transform.GetChild(1).GetComponent<MeshRenderer>();
    }
    
    public TileType GetTileType()
    {
        return tileType;
    }

    public void SetTileType(TileType tileType)
    {
        if(GetTileType() == tileType) return;
        this.tileType = tileType;
        Material mat = normalMat;
        switch(tileType)
        {
            case TileType.Row : mat = rowMat; break;
            case TileType.Col : mat = colMat; break;
            case TileType.Special : mat = specialMat; type = "special"; break;
        }
        quad1.material = mat;
        quad2.material = mat;
    }

    public void MoveDown(int x, int y) {
        if(moveDown_Croroutine != null)
        {
            StopCoroutine(moveDown_Croroutine);
            moveDown_Croroutine = StartCoroutine(CoMoveDown(x, y));
        }
        else
            moveDown_Croroutine = StartCoroutine(CoMoveDown(x, y));
    }

    IEnumerator CoMoveDown(int x, int y)
    {
        //Debug.Log($"CoMoveDown to {x}, {y}");
        Vector3 newP = new Vector3(x, y, 0);
        while(Vector3.Distance(transform.position, newP) > 0.01f)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, newP, Time.deltaTime*10);
            yield return new WaitForEndOfFrame();
        }
        transform.position = newP;
    }

    private void OnMouseDown()
    {
        //Debug.Log($"mouseDown : {xIndex}, {yIndex}");
        //gameController.SelectTile(xIndex, yIndex);
        touchStartPos = Input.mousePosition;
        //touchStartPos = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        if(gameController.isMoving) return;

        touchEndPos = Input.mousePosition;
       // Debug.Log($"mouseUp : {xIndex}, {yIndex}");

        float swipeDist = (touchEndPos - touchStartPos).magnitude;
        if (swipeDist > 20f)
        {
            Vector2 swipeDir = (touchEndPos - touchStartPos).normalized;
            StartCoroutine(Swipe(swipeDir));
        }
    }

    IEnumerator Swipe(Vector2 swipeDir)
    {
        int endX = xIndex;
        int endY = yIndex;
        
        if (Mathf.Abs(swipeDir.x) > Mathf.Abs(swipeDir.y))
        {
            // 가로로 스와이프
            int xDiff = (swipeDir.x > 0f) ? 1 : -1;
            //Debug.Log($"블럭 교체 : {xIndex}, {yIndex} <-> {xIndex + xDiff}, {yIndex}");
            yield return StartCoroutine(gameController.MoveTile(xIndex, yIndex, xIndex + xDiff, yIndex));
        }
        else
        {
            // 세로로 스와이프
            int yDiff = (swipeDir.y > 0f) ? 1 : -1;
            //Debug.Log($"블럭 교체 : {xIndex}, {yIndex} <-> {xIndex}, {yIndex + yDiff}");
            yield return StartCoroutine(gameController.MoveTile(xIndex, yIndex, xIndex, yIndex + yDiff));
        }

        Check(endX, endY);
    }

    public void Check(int endX, int endY)
    {
        //둘중 하나가 스페셜 타일이면 나머지 종류는 다 삭제함.
        bool isSpecial = gameController.CheckSpecialTile(xIndex, yIndex, endX, endY);

        //스페셜 타일이 없을 경우에만 블럭 다시 교체 여부 판단
        if(!isSpecial)
        {    
            //Debug.Log($"스페셜 타일이 없을 경우에만 블럭 다시 교체 여부 판단");
            StartCoroutine(gameController.CoDelayCheckMatches(xIndex, yIndex, (isMatches) => 
            {
                if(!isMatches)
                {
                    Debug.Log($"블럭 다시 교체 : {xIndex}, {yIndex} <-> {endX}, {endY}");
                    StartCoroutine(gameController.MoveTile(xIndex, yIndex, endX, endY));
                }
            }));
        }
    }

    public IEnumerator MoveTo(int x, int y)
    {   
        float t = 0;
        Vector2 startPos = new Vector2(xIndex, yIndex);
        Vector2 endPos = new Vector2(x * gameController.tileSize, y * gameController.tileSize);
        while (t < moveTime)
        {
            t += Time.deltaTime;
            float factor = t / moveTime;
            this.transform.position = Vector2.Lerp(startPos, endPos, factor);
            yield return null;
        }
        this.transform.position = endPos;
        xIndex = x;
        yIndex = y;
    }
}
