using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if HBDOTween
using DG.Tweening;
#endif

public class Block : MonoBehaviour
{
    public Vector3 CurrentPos => new Vector3(currentPos.x,currentPos.y, 0);

    [SerializeField] private SpriteRenderer _blockPrefab;
    [SerializeField] private List<Sprite> _blockSprites;
    [SerializeField] private float _blockSpawnSize;

    private Vector3 startPos;
    private Vector3 previousPos;
    private Vector3 currentPos;
    private List<SpriteRenderer> blockSprites;
    private List<Vector2Int> blockPositions;

    private const int TOP = 1;
    private const int BOTTOM = 0;

    public void Init(List<Vector2Int> blocks,Vector3 start, int blockNum)
    {
        startPos = start;
        previousPos = start;
        currentPos = start;
        blockPositions = blocks;
        blockSprites = new List<SpriteRenderer>();
        for (int i = 0; i < blockPositions.Count; i++)
        {
            SpriteRenderer spawnedBlock = Instantiate(_blockPrefab, transform);
            spawnedBlock.sprite = _blockSprites[blockNum + 1];
            spawnedBlock.transform.localPosition = new Vector3(blockPositions[i].y,
                blockPositions[i].x, 0);
            blockSprites.Add(spawnedBlock);
        }
        transform.localScale = Vector3.one * _blockSpawnSize;
        ElevateSprites(true);
    }

    public void UpdatePos(Vector3 offset)
    {
        currentPos += offset;
        transform.position = currentPos;
    }

    public void ElevateSprites(bool reverse = false)
    {
        foreach (var blockSprite in blockSprites)
        {
            blockSprite.sortingOrder = reverse ? BOTTOM : TOP;
        }
    }

    public List<Vector2Int> BlockPositions()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (var pos in blockPositions)
        {
            result.Add(pos + new Vector2Int(
                Mathf.FloorToInt(currentPos.y),
                Mathf.FloorToInt(currentPos.x)
                ));
        }
        return result;
    }

    public void UpdateIncorrectMove()
    {
        currentPos = previousPos;
        transform.position = currentPos;
    }

    public void UpdateStartMove()
    {
        currentPos = startPos;
        previousPos = startPos;
        transform.position = currentPos;
    }

    public void UpdateCorrectMove()
    {
        currentPos.x = Mathf.FloorToInt(currentPos.x) + 0.5f;
        currentPos.y = Mathf.FloorToInt(currentPos.y) + 0.5f;
        previousPos = currentPos;
        transform.position = currentPos;
    }

    //Row Index of block.
    public int rowID;

    //Column Index of block.
    public int columnID;

    //Status whether block is empty or filled.
    [HideInInspector] public bool isFilled = false;

    //Block image instance.
    [HideInInspector] public Image blockImage;
    public int blockID = -1;

    //Bomb blast counter, will keep reducing with each move.
    [HideInInspector] public int bombCounter = 0;
    Text txtCounter;

    //Determines whether this block is normal or bomb.
    [HideInInspector] public bool isBomb = false;

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        //Counter will be used on Blast and challenge mode only.
        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
        {
            txtCounter = transform.GetChild(0).GetChild(0).GetComponent<Text>();
        }
    }

    /// <summary>
    /// Sets the highlight image.
    /// </summary>
    /// <param name="sprite">Sprite.</param>
    public void SetHighlightImage(Sprite sprite)
    {
        blockImage.sprite = sprite;
        blockImage.color = new Color(1, 1, 1, 0.5F);
    }

    /// <summary>
    /// Stops the highlighting.
    /// </summary>
    public void StopHighlighting()
    {
        blockImage.sprite = null;
        blockImage.color = new Color(1, 1, 1, 0);
    }

    /// <summary>
    /// Sets the block image.
    /// </summary>
    /// <param name="sprite">Sprite.</param>
    /// <param name="_blockID">Block I.</param>
    public void SetBlockImage(Sprite sprite, int _blockID)
    {
        blockImage.sprite = sprite;
        blockImage.color = new Color(1, 1, 1, 1);
        blockID = _blockID;
        isFilled = true;
    }

    /// <summary>
    /// Converts to filled block.
    /// </summary>
    /// <param name="_blockID">Block I.</param>
    public void ConvertToFilledBlock(int _blockID)
    {
        blockImage.sprite = BlockShapeSpawner.Instance.ActiveShapeBlockModule.ShapeBlocks.Find(o => o.BlockID == _blockID).shapeBlock.transform.GetChild(0).GetComponent<Image>().sprite;
        blockImage.color = new Color(1, 1, 1, 1);
        blockID = _blockID;
        isFilled = true;
    }

    #region bomb mode specific
    /// <summary>
    /// Converts to bomb.
    /// </summary>
    /// <param name="counterValue">Counter value.</param>
    public void ConvertToBomb(int counterValue = 9)
    {
        blockImage.sprite = GamePlay.Instance.BombSprite;
        blockImage.color = new Color(1, 1, 1, 1);
        isFilled = true;
        isBomb = true;
        SetCounter(counterValue);
    }

    /// <summary>
    /// Sets the counter.
    /// </summary>
    /// <param name="counterValue">Counter value.</param>
    public void SetCounter(int counterValue = 9)
    {
        txtCounter.gameObject.SetActive(true);
        txtCounter.text = counterValue.ToString();
        bombCounter = counterValue;
    }

    /// <summary>
    /// Decreases the counter.
    /// </summary>
    public void DecreaseCounter()
    {
        bombCounter -= 1;
        txtCounter.text = bombCounter.ToString();

        if (bombCounter == 0)
        {
            GamePlay.Instance.OnBombCounterOver();
        }
    }

    /// <summary>
    /// Removes the counter.
    /// </summary>
    void RemoveCounter()
    {
        txtCounter.text = "";
        txtCounter.gameObject.SetActive(false);
        bombCounter = 0;
        isBomb = false;
    }
    #endregion

    /// <summary>
    /// Clears the block.
    /// </summary>
    public void ClearBlock()
    {
        transform.GetComponent<Image>().color = new Color(1, 1, 1, 0);
#if HBDOTween
		blockImage.transform.DOScale (Vector3.zero, 0.35F).OnComplete (() => 
		{
			blockImage.transform.localScale = Vector3.one;
			blockImage.sprite = null;
		});

		transform.GetComponent<Image> ().DOFade (1, 0.35F).SetDelay (0.3F);
		blockImage.DOFade(0,0.3F);
#endif

        blockID = -1;
        isFilled = false;

        if (GameController.gameMode == GameMode.BLAST || GameController.gameMode == GameMode.CHALLENGE)
        {
            RemoveCounter();
        }
    }
}