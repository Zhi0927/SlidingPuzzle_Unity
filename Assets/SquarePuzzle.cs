using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

/*
 Create an empty object on canvas and attach this script to it
*/

[RequireComponent(typeof(RectTransform))]
public class SquarePuzzle : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private Texture2D   Texture     = null;
    [SerializeField]
    private int         GridSize    = 3;


    private float       SquareSize  = 300;
    private SquarePool  SquarePool  = null;

    #endregion

    #region  Unity Methods

    void Awake()
    {
        SquareSize                  = Mathf.Min(Screen.width, Screen.height) * 0.3f;
        RectTransform RectTrans     = GetComponent<RectTransform>();
        RectTrans.anchorMin         = new Vector2(0, 0);
        RectTrans.anchorMax         = new Vector2(0, 0);
        RectTrans.pivot             = new Vector2(0.5f, 0.5f);
        RectTrans.sizeDelta         = new Vector2(SquareSize * GridSize, SquareSize * GridSize);
        RectTrans.anchoredPosition  = new Vector2(Screen.width / 2, Screen.height /2);
    }

    void Start()
    {
        SquarePool = new SquarePool(GridSize, SquareSize, Texture, this.transform);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var TargetObject = IsPointerOverUIObject(Input.mousePosition);
            if (TargetObject != null)
            {
                SquarePool.Swap(TargetObject);
            }
        }
    }

    void OnGUI()
    {
        GUIStyle ButtonSkin  = new GUIStyle(GUI.skin.button);
        ButtonSkin.fontSize  = 30;
        ButtonSkin.fontStyle = FontStyle.Bold;
        ButtonSkin.alignment = TextAnchor.MiddleCenter;

        if (GUI.Button(new Rect(75, 50, 200, 70), "SKIP", ButtonSkin))
        {
            SquarePool.CorrentPool();
        }
        if (GUI.Button(new Rect(75, 150, 200, 70), "RESTART", ButtonSkin))
        {
            SquarePool.RandomPool();
        }

        if (SquarePool.CheckPass())
        {
            GUIStyle LabelSkin          = new GUIStyle(GUI.skin.label);
            LabelSkin.fontSize          = 500;
            LabelSkin.fontStyle         = FontStyle.Bold;
            LabelSkin.normal.textColor  = Color.red;
            LabelSkin.alignment         = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Pass", LabelSkin);
        }
    }

    #endregion

    #region Private Methods

    private GameObject IsPointerOverUIObject(Vector3 mousePosition)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position         = new Vector2(mousePosition.x, mousePosition.y);
        List<RaycastResult> results               = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (results.Count == 0)
        {
            return null;
        }
        return results[0].gameObject;
    }

    #endregion
}

public struct Point
{
    #region Fields

    public int row;
    public int col;

    #endregion

    #region Constructors
    public Point(int _row, int _col)
    {
        row = _row;
        col = _col;
    }

    public Point(Point _point)
    {
        row = _point.row;
        col = _point.col;
    }

    #endregion

    #region Public Methods

    public Point Up()
    {
        return new Point(row - 1, col);
    }

    public Point Down()
    {
        return new Point(row + 1, col);
    }

    public Point Left()
    {
        return new Point(row, col - 1);
    }

    public Point Right()
    {
        return new Point(row, col + 1);
    }

    #endregion

    #region Extension Methods

    public static Point operator -(Point lhs, Point rhs)
    {
        int distanceRow = Mathf.Abs(lhs.row - rhs.row);
        int distanceCol = Mathf.Abs(lhs.col - rhs.col);

        return new Point(distanceRow, distanceCol);
    }

    public static bool operator ==(Point lhs, Point rhs)
    {
        return (lhs.row == rhs.row) && (lhs.col == rhs.col);
    }

    public static bool operator !=(Point lhs, Point rhs)
    {
        return !(lhs == rhs);
    }

    #endregion
}

public class SquareObject
{
    #region Fields

    private int         m_Index;
    private GameObject  m_Object;
    private float       m_SquareSize;
    private Point       m_OriginMatrixIndex;
    private Point       m_CurrentMatrixIndex;
    private bool        m_IsBlank;

    #endregion

    #region Properties

    public Point CurrentPoint
    {
        set
        {
            m_CurrentMatrixIndex = value;
            m_Object.GetComponent<RectTransform>().anchoredPosition = new Vector2(value.col * m_SquareSize, -value.row * m_SquareSize);
        }
        get
        {
            return m_CurrentMatrixIndex;
        }
    }

    public int      GetIndex        => m_Index;
    public bool     GetIsBlank      => m_IsBlank;
    public Vector2  GetPosition     => m_Object.GetComponent<RectTransform>().anchoredPosition;

    #endregion

    #region Constructors

    public SquareObject(int index, float squareSize, Point pointIndex, Texture2D texture, Transform parent = null)
    {
        m_Index                         = index;
        m_SquareSize                    = squareSize;
        m_OriginMatrixIndex             = pointIndex;
        m_IsBlank                       = false;

        m_Object                        = new GameObject();
        m_Object.name                   = index.ToString();
        Image ImageComponent            = m_Object.AddComponent<Image>();
        ImageComponent.sprite           = texture.ToSprite();
        if (parent != null)
            m_Object.transform.parent   = parent;

        RectTransform RectTrans         = m_Object.GetComponent<RectTransform>();
        RectTrans.anchorMin             = new Vector2(0, 1);
        RectTrans.anchorMax             = new Vector2(0, 1);
        RectTrans.pivot                 = new Vector2(0, 1);
        RectTrans.sizeDelta             = new Vector2(squareSize, squareSize);

        CurrentPoint                    = pointIndex;
    }
    #endregion

    #region Public Methods

    public void SetActive(bool value)
    {
        m_IsBlank = !value;
        m_Object.SetActive(value);
    }

    public void SetOrigin()
    {
        CurrentPoint = m_OriginMatrixIndex;
        if (m_IsBlank == true)
        {
            SetActive(true);
        }
    }

    public bool CheckStatus()
    {
        return CurrentPoint == m_OriginMatrixIndex;
    }

    #endregion

    #region Extension Methods

    public static Vector2 operator -(SquareObject lhs, SquareObject rhs)
    {
        return lhs.GetPosition - rhs.GetPosition;
    }

    public static Vector2 operator -(SquareObject lhs, Vector2 position)
    {
        return lhs.GetPosition - position;
    }

    #endregion
}

public class SquarePool
{
    #region Fields

    private SquareObject[]  m_MainPool          = null;
    private int             m_GridSize          = 0;
    private float           m_SquareSize        = 0;
    private Point           m_BlankPoint        = new Point(0, 0);
    private bool            m_LockOperate       = false;

    private const int       RandomIteration     = 20;

    #endregion

    #region Properties

    public Point GetBlankPoint => m_BlankPoint;

    #endregion

    #region Constructors

    public SquarePool(int gridSize, float squareSize, Texture2D texture, Transform parent)
    {
        m_GridSize   = gridSize;
        m_SquareSize = squareSize;
        m_MainPool   = new SquareObject[m_GridSize * m_GridSize];

        int sizeLess = Mathf.Min(texture.width, texture.height);
        int MainSize = sizeLess - (sizeLess % m_GridSize);
        int SepSize  = sizeLess / m_GridSize;

        int leftTopX = (texture.width - MainSize) / 2;
        int leftTopY = (texture.height - MainSize) / 2;

        Texture2D MainTexture = texture.CropTexture(new Rect(leftTopX, leftTopY, MainSize, MainSize));

        int index = 0;
        for (int i = 0; i < m_GridSize; i++)
        {
            for (int j = 0; j < m_GridSize; j++)
            {
                Texture2D sliceTexture    = MainTexture.CropTexture(new Rect(j * SepSize, (m_GridSize - 1 - i) * SepSize, SepSize, SepSize));
                SquareObject squareObject = new SquareObject(index, m_SquareSize, new Point(i, j), sliceTexture, parent);
                m_MainPool[index]         = squareObject;
                index += 1;
            }
        }
        RandomPool();
    }

    #endregion

    #region Public Methods

    public SquareObject At(Point point)
    {
        return Array.Find(m_MainPool, element => element.CurrentPoint == point);
    }

    public bool[] GetValidDir(Point point)
    {
        var vaildDirction = new bool[4] { false, false, false, false };
        vaildDirction[0]  = point.row > 0                ? true : false;
        vaildDirction[1]  = point.row < (m_GridSize - 1) ? true : false;
        vaildDirction[2]  = point.col > 0                ? true : false;
        vaildDirction[3]  = point.col < (m_GridSize - 1) ? true : false;

        return vaildDirction;
    }

    public void Swap(Point target)
    {
        if (IsValidMove(target) == false)
        {
            return;
        }

        (At(m_BlankPoint).CurrentPoint, At(target).CurrentPoint) = (At(target).CurrentPoint, At(m_BlankPoint).CurrentPoint);
        m_BlankPoint = target;
    }

    public void Swap(GameObject targetObject)
    {
        if (m_LockOperate)
        {
            return;
        }

        Vector2 Anchor = targetObject.GetComponent<RectTransform>().anchoredPosition;
        Point Target   = new Point((int)(-Anchor.y / m_SquareSize), (int)(Anchor.x / m_SquareSize));
        Swap(Target);
    }

    public void RandomPool()
    {
        m_LockOperate = false;

        for (int time = 0; time < RandomIteration; time++)
        {
            var ValidDirs = GetValidDir(m_BlankPoint);

            int randomDirIndex;
            do
            {
                randomDirIndex = UnityEngine.Random.Range(0, 4);
            } while (ValidDirs[randomDirIndex] == false);


            Point Target = new Point(m_BlankPoint);
            switch (randomDirIndex)
            {
                case 0:
                    Target = m_BlankPoint.Up();
                    break;
                case 1:
                    Target = m_BlankPoint.Down();
                    break;
                case 2:
                    Target = m_BlankPoint.Left();
                    break;
                case 3:
                    Target = m_BlankPoint.Right();
                    break;
            }

            Swap(Target);
        }

        if (At(m_BlankPoint).GetIsBlank == false)
        {
            At(m_BlankPoint).SetActive(false);
        }
    }

    public void CorrentPool()
    {
        foreach (var item in m_MainPool)
        {
            item.SetOrigin();
        }

        m_LockOperate = true;
    }

    public bool IsValidMove(Point point)
    {
        Point diff = point - m_BlankPoint;
        return (diff.row + diff.col) == 1;
    }

    public bool CheckPass()
    {
        foreach (var item in m_MainPool)
        {
            if (item.CheckStatus() == false)
            {
                return false;
            }
        }

        m_LockOperate = true;
        At(m_BlankPoint).SetActive(true);
        return true;
    }

    #endregion
}

public static class Extension
{
    #region Extension Methods

    public static Texture2D CropTexture(this Texture2D texture, Rect rect)
    {
        var CropPixel   = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        var CropTexture = new Texture2D((int)rect.width, (int)rect.height);
        CropTexture.SetPixels(CropPixel);
        CropTexture.Apply();
        return CropTexture;
    }

    public static Sprite ToSprite(this Texture2D texture)
    {
        var rect   = new Rect(0, 0, texture.width, texture.height);
        var pivot  = Vector2.one * 0.5f;
        var sprite = Sprite.Create(texture, rect, pivot);

        return sprite;
    }

    #endregion
}
