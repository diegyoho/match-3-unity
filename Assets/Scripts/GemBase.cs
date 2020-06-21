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

    public void SetPosition(Vector2Int position) {
        this.position = position;
        GameController.instance.gemBoard[position.x, position.y] = this;
    }

    public void MoveTo(Vector2Int position) {
        transform.position = GameController.GetWorldPosition(position);
    }

    public IEnumerator IEMoveTo(Vector3 target, float duration) {
        
        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        float time = 0;

        while(time < duration) {
            transform.position += direction * ((Time.deltaTime * distance)/duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

    }

    public void TouchDown() {
        // Debug.Log($"{type}, {position}");
    }

    public void TouchDrag() {
        if(Vector2.Distance(transform.position, TouchController.touchPosition) > 0.75f) {

            Vector2 delta = TouchController.touchPosition - transform.position;
            GemBase otherGem;

            if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {

                int swapX = (int) (position.x + Mathf.Sign(delta.x));

                if(swapX < 0 || swapX >= GameController.instance.sizeBoardX) {
                    TouchUp();
                    return;
                }

                otherGem = GameController.instance.gemBoard[swapX, position.y];
            } else {
                
                int swapY = (int) (position.y + Mathf.Sign(delta.y));

                if(swapY < 0 || swapY >= GameController.instance.sizeBoardY) {
                    TouchUp();
                    return;
                }

                otherGem = GameController.instance.gemBoard[position.x, swapY];
            }

            if(otherGem) {
                GameController.TryMatch(this, otherGem);
            }

            TouchUp();
        }
            
    }

    public void TouchUp() {
        TouchController.ClearElementClicked();
    }
}
