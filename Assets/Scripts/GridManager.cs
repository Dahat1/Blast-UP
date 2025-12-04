using UnityEngine;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    
    [Header("Intro/Outro Ayarları")]
    public GameObject singleBlockVisualPrefab; 
    public float introRowDelay = 0.05f; 
    public float outroBlockDelay = 0.05f; 

    public float gameOverFillDelay = 0.08f; 

    [Header("Patlama (Vakum) Efekti")]
    public float suckAnimationDuration = 0.3f; 
    public AnimationCurve suckCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); 

    public float spacing = 1f;

    public bool[,] gridStatus; 
    public Transform[,] occupiedBlocks; 
    public SpriteRenderer[,] cellRenderers; 
    
    public Vector2 startPos;

    [Header("Ses Efektleri")]
    public AudioClip explodeSound;

    [Header("Görsel Efektler")]
    public GameObject explosionPrefab; 

    private Dictionary<SpriteRenderer, Color> _originalColors = new Dictionary<SpriteRenderer, Color>();
    private GameObject[,] _introVisualBlocks;

    void Awake()
    {
        Instance = this;
        gridStatus = new bool[width, height];
        occupiedBlocks = new Transform[width, height];
        cellRenderers = new SpriteRenderer[width, height];
        _introVisualBlocks = new GameObject[width, height];
    }

    void Start()
    {
        GenerateGrid();
        
        if (singleBlockVisualPrefab != null)
        {
            StartCoroutine(PlayGridIntroAnimation());
        }
        else
        {
            if(GameManager.Instance != null) GameManager.Instance.StartActualGame();
        }
    }

    private IEnumerator PlayGridIntroAnimation()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 spawnPos = GridToWorld(new Vector2Int(x, y));
                GameObject visualBlock = Instantiate(singleBlockVisualPrefab, spawnPos, Quaternion.identity);
                visualBlock.transform.SetParent(transform);
                
                SpriteRenderer sr = visualBlock.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);

                _introVisualBlocks[x, y] = visualBlock;
            }
            yield return new WaitForSeconds(introRowDelay);
        }

        yield return new WaitForSeconds(0.3f);

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (_introVisualBlocks[x, y] != null)
                {
                    Destroy(_introVisualBlocks[x, y]);
                    _introVisualBlocks[x, y] = null;
                }
            }
            yield return new WaitForSeconds(introRowDelay);
        }

        yield return new WaitForSeconds(0.2f);
        
        if (GameManager.Instance != null) GameManager.Instance.StartActualGame();
    }

    public void PlayGameOverFillAnimation()
    {
        StartCoroutine(GameOverFillRoutine());
    }

    private IEnumerator GameOverFillRoutine()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (occupiedBlocks[x, y] != null)
                {
                    Destroy(occupiedBlocks[x, y].gameObject);
                    occupiedBlocks[x, y] = null;
                }

                Vector3 spawnPos = GridToWorld(new Vector2Int(x, y));
                GameObject visualBlock = Instantiate(singleBlockVisualPrefab, spawnPos, Quaternion.identity);
                visualBlock.transform.SetParent(transform);

                SpriteRenderer sr = visualBlock.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);
            }
            yield return new WaitForSeconds(gameOverFillDelay); 
        }

        yield return new WaitForSeconds(1.0f); 

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverPanel();
        }
    }

    public void PlayOutroAndLoadMenu()
    {
        StartCoroutine(OutroRoutine());
    }

    private IEnumerator OutroRoutine()
    {
        GameObject[,] tempOutroBlocks = new GameObject[width, height];
        Color targetColor = Color.white;
        if (ThemeManager.Instance != null) targetColor = ThemeManager.Instance.currentPalette.blockColor;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (occupiedBlocks[x, y] == null)
                {
                    Vector3 spawnPos = GridToWorld(new Vector2Int(x, y));
                    GameObject visualBlock = Instantiate(singleBlockVisualPrefab, spawnPos, Quaternion.identity);
                    visualBlock.transform.SetParent(transform);

                    SpriteRenderer sr = visualBlock.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = targetColor; 
                    tempOutroBlocks[x, y] = visualBlock;
                }
            }
            yield return new WaitForSeconds(outroBlockDelay); 
        }

        yield return new WaitForSeconds(0.2f); 

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                if (tempOutroBlocks[x, y] != null) Destroy(tempOutroBlocks[x, y]);
                if (occupiedBlocks[x, y] != null)
                {
                    Destroy(occupiedBlocks[x, y].gameObject);
                    occupiedBlocks[x, y] = null;
                }
            }
            yield return new WaitForSeconds(outroBlockDelay);
        }

        yield return new WaitForSeconds(0.2f); 
        SceneManager.LoadScene("MainMenu");
    }

    void GenerateGrid()
    {
        float boardWidth = width * spacing;
        float startX = -boardWidth / 2 + 0.5f;
        float startY = -2.5f; 
        startPos = new Vector2(startX, startY);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 spawnPos = new Vector2(x * spacing, y * spacing) + startPos;
                GameObject newCell = Instantiate(cellPrefab, spawnPos, Quaternion.identity);
                newCell.transform.SetParent(transform);
                newCell.name = $"Cell {x},{y}";

                SpriteRenderer sr = newCell.GetComponent<SpriteRenderer>();
                cellRenderers[x, y] = sr;

                if (ThemeManager.Instance != null && sr != null)
                {
                    sr.color = ThemeManager.Instance.currentPalette.gridColor;
                }
            }
        }
    }

    public void UpdateGridColors(Color newGridColor)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cellRenderers[x, y] != null) cellRenderers[x, y].color = newGridColor;
            }
        }
    }

    public void CheckLines()
    {
        ResetHighlights();
        List<int> rowsToClear = new List<int>();
        List<int> colsToClear = new List<int>();

        for (int y = 0; y < height; y++) { bool full = true; for (int x = 0; x < width; x++) if (!gridStatus[x, y]) { full = false; break; } if (full) rowsToClear.Add(y); }
        for (int x = 0; x < width; x++) { bool full = true; for (int y = 0; y < height; y++) if (!gridStatus[x, y]) { full = false; break; } if (full) colsToClear.Add(x); }

        int totalLinesCleared = rowsToClear.Count + colsToClear.Count;
        Vector3 centerOfExplosion = Vector3.zero;
        int blockCount = 0;

        foreach (int y in rowsToClear) for (int x = 0; x < width; x++) if (occupiedBlocks[x, y] != null) { centerOfExplosion += occupiedBlocks[x, y].position; blockCount++; }
        foreach (int x in colsToClear) for (int y = 0; y < height; y++) if (occupiedBlocks[x, y] != null) { centerOfExplosion += occupiedBlocks[x, y].position; blockCount++; }
        
        if (blockCount > 0) centerOfExplosion /= blockCount;
        else centerOfExplosion = transform.position;

        foreach (int y in rowsToClear) StartCoroutine(AnimateAndClearRow(y));
        foreach (int x in colsToClear) StartCoroutine(AnimateAndClearCol(x));

        if (totalLinesCleared > 0)
        {
            if (AudioManager.Instance != null) 
            { 
                AudioManager.Instance.PlaySFX(explodeSound); 
                AudioManager.Instance.VibrateHeavy(); 
            }
        }

        bool isAllClear = CheckIfAllClear(); 
        if (GameManager.Instance != null) GameManager.Instance.ProcessTurn(totalLinesCleared, isAllClear, centerOfExplosion);
    }

    private IEnumerator AnimateAndClearRow(int y)
    {
        List<GameObject> blocksToAnimate = new List<GameObject>();
        for (int x = 0; x < width; x++)
        {
            if (occupiedBlocks[x, y] != null)
            {
                blocksToAnimate.Add(occupiedBlocks[x, y].gameObject);
                gridStatus[x, y] = false;
                occupiedBlocks[x, y] = null;
            }
        }

        float timer = 0f;
        while (timer < suckAnimationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / suckAnimationDuration;
            float curveT = suckCurve.Evaluate(t);

            foreach (GameObject block in blocksToAnimate)
            {
                if (block == null) continue;
                float scaleY = Mathf.Lerp(1f, 0.05f, curveT);
                float scaleX = Mathf.Lerp(1f, 1.4f, curveT); 
                block.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(1f, 0f, curveT);
                    sr.color = c;
                }
            }
            yield return null;
        }

        foreach (GameObject block in blocksToAnimate)
        {
            if (block != null) Destroy(block);
        }
    }

    private IEnumerator AnimateAndClearCol(int x)
    {
        List<GameObject> blocksToAnimate = new List<GameObject>();
        for (int y = 0; y < height; y++)
        {
            if (occupiedBlocks[x, y] != null)
            {
                blocksToAnimate.Add(occupiedBlocks[x, y].gameObject);
                gridStatus[x, y] = false;
                occupiedBlocks[x, y] = null;
            }
        }

        float timer = 0f;
        while (timer < suckAnimationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / suckAnimationDuration;
            float curveT = suckCurve.Evaluate(t);

            foreach (GameObject block in blocksToAnimate)
            {
                if (block == null) continue;
                float scaleX = Mathf.Lerp(1f, 0.05f, curveT);
                float scaleY = Mathf.Lerp(1f, 1.4f, curveT);
                block.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(1f, 0f, curveT);
                    sr.color = c;
                }
            }
            yield return null;
        }

        foreach (GameObject block in blocksToAnimate)
        {
            if (block != null) Destroy(block);
        }
    }

    public void HighlightPotentialClears(List<Vector2Int> ghostCoords, Color indicatorColor)
    {
        ResetHighlights();
        HashSet<int> rowsToHighlight = new HashSet<int>();
        HashSet<int> colsToHighlight = new HashSet<int>();
        HashSet<int> affectedRows = new HashSet<int>();
        HashSet<int> affectedCols = new HashSet<int>();

        foreach (Vector2Int pos in ghostCoords)
        {
            if (IsValidPosition(pos)) 
            {
                affectedRows.Add(pos.y);
                affectedCols.Add(pos.x);
            }
        }

        foreach (int y in affectedRows)
        {
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                bool occupied = gridStatus[x, y] || ghostCoords.Contains(new Vector2Int(x, y));
                if (occupied) count++;
            }
            if (count == width) rowsToHighlight.Add(y);
        }

        foreach (int x in affectedCols)
        {
            int count = 0;
            for (int y = 0; y < height; y++)
            {
                bool occupied = gridStatus[x, y] || ghostCoords.Contains(new Vector2Int(x, y));
                if (occupied) count++;
            }
            if (count == height) colsToHighlight.Add(x);
        }

        if (rowsToHighlight.Count > 0 || colsToHighlight.Count > 0)
        {
            Color targetColor = indicatorColor;
            targetColor.a = 1.0f; 
            foreach (int y in rowsToHighlight) for (int x = 0; x < width; x++) ChangeColorTemporary(x, y, targetColor);
            foreach (int x in colsToHighlight) for (int y = 0; y < height; y++) ChangeColorTemporary(x, y, targetColor);
        }
    }

    void ChangeColorTemporary(int x, int y, Color color)
    {
        if (occupiedBlocks[x, y] != null)
        {
            SpriteRenderer blockSr = occupiedBlocks[x, y].GetComponent<SpriteRenderer>();
            if (blockSr != null)
            {
                if (!_originalColors.ContainsKey(blockSr)) _originalColors.Add(blockSr, blockSr.color);
                blockSr.color = color;
            }
        }
        if (cellRenderers[x, y] != null)
        {
            SpriteRenderer cellSr = cellRenderers[x, y];
            if (!_originalColors.ContainsKey(cellSr)) _originalColors.Add(cellSr, cellSr.color);
            cellSr.color = color;
        }
    }

    public void ResetHighlights()
    {
        foreach (var entry in _originalColors) if (entry.Key != null) entry.Key.color = entry.Value;
        _originalColors.Clear();
    }

    bool CheckIfAllClear()
    {
        for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) if (gridStatus[x, y]) return false;
        return true; 
    }

    // --- SİMÜLASYON KODLARI (Aynen Korundu) ---
    public bool CanShapeClearAnyLine(GameObject shapePrefab)
    {
        List<Vector2Int> offsets = new List<Vector2Int>();
        foreach (Transform child in shapePrefab.transform)
        {
            int x = Mathf.RoundToInt(child.localPosition.x);
            int y = Mathf.RoundToInt(child.localPosition.y);
            offsets.Add(new Vector2Int(x, y));
        }

        if (offsets.Count == 0) return false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CheckFitAtPosition(x, y, offsets))
                {
                    if (SimulateLineClear(x, y, offsets)) return true; 
                }
            }
        }
        return false; 
    }

    bool SimulateLineClear(int startX, int startY, List<Vector2Int> offsets)
    {
        HashSet<int> affectedRows = new HashSet<int>();
        HashSet<int> affectedCols = new HashSet<int>();

        foreach (Vector2Int offset in offsets)
        {
            int targetX = startX + offset.x;
            int targetY = startY + offset.y;
            affectedRows.Add(targetY);
            affectedCols.Add(targetX);
        }

        foreach (int r in affectedRows)
        {
            int count = 0;
            for (int c = 0; c < width; c++)
            {
                bool isFilledByGrid = gridStatus[c, r];
                bool isFilledByShape = false;
                foreach (Vector2Int off in offsets)
                {
                    if ((startX + off.x) == c && (startY + off.y) == r) { isFilledByShape = true; break; }
                }
                if (isFilledByGrid || isFilledByShape) count++;
            }
            if (count == width) return true; 
        }

        foreach (int c in affectedCols)
        {
            int count = 0;
            for (int r = 0; r < height; r++)
            {
                bool isFilledByGrid = gridStatus[c, r];
                bool isFilledByShape = false;
                foreach (Vector2Int off in offsets)
                {
                    if ((startX + off.x) == c && (startY + off.y) == r) { isFilledByShape = true; break; }
                }
                if (isFilledByGrid || isFilledByShape) count++;
            }
            if (count == height) return true; 
        }

        return false;
    }

    public bool CanShapeFitAnywhere(GameObject shape)
    {
         List<Vector2Int> blockOffsets = new List<Vector2Int>();
        foreach (Transform child in shape.transform)
        {
            int x = Mathf.RoundToInt(child.localPosition.x);
            int y = Mathf.RoundToInt(child.localPosition.y);
            blockOffsets.Add(new Vector2Int(x, y));
        }

        if (blockOffsets.Count == 0) return false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (CheckFitAtPosition(x, y, blockOffsets)) return true;
            }
        }
        return false;
    }

    bool CheckFitAtPosition(int startX, int startY, List<Vector2Int> offsets)
    {
        foreach (Vector2Int offset in offsets)
        {
            int targetX = startX + offset.x;
            int targetY = startY + offset.y;
            if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height) return false;
            if (gridStatus[targetX, targetY]) return false;
        }
        return true;
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - startPos.x) / spacing);
        int y = Mathf.RoundToInt((worldPosition.y - startPos.y) / spacing);
        return new Vector2Int(x, y);
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) return false;
        if (gridStatus[pos.x, pos.y] == true) return false;
        return true;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = startPos.x + (gridPos.x * spacing);
        float y = startPos.y + (gridPos.y * spacing);
        return new Vector3(x, y, 0);
    }

    // --- SAVE/LOAD İÇİN EKLENENLER (CLASS SONUNA) ---

    public List<bool> GetGridData()
    {
        List<bool> data = new List<bool>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                data.Add(gridStatus[x, y]);
            }
        }
        return data;
    }

    public void LoadGridFromData(List<bool> data)
    {
        // Temizle
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (occupiedBlocks[x, y] != null)
                {
                    Destroy(occupiedBlocks[x, y].gameObject);
                    occupiedBlocks[x, y] = null;
                }
                gridStatus[x, y] = false;
            }
        }

        // Yükle
        int index = 0;
        Color blockColor = Color.white;
        if(ThemeManager.Instance != null) blockColor = ThemeManager.Instance.currentPalette.blockColor;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (index < data.Count && data[index])
                {
                    Vector3 pos = GridToWorld(new Vector2Int(x, y));
                    if (singleBlockVisualPrefab != null)
                    {
                        GameObject blockObj = Instantiate(singleBlockVisualPrefab, pos, Quaternion.identity);
                        blockObj.transform.SetParent(transform);
                        
                        SpriteRenderer sr = blockObj.GetComponent<SpriteRenderer>();
                        if(sr != null) sr.color = blockColor;

                        occupiedBlocks[x, y] = blockObj.transform;
                        gridStatus[x, y] = true;
                    }
                }
                index++;
            }
        }
    }
}