using UnityEngine;
using System.Collections.Generic; 

public class ShapeMovement : MonoBehaviour
{
    private Vector3 _startPosition;
    private Vector3 _initialScale;
    private bool _isDragging = false;
    
    [Header("Dokunma Ayarları")]
    public float dragOffset = 3.0f; 

    [Header("Ses Efektleri")]
    public AudioClip pickUpSound; 
    public AudioClip placeSound;  

    private GameObject _ghostObject;
    private bool _canSnap = false;

    private Color _myColor = Color.white;

    void Start()
    {
        _startPosition = transform.position;
        
        // --- DÜZELTME BURADA ---
        // Eskiden "transform.localScale" diyorduk, o an 0 olduğu için hata oluyordu.
        // Artık "Normal boyutum 0.5'tir" diye elle sabitliyoruz.
        _initialScale = Vector3.one * 0.5f; 
        // -----------------------

        RefreshColor(); 
    }

    void RefreshColor()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) _myColor = sr.color;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (UIManager.Instance != null && GameManager.Instance != null && GameManager.Instance.isPaused) return;

        RefreshColor(); 
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(pickUpSound);
        
        _isDragging = true;

        UpdatePositionWithOffset();
        
        // Tutunca büyüt (1.0f)
        transform.localScale = Vector3.one; 
        
        SetSortingOrder(20); 
        
        CreateGhost();
        UpdateGhostPosition(); 
    }

    void OnMouseDrag()
    {
        if (_isDragging)
        {
            UpdatePositionWithOffset();
            UpdateGhostPosition();
        }
    }

    void UpdatePositionWithOffset()
    {
        Vector3 mousePos = GetMouseWorldPos();
        transform.position = new Vector3(mousePos.x, mousePos.y + dragOffset, 0f);
    }

    void OnMouseUp()
    {
        if (!_isDragging) return;

        _isDragging = false;
        
        SetSortingOrder(5); // Bırakınca sıra düzelsin
        
        DestroyGhost();

        if (GridManager.Instance != null) GridManager.Instance.ResetHighlights();

        if (GridManager.Instance == null) { ResetPosition(); return; }

        if (_canSnap)
        {
            SnapAndPlace();
        }
        else
        {
            ResetPosition();
        }
    }

    void CreateGhost()
    {
        _ghostObject = Instantiate(gameObject);
        _ghostObject.name = "GhostShape";
        _ghostObject.SetActive(false); 

        Destroy(_ghostObject.GetComponent<ShapeMovement>());
        Destroy(_ghostObject.GetComponent<Collider2D>());

        SpriteRenderer[] renderers = _ghostObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = 0.5f; 
            sr.color = c;
            sr.sortingOrder = 15;
        }
        _ghostObject.transform.localScale = Vector3.one;
    }

    void UpdateGhostPosition()
    {
        if (GridManager.Instance == null || _ghostObject == null) return;

        _canSnap = CanPlaceShape();

        if (_canSnap)
        {
            if (!_ghostObject.activeSelf) _ghostObject.SetActive(true);

            Transform firstChild = transform.GetChild(0);
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(firstChild.position);
            Vector3 targetWorldPos = GridManager.Instance.GridToWorld(gridPos);
            Vector3 snapOffset = targetWorldPos - firstChild.position;
            _ghostObject.transform.position = transform.position + snapOffset;

            List<Vector2Int> ghostCoords = new List<Vector2Int>();
            foreach(Transform ghostChild in _ghostObject.transform)
            {
                ghostCoords.Add(GridManager.Instance.WorldToGrid(ghostChild.position));
            }
            
            Color indicatorColor = Color.white;
            if (ThemeManager.Instance != null)
            {
                indicatorColor = ThemeManager.Instance.currentPalette.explosionColor;
            }
            
            GridManager.Instance.HighlightPotentialClears(ghostCoords, indicatorColor);
        }
        else
        {
            if (_ghostObject.activeSelf) _ghostObject.SetActive(false);
            if(GridManager.Instance != null) GridManager.Instance.ResetHighlights();
        }
    }

    void DestroyGhost()
    {
        if (_ghostObject != null) Destroy(_ghostObject);
    }

    bool CanPlaceShape()
    {
        foreach (Transform child in transform) {
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(child.position);
            if (!GridManager.Instance.IsValidPosition(gridPos)) return false;
        }
        return true;
    }

    void SnapAndPlace()
    {
        Transform firstChild = transform.GetChild(0);
        Vector2Int gridPos = GridManager.Instance.WorldToGrid(firstChild.position);
        Vector3 targetWorldPos = GridManager.Instance.GridToWorld(gridPos);
        Vector3 snapOffset = targetWorldPos - firstChild.position;
        
        transform.position += snapOffset;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LastShapePosition = transform.position;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(placeSound);
            AudioManager.Instance.VibrateLight(); 
        }

        System.Collections.Generic.List<Transform> children = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in transform) children.Add(child);

        foreach (Transform child in children)
        {
            Vector2Int finalGridPos = GridManager.Instance.WorldToGrid(child.position);
            
            GridManager.Instance.gridStatus[finalGridPos.x, finalGridPos.y] = true;
            GridManager.Instance.occupiedBlocks[finalGridPos.x, finalGridPos.y] = child;
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(1); 
            }

            child.SetParent(null); 
        }

        transform.SetParent(null); 
        Destroy(gameObject);

        GridManager.Instance.CheckLines();

        if (ShapeSpawner.Instance != null)
        {
            ShapeSpawner.Instance.CheckIfFieldIsEmpty();
        }
    }

    void ResetPosition()
    {
        transform.position = _startPosition;
        // Geri dönerken normal boyuta (0.5f) dön
        transform.localScale = _initialScale; 
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void SetSortingOrder(int order)
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = order;
        }
    }
}