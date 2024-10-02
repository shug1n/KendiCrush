using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchController : MonoBehaviour
{
    [SerializeField]
    BoardManager board;

    public List<JewManager> matchedJews = new List<JewManager>();

    public void FindMatchesFonx()
    {
        matchedJews.Clear();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                JewManager jew = board.allJews[x, y];
                
                
                if (x > 0 && x < board.width -1)
                {
                    JewManager leftJew = board.allJews[x - 1, y];
                    JewManager rightJew = board.allJews[x + 1, y];

                    if (leftJew != null && rightJew != null && jew != null)
                    {
                        if (leftJew.type == jew.type && rightJew.type == jew.type)
                        {
                            jew.matched = true;
                            leftJew.matched = true;
                            rightJew.matched = true;

                            matchedJews.Add(jew);
                            matchedJews.Add(leftJew);
                            matchedJews.Add(rightJew);
                        }

                    }
                }
                
                if (y > 0 && y < board.height -1)
                {
                    JewManager upJew = board.allJews[x, y + 1];
                    JewManager downJew = board.allJews[x, y - 1];

                    if (upJew != null && downJew != null && jew != null)
                    {
                        if (upJew.type == jew.type && downJew.type == jew.type)
                        {
                            jew.matched = true;
                            upJew.matched = true;
                            downJew.matched = true;

                            matchedJews.Add(jew);
                            matchedJews.Add(upJew);
                            matchedJews.Add(downJew);
                        }
                    }
                }
            }
        } //Loops are over
        if (matchedJews.Count > 0)
        {
            matchedJews = matchedJews.Distinct().ToList();
        }

        board.FindBombFonx();
    }
}
