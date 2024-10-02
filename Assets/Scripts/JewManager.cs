using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewManager : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int posIndex, firstPos;

    [HideInInspector]
    public BoardManager board;

    public Vector2 firstPressedPos, lastPressedPos;

    bool mousePressed;

    float draggingAngle;

    JewManager otherJew;

    public enum jewType {blue, green, darkGreen, pink, yellow, bomb};

    public jewType type;

    public bool matched;

    public GameObject explosionEffect;

    public int scoreValue;

    private void Update()
    {
        if(Vector2.Distance(transform.position, posIndex) != 0)
        {
            transform.position = Vector2.Lerp(transform.position, posIndex, Time.deltaTime * board.transitionSpeed);
        }
        else
        {
            transform.position = new Vector2(posIndex.x, posIndex.y);
        }

        if (mousePressed && Input.GetMouseButtonUp(0))
        {
            mousePressed = false;
            lastPressedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngleFonx();
        }
    }

    public void OrganizeJewsFonx(Vector2Int pos, BoardManager localBoard)
    {
        posIndex = pos;

        board = localBoard;
    }

    private void OnMouseDown() //Mouse sol týka basýldýðý zaman
    {
        if(board.currentStatus == BoardManager.boardStatus.moving && !UIManager.instance.episodeEnded)
        {
            firstPos = posIndex;
            firstPressedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePressed = true;
        }
    }

    void CalculateAngleFonx()
    {
        float dx = lastPressedPos.x - firstPressedPos.x;

        float dy = lastPressedPos.y - firstPressedPos.y;

        draggingAngle = Mathf.Atan2(dy, dx); //Elde ettiðimiz sonuç radyan cinsindendir, açýya dönüþtürmek için 180 ile çarpýp pi'ye bölüyoruz

        draggingAngle = (draggingAngle * 180) / Mathf.PI;

        if(draggingAngle < 0)
        {
            draggingAngle += 360;
        }

        if (Vector3.Distance(lastPressedPos, firstPressedPos) > .5f)
        {
            MoveTheTileFonx();
        }
    }

    void MoveTheTileFonx()
    {
        firstPos = posIndex;

        if (draggingAngle < 45 || draggingAngle > 315 && posIndex.x < board.width - 1) // Saða sürüklenirse
        {
            otherJew = board.allJews[(posIndex.x + 1), posIndex.y];
            otherJew.posIndex.x--;

            posIndex.x++;
        }

        if (draggingAngle > 45 && draggingAngle < 135 && posIndex.y < board.height - 1) // Yukarý sürüklenirse
        {
            otherJew = board.allJews[(posIndex.x), posIndex.y +1];
            otherJew.posIndex.y--;

            posIndex.y++;
        }

        if (draggingAngle > 225 && draggingAngle < 315 && posIndex.y > 0) //Aþaðý sürüklenirse
        {
            otherJew = board.allJews[(posIndex.x), posIndex.y -1];
            otherJew.posIndex.y++;

            posIndex.y--;
        }

        if (draggingAngle > 135 && draggingAngle < 225 && posIndex.x >0) // Sola sürüklenirse
        {
            otherJew = board.allJews[(posIndex.x-1), posIndex.y];
            otherJew.posIndex.x++;

            posIndex.x--;
        }

        board.allJews[posIndex.x, posIndex.y] = this;
        board.allJews[otherJew.posIndex.x, otherJew.posIndex.y] = otherJew;

        StartCoroutine(CheckMovementCoroutine());
    }

    public IEnumerator CheckMovementCoroutine()
    {
        board.currentStatus = BoardManager.boardStatus.waiting;

        board.FindMatchesIndirectFonx();

        yield return new WaitForSeconds(.5f);

        if(matched || otherJew.matched)
        {
            board.DestroyMatchedJewelsFonx();
        }
        else
        {
            otherJew.posIndex = posIndex;
            this.posIndex = firstPos;

            board.allJews[posIndex.x, posIndex.y] = this;
            board.allJews[otherJew.posIndex.x, otherJew.posIndex.y] = otherJew;

            yield return new WaitForSeconds(.5f);
            board.currentStatus = BoardManager.boardStatus.moving;
        }
    }


}
