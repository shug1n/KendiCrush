using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width, height;

    public GameObject tilePrefab;

    public JewManager[] jews;

    public JewManager[,] allJews;

    public float transitionSpeed;
 
    public MatchController matchController;

    public JewManager bomb;
    public float bombAppearChance = 2f;
    public int bombImpactArea;

    public enum boardStatus { waiting, moving };
    public boardStatus currentStatus = boardStatus.moving;

    private void Start()
    {
        allJews = new JewManager[width, height]; //Tüm mücevherlerimizin ebatýný belirliyoruz.

        OrganizingFonx();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            MixTheBoardFonx();
        }
    }

    void OrganizingFonx() //Mücevherlerin arka planlarýný oluþturup rastgele mücevher oluþturma fonksiyonunu baþlatýyoruz
    {
        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);

                GameObject tileObject = Instantiate(tilePrefab, pos, Quaternion.identity);

                tileObject.transform.parent = this.transform;

                tileObject.name = $"tileObject {x},{y}";

                int randomJew = Random.Range(0, jews.Length);

                while (ControllingMatchesAtStartFonx(new Vector2Int(x,y), jews[randomJew]))
                {
                    randomJew = Random.Range(0, jews.Length);
                }

                InstantiateJewsFonx(new Vector2Int(x,y), jews[randomJew]);
            }
        }
    }
    
    void InstantiateJewsFonx(Vector2Int pos, JewManager jewelToBeInstantiated)
    {
        if (Random.Range(0f, 101f) < bombAppearChance)
        {
            jewelToBeInstantiated = bomb;
        }

        JewManager localJew = Instantiate(jewelToBeInstantiated, new Vector2(pos.x,pos.y + height), Quaternion.identity);
        localJew.transform.parent = this.transform;
        localJew.name = $"Jewelery {pos.x}, {pos.y}";
        allJews[pos.x, pos.y] = localJew;
        localJew.OrganizeJewsFonx(pos, this);
    }

    bool ControllingMatchesAtStartFonx(Vector2Int posToBeChecked, JewManager theJewelWeControl)
    {
        if(posToBeChecked.x > 1)
        {
            if (allJews[posToBeChecked.x - 1, posToBeChecked.y].type == theJewelWeControl.type && allJews[posToBeChecked.x -2, posToBeChecked.y].type == theJewelWeControl.type)
            {
                return true;
            }
        }

        if(posToBeChecked.y > 1)
        {
            if(allJews[posToBeChecked.x, posToBeChecked.y - 1].type == theJewelWeControl.type && allJews[posToBeChecked.x, posToBeChecked.y -2].type == theJewelWeControl.type)
            {
                return true;
            }
        }
        return false;
    }

    public void DestroyMatchedJewelsFonx()
    {
        for(int i = 0; i < matchController.matchedJews.Count; i++)
        {
            if (matchController.matchedJews[i] != null)
            {
                Vector2Int pos = matchController.matchedJews[i].posIndex;
                if (allJews[pos.x, pos.y] != null)
                {
                    if (allJews[pos.x, pos.y].matched)
                    {
                        GameObject explosionEffect = Instantiate(allJews[pos.x, pos.y].explosionEffect, new Vector2(pos.x,pos.y), Quaternion.identity);
                        Destroy(explosionEffect, 1f);

                        UIManager.instance.IncreaseScoreFonx(allJews[pos.x, pos.y].scoreValue);
                        Destroy(allJews[pos.x, pos.y].gameObject);
                        allJews[pos.x, pos.y] = null;
                    }
                }

            }
        }

        StartCoroutine(DropJewelsCoroutine());
    }

    IEnumerator DropJewelsCoroutine()
    {
        yield return new WaitForSeconds(.2f);

        int emptinessCounter = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allJews[x, y] == null)
                {
                    emptinessCounter++;
                }
                else if (emptinessCounter > 0)
                {
                    allJews[x, y].posIndex.y -= emptinessCounter;
                    allJews[x, y - emptinessCounter] = allJews[x, y];
                    allJews[x, y] = null;
                }
            }


            emptinessCounter = 0;
        }

        yield return new WaitForSeconds(.5f);
        RefillTheBlanksFonx();

        yield return new WaitForSeconds(.5f);
        matchController.FindMatchesFonx();

        if (matchController.matchedJews.Count > 0)
        {
            DestroyMatchedJewelsFonx();  
        }

        else
        {
            yield return new WaitForSeconds(.5f);
            
            currentStatus = BoardManager.boardStatus.moving;

            int counter = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (ControllingMatchesAtStartFonx(new Vector2Int(x, y), allJews[x, y]))
                    {
                        counter++;
                    }
                }
            }

            if (counter != 0)
            {
                MixTheBoardFonx();
            }
        }
    }

    void RefillTheBlanksFonx()
    {
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allJews[x, y] == null)
                {
                    int randomJewel = Random.Range(0, jews.Length);

                    InstantiateJewsFonx(new Vector2Int(x, y), jews[randomJewel]);
                }
            }
        }
        DetectWrongPlacementsFonx();
    }

    void DetectWrongPlacementsFonx()
    {
        List <JewManager> foundJewels = new List<JewManager>();

        foundJewels.AddRange(FindObjectsOfType<JewManager>());

        for (int x = 0;x < width;x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foundJewels.Contains(allJews[x, y]))
                {
                    foundJewels.Remove(allJews[x, y]);
                }
            }
        }

        for(int i = 0; i < foundJewels.Count; i++)
        {
            Destroy(foundJewels[i].gameObject);
        }
    }

    public void FindMatchesIndirectFonx()
    {
        matchController.FindMatchesFonx();
    }

    public void FindBombFonx()
    {
        for(int i = 0; i < matchController.matchedJews.Count; i++)
        {
            JewManager matchedJew = matchController.matchedJews[i];

            int x = matchedJew.posIndex.x;
            int y = matchedJew.posIndex.y;

            if(x > 0)
            {
                if (allJews[x - 1, y] != null)
                {
                    if (allJews[x-1,y].type == JewManager.jewType.bomb) //Patlayan mücevherin solunda bomba var ise
                    {
                        MarkBombAreaFonx(new Vector2Int(x-1,y), allJews[x-1,y]);
                    }
                }
            }

            if (x < width - 1)
            {
                if (allJews[x + 1, y] != null)
                {
                    if (allJews[x + 1, y].type == JewManager.jewType.bomb) //Patlayan mücevherin saðýnda bomba var ise
                    {
                        MarkBombAreaFonx(new Vector2Int(x + 1, y), allJews[x + 1, y]);
                    }
                }
            }
            
            if (y > 0)
            {
                if (allJews[x, y-1] != null)
                {
                    if (allJews[x, y-1].type == JewManager.jewType.bomb) //Patlayan mücevherin altýnda bomba var ise
                    {
                        MarkBombAreaFonx(new Vector2Int(x, y -1), allJews[x, y -1]);
                    }
                }
            }

            if (y < height - 1)
            {
                if (allJews[x, y + 1] != null)
                {
                    if (allJews[x, y +1].type == JewManager.jewType.bomb) //Patlayan mücevherin üstünde bomba var ise
                    {
                        MarkBombAreaFonx(new Vector2Int(x, y + 1), allJews[x, y + 1]);
                    }
                }
            }
        }
    }

    void MarkBombAreaFonx(Vector2Int pos, JewManager bomb)
    {
        for (int x = pos.x - bombImpactArea; x <= pos.x + bombImpactArea; x++)
        {
            for(int y = pos.y - bombImpactArea; y<= pos.y + bombImpactArea; y++)
            {
                if(x >= 0 && y >= 0 && x < width && y < height)
                {
                    if (allJews[x, y] != null)
                    {
                        allJews[x, y].matched = true;
                        matchController.matchedJews.Add(allJews[x, y]);
                    }
                }
            }
        }

        if(matchController.matchedJews.Count > 0)
        {
            matchController.matchedJews = matchController.matchedJews.Distinct().ToList();  
        }
    }

    public void MixTheBoardFonx()
    {
        if(currentStatus != boardStatus.waiting)
        {
            currentStatus = boardStatus.waiting;

            List<JewManager> jewsOnTheBoard = new List<JewManager>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    jewsOnTheBoard.Add(allJews[x, y]);
                    allJews[x, y] = null;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int jewelNum = Random.Range(0, jewsOnTheBoard.Count);

                    while (ControllingMatchesAtStartFonx(new Vector2Int(x,y), jewsOnTheBoard[jewelNum]))
                    {
                        jewelNum = Random.Range(0, jewsOnTheBoard.Count);
                    }

                    jewsOnTheBoard[jewelNum].OrganizeJewsFonx(new Vector2Int(x, y), this);
                    allJews[x,y] = jewsOnTheBoard[jewelNum];
                    jewsOnTheBoard.RemoveAt(jewelNum);
                }
            }
        }
        currentStatus = boardStatus.moving;
    }
}
