using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(SpriteRenderer))]
public class GemBase : MonoBehaviour, ITouchHandler {
    
    SpriteRenderer spr;
    
    [SerializeField]
    GemType _type;
    public GemType type {
        get { return _type; }
        set {
            _type = value;
            spr.sprite = GameController.gameData.GetGemSprite(value);
        }
    }

    public Vector2Int position;
    
    void Awake() {
        spr = GetComponent<SpriteRenderer>();
    }

    public void SetPosition(int x, int y) {
        int sizeBoard = GameController.instance.sizeBoard;

        position = new Vector2Int(x, y);
        transform.position = new Vector2( x - ((sizeBoard/2) - 0.5f), y - ((sizeBoard/2) - 0.5f));
    }

    public void TouchDown() {
        // Debug.Log($"{type}, {position}");
    }

    public void TouchDrag() {
        if(Vector2.Distance(transform.position, TouchController.touchPosition) > 0.75f) {

            Vector2 delta = TouchController.touchPosition - transform.position;
            Vector2Int toPosition;

            if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {

                int newX = (int) (position.x + Mathf.Sign(delta.x));

                toPosition = new Vector2Int(
                    newX,
                    position.y
                );
            } else {
                
                int newY = (int) (position.y + Mathf.Sign(delta.y));

                toPosition = new Vector2Int(
                    position.x,
                    newY
                );
            }

            Vector2Int lastPosition = position;
            Debug.Log($"BSwap{position}");
            if(GameController.SwapGems(position, toPosition)) {
                
            Debug.Log($"ASwap{position}");
                if(!GameController.HasMatch(position, type) &&
                   !GameController.HasMatch(lastPosition, GameController.GetGem(lastPosition).type)) {
                       GameController.SwapGems(position, lastPosition);
                } else {
                    List<GemBase> matches = GameController.Match(position, type);
                    matches.AddRange(GameController.Match(lastPosition, GameController.GetGem(lastPosition).type));
                    foreach(GemBase g in matches)
                        g.GetComponent<SpriteRenderer>().color = Color.black;
                }
            }

            TouchUp();
        }
            
    }

    public void TouchUp() {
        TouchController.ClearElementClicked();
    }
}
