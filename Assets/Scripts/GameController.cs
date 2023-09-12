using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using UnityEngine;

public enum GameState{
    Ready,
    Play,
    GameOver
}
public class GameController : MonoBehaviour
{
    public GameState gameState;
    // 블럭 종류
    public GameObject[] tilePrefabs;
    public static GameController instance;

    private void Awake() {
        if(instance == null)
            instance = this;
    }
    public UIManager uiManager;
    public EffectManager effectManager;

    public float gameTime;
    // 그리드 크기
    public int gridWidth = 7;
    public int gridHeight = 8;

    // 타일 크기
    public float tileSize = 1f;

    public bool isMoving;
    private int _score;
    
    public int score{
        get{return _score;}
        set{ _score = value; uiManager.SetText(uiManager.score, _score);}
    }

    // 타일 배열
    private GameObject[,] tiles;

    // Start 함수에서 게임 초기화 처리
    private void Start()
    {
        // 타일 배열 생성
        uiManager = GameObject.FindObjectOfType<UIManager>();
        effectManager = GameObject.FindObjectOfType<EffectManager>();
        tiles = new GameObject[gridWidth, gridHeight];
        ReadyGame();
    }
    // 게임 시작 시 호출되는 함수
    public void StartGame()
    {
        gameState = GameState.Play;
        uiManager.ShowScreen(uiManager.gameScreen);
        StartCoroutine(CoDelayCheckMatches(0.5f));
        StartCoroutine(timer(gameTime));
    }

    // 게임 종료 시 호출되는 함수
    public void GameOver()
    {
        gameState = GameState.GameOver;
        uiManager.ShowScreen(uiManager.endScreen);
        uiManager.SetText(uiManager.scoreFinal, _score);
        uiManager.SetText(uiManager.scoreRank, _score);
        uiManager.StopBlink();
    }

    // 게임 준비 상태로 설정하는 함수
    public void ReadyGame()
    {
        foreach(var tile in tiles)
            Destroy(tile);
        gameState = GameState.Ready;
        uiManager.ShowScreen(uiManager.readyScreen);
        uiManager.showNameInputBtn.interactable = true;
        _isTimerEnd = false;
        score = 0;
        // 타일 생성
        CreateTiles();
    }
    private bool _isTimerEnd = false;
    IEnumerator timer(float time)
    {
        float t = 0;
        while(t < gameTime)
        {
            t += Time.deltaTime;
            uiManager.SetSliderValue(t);
            if(t > gameTime - 10)
            {
                uiManager.BlinkHandle();
            }
            yield return null;
        }
        t = gameTime;
        _isTimerEnd = true;
        uiManager.SetSliderValue(t);
        if(!isMoving) 
        {
            Debug.Log("게임 종료");
            isMoving = true;
            GameOver();
        }
    }
    // 타일 생성 함수
    private void CreateTiles()
    {
        // 그리드 중심을 기준으로 타일 생성
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // 타일 생성 및 위치 설정
                Vector2 position = new Vector2(x, y);
                GameObject tile = Instantiate(tilePrefabs[UnityEngine.Random.Range(0, tilePrefabs.Length)], position, Quaternion.identity);
                tile.name = "Tile_" + x + "_" + y;
                tile.GetComponent<Tile>().xIndex = x;
                tile.GetComponent<Tile>().yIndex = y;
                tiles[x, y] = tile;
            }
        }
    }
    
    // 타일 이동 함수
    public IEnumerator MoveTile(int startX, int startY, int endX, int endY)
    {
        //Debug.Log($"타일 이동 함수 시작 {startX}, {startY} <-> {endX}, {endY}");
        isMoving = true;
        // 이동할 타일
        GameObject tileStart = tiles[startX, startY];
        GameObject tileEnd = tiles[endX, endY];

        // 이동할 위치
        Vector2 newPosition_start = new Vector2(endX * tileSize, endY * tileSize);
        Vector2 newPosition_end = new Vector2(startX * tileSize, startY * tileSize);


        // 이동 애니메이션 실행
        StartCoroutine(tileStart.GetComponent<Tile>().MoveTo(endX, endY));
        yield return StartCoroutine(tileEnd.GetComponent<Tile>().MoveTo(startX, startY));

        //애니메이션 끝날때까지 기다리기

        // 타일 배열에서 위치 변경
        GameObject tileTemp = tileStart;
        tiles[startX, startY] = tileEnd;
        tiles[endX, endY] = tileTemp;

        // 타일 인덱스 변경
        tileStart.GetComponent<Tile>().xIndex = endX;
        tileStart.GetComponent<Tile>().yIndex = endY;
        tileEnd.GetComponent<Tile>().xIndex = startX;
        tileEnd.GetComponent<Tile>().yIndex = startY;
        isMoving = false;
    }

    public IEnumerator CoDelayCheckMatches(int touchX, int touchY, Action<bool> callback)
    {
        //yield return new WaitForSeconds(seconds);
        callback(CheckMatches(touchX, touchY));
        yield return null;
    }

    public IEnumerator CoDelayCheckMatches(float seconds, int touchX = -1, int touchY = -1)
    {
        yield return new WaitForSeconds(seconds);
        CheckMatches(touchX, touchY);
    }

    public bool CheckSpecialTile(int startX, int startY, int endX, int endY)
    {
        // 이동할 타일
        GameObject tileStart = tiles[startX, startY];
        GameObject tileEnd = tiles[endX, endY];

        string type = "";
        if(tileStart.GetComponent<Tile>().GetTileType() == TileType.Special)
            type = tileEnd.GetComponent<Tile>().type;
        else if(tileEnd.GetComponent<Tile>().GetTileType() == TileType.Special)
            type = tileStart.GetComponent<Tile>().type;
        else
            return false;

        AddMatches(tileStart);
        AddMatches(tileEnd);
        for(int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if(tiles[x,y].GetComponent<Tile>().type == type)
                {
                    AddMatches(tiles[x,y]);
                }
            }
        }

        StartCoroutine(DestroyMatchTiles(true));

        return true;
    }

    List<GameObject> matches = new List<GameObject>();
    // 매치 확인 함수
    public bool CheckMatches(int touchX = -1, int touchY = -1)
    {   
        isMoving = true;
        matches.Clear();
        // 가로 매치 확인
        for (int y = 0; y < gridHeight; y++)
        {
            List<GameObject> rowTiles = new List<GameObject>();
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject tile = tiles[x, y];
                if (tile != null && rowTiles.Count == 0)
                {
                    rowTiles.Add(tile);
                }
                else if (tile != null && rowTiles.Count > 0 && tile.GetComponent<Tile>().type == rowTiles[0].GetComponent<Tile>().type)
                {
                    rowTiles.Add(tile);
                    //Debug.Log($"{x}, {y} : 가로 match 타일 추가 => {rowTiles.Count}");
                }
                else
                {
                    Check3Matches(true, rowTiles, touchX, touchY);
                    //Debug.Log("클리어하고 다시 추가");
                    rowTiles.Clear();
                    if (tile != null)
                    {
                        rowTiles.Add(tile);
                    }
                }
                if (x == gridWidth - 1)
                {
                    // 매치된 타일 제거
                    //Debug.Log($"매치된 타일 제거");
                    Check3Matches(true, rowTiles, touchX, touchY);
                }
            }
        }

        //세로 매치 확인
        for (int x = 0; x < gridWidth; x++)
        {
            List<GameObject> columnTiles = new List<GameObject>();
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject tile = tiles[x, y];
                if (tile != null && columnTiles.Count == 0)
                {
                    //Debug.Log("match 타일 추가1");
                    columnTiles.Add(tile);
                }
                else if (tile != null && columnTiles.Count > 0 && tile.GetComponent<Tile>().type == columnTiles[0].GetComponent<Tile>().type)
                {
                    columnTiles.Add(tile);
                    //Debug.Log($"{x}, {y} : 세로 match 타일 추가 => {columnTiles.Count}");
                }
                else
                {
                    Check3Matches(false, columnTiles, touchX, touchY);
                    columnTiles.Clear();
                    if (tile != null)
                    {
                        columnTiles.Add(tile);
                    }

                }
                if (y == gridHeight - 1)
                {
                    // 매치된 타일 제거
                    //Debug.Log($"매치된 타일 제거");
                    Check3Matches(false, columnTiles, touchX, touchY);
                }
            }
        }

        StartCoroutine(DestroyMatchTiles(false));

        return (matches.Count > 0) ? true : false;

    }

    public IEnumerator DestroyMatchTiles(bool isSpecial)
    {
        if (matches.Count > 0)
        {
            nullTiles = new int[gridWidth];
            matches = matches.Distinct().ToList();
            // 매치된 타일 제거
            foreach (GameObject matchTile in matches)
            {
                score++;
                Tile _tile = matchTile.GetComponent<Tile>();
                //matchTile type이 normal이여야 없앰. row, col, Special도 없애버리면 안만들어지고 없어지는 현상 생김.
                if(_tile.GetTileType() == TileType.Normal)
                {
                    //Debug.Log("매치된 타일 제거");
                    Destroy(matchTile);
                    StartCoroutine(effectManager.StartPopping(isSpecial, matchTile.transform.position));
                    //Debug.Log($"{_tile.xIndex}, {_tile.yIndex} 지움");
                    tiles[_tile.xIndex, _tile.yIndex] = null;
                    DropTilesofLine(_tile.xIndex);
                    // 새로운 타일 생성
                    CreateNewTiles(_tile);
                    nullTiles[_tile.xIndex]++;
                    yield return new WaitForSeconds(isSpecial? 0.02f : 0.0f);
                }
            }
            //DropTiles();
            // 새로운 타일 생성
            //CreateNewTiles(matches);

            // 점수 증가
            // TODO: 점수 증가 처리 구현

            // 매치된 타일이 없을 때까지 반복
            //Debug.Log("CheckMatches 다시");
            StartCoroutine(CoDelayCheckMatches(0.5f));
        }
        else
        {
            isMoving = false;
            if(_isTimerEnd)
            {
                Debug.Log("터질거 터지고 게임 종료"); 
                isMoving = true;
                GameOver();
            }
        }
    }

    public void Check3Matches(bool isRow, List<GameObject> tiles, int touchX, int touchY)
    {
            if (tiles.Count >= 5)
            {
                // 매치된 타일 제거
                //Debug.Log($"매치된 타일 제거");
                int cnt = 0;
                foreach (GameObject tile in tiles)
                {
                    AddMatches(tile); //matches.Add(rowTile);
                    if(cnt++ == 2)
                        tile.GetComponent<Tile>().SetTileType(TileType.Special);
                    else
                        tile.GetComponent<Tile>().SetTileType(TileType.Normal);
                }
            }
            else if (tiles.Count >= 4)
            {
                // 매치된 타일 제거
                //Debug.Log($"매치된 타일 제거");
                int cnt = 0;
                foreach (GameObject tile in tiles)
                {
                    AddMatches(tile); //matches.Add(rowTile);
                    if(isRow)
                    {
                        //Debug.Log($"가로 {tile.GetComponent<Tile>().xIndex},{tile.GetComponent<Tile>().yIndex}랑 {touchX}, {touchY} 비교 ");
                        if(touchX == -1 && cnt++ == 1)
                            tile.GetComponent<Tile>().SetTileType(TileType.Col);
                        else if(tile.GetComponent<Tile>().xIndex == touchX)
                            tile.GetComponent<Tile>().SetTileType(TileType.Col);
                        else
                            tile.GetComponent<Tile>().SetTileType(TileType.Normal);
                    }
                    else
                    {
                        //Debug.Log($"세로 {tile.GetComponent<Tile>().xIndex},{tile.GetComponent<Tile>().yIndex}랑 {touchX}, {touchY} 비교 ");
                        if(touchX == -1 && cnt++ == 2)
                            tile.GetComponent<Tile>().SetTileType(TileType.Row);
                        else if(tile.GetComponent<Tile>().yIndex == touchY)
                            tile.GetComponent<Tile>().SetTileType(TileType.Row);
                        else
                            tile.GetComponent<Tile>().SetTileType(TileType.Normal);
                    }
                
                }
            }
            else if (tiles.Count >= 3)
            {
                // 매치된 타일 제거
                //Debug.Log($"매치된 타일 제거");
                foreach (GameObject tile in tiles)
                {
                    AddMatches(tile); //matches.Add(rowTile);
                    tile.GetComponent<Tile>().SetTileType(TileType.Normal);
                }
            }   
    }

    public void AddMatches(GameObject tile)
    {
        //Debug.Log("매치 타일 추가");
        Tile _tile = tile.GetComponent<Tile>();
        switch(_tile.GetTileType())
        {
            case TileType.Normal : matches.Add(tiles[_tile.xIndex, _tile.yIndex]); break;
            case TileType.Row : 
                                for (int x = 0; x < gridWidth; x++)
                                {
                                    matches.Add(tiles[x, _tile.yIndex]);
                                    if(tiles[x, _tile.yIndex].GetComponent<Tile>().GetTileType() == TileType.Col)
                                    {
                                        //row라인 추가한건 normal로 바꿈
                                        AddMatches(tiles[x, _tile.yIndex]);
                                    }
                                    tiles[x, _tile.yIndex].GetComponent<Tile>().SetTileType(TileType.Normal);
                                } 
                                break;
            case TileType.Col : 
                                for (int y = 0; y < gridHeight; y++)
                                {
                                    matches.Add(tiles[_tile.xIndex, y]);
                                    if(tiles[_tile.xIndex, y].GetComponent<Tile>().GetTileType() == TileType.Row)
                                    {
                                        AddMatches(tiles[_tile.xIndex, y]);
                                    }
                                    //col라인 추가한건 normal로 바꿈
                                    tiles[_tile.xIndex, y].GetComponent<Tile>().SetTileType(TileType.Normal);
                                } 
                                break;
            case TileType.Special : matches.Add(tiles[_tile.xIndex, _tile.yIndex]);
                                    tiles[_tile.xIndex, _tile.yIndex].GetComponent<Tile>().SetTileType(TileType.Normal);break;
        }
    }

    public int[] nullTiles;

    public void DropTiles() 
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (tiles[x, y] == null)
                {
                    // 빈 자리에 채울 위쪽 타일 검사
                    for (int y2 = y + 1; y2 < gridHeight; y2++)
                    {
                        if (tiles[x, y2] != null)
                        {
                            // 위쪽 타일을 현재 위치로 이동
                            tiles[x, y] = tiles[x, y2];
                            tiles[x, y2] = null;
                            tiles[x, y].GetComponent<Tile>().xIndex = x;
                            tiles[x, y].GetComponent<Tile>().yIndex = y;
                            tiles[x, y].GetComponent<Tile>().MoveDown(x, y);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void DropTilesofLine(int x) 
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (tiles[x, y] == null)
            {
                // 빈 자리에 채울 위쪽 타일 검사
                for (int y2 = y + 1; y2 < gridHeight; y2++)
                {
                    if (tiles[x, y2] != null)
                    {
                        // 위쪽 타일을 현재 위치로 이동
                        tiles[x, y] = tiles[x, y2];
                        tiles[x, y2] = null;
                        tiles[x, y].GetComponent<Tile>().xIndex = x;
                        tiles[x, y].GetComponent<Tile>().yIndex = y;
                        tiles[x, y].GetComponent<Tile>().MoveDown(x, y);
                        break;
                    }
                }
            }
        }
    }


    // 새로운 타일 생성 함수
    public void CreateNewTiles(List<GameObject> matchedTiles)
    {
        int numNewTiles = matchedTiles.Count;
        for (int i = 0; i < numNewTiles; i++)
        {
            int x = matchedTiles[i].GetComponent<Tile>().xIndex;
            int y = gridHeight - nullTiles[x]--;
            //Debug.Log($"{x}, {y}에 생성");
            GameObject newTile = Instantiate(tilePrefabs[UnityEngine.Random.Range(0, tilePrefabs.Length)], transform);
            newTile.GetComponent<Tile>().xIndex = x;
            newTile.GetComponent<Tile>().yIndex = y;
            newTile.transform.position = new Vector2(x * tileSize, (y + gridHeight) * tileSize);
            tiles[x, y] = newTile;
            newTile.GetComponent<Tile>().MoveDown(x,y);
        }
    }

    public void CreateNewTiles(Tile tile)
    {
        int x = tile.xIndex;
        int y = 7;//gridHeight - nullTiles[x] - 1;
        //Debug.Log($"{x}, {y}에 생성");
        GameObject newTile = Instantiate(tilePrefabs[UnityEngine.Random.Range(0, tilePrefabs.Length)], transform);
        newTile.GetComponent<Tile>().xIndex = x;
        newTile.GetComponent<Tile>().yIndex = y;
        newTile.transform.position = new Vector2(x * tileSize, (y + gridHeight) * tileSize);
        tiles[x, y] = newTile;
        newTile.GetComponent<Tile>().MoveDown(x,y);
    }
    private void Update() {
        if(Input.GetKeyDown(KeyCode.D))
        {
            DebugTiles();
        }
    }
    public void DebugTiles()
    {
        string text = "";
        for (int y = gridHeight-1; y > 0; y--)
        {
           for (int x = 0; x < gridWidth; x++)
           {
                text += tiles[x,y].GetComponent<Tile>().type.ToString() + " ";
           } 
           text += "\n";
        }
        Debug.Log(text);
    }
}

           

