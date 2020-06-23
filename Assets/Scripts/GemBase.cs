using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

[RequireComponent(typeof(SpriteRenderer))]
public class GemBase : MonoBehaviour, ITouchable {

    Coroutine moveTo = null;
    
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    [HideInInspector]
    public Animator animator;

    public GemType type;
    public Vector2Int position;
    public int minMatch = 3;
    
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    public void SetType(GemData gemData) {
        type = gemData.type;
        spriteRenderer.sprite = gemData.sprite;
        minMatch = gemData.minMatch;
    }

    public void SetPosition(Vector2Int position) {
        this.position = position;
        BoardController.gemBoard[position.x, position.y] = this;
    }

    public float MoveTo(Vector3 target, float speed) {
        if(moveTo != null)
            StopCoroutine(moveTo);
        
        moveTo = StartCoroutine(IEMoveTo(target, speed));

        return (target - transform.position).magnitude / speed;
    }

    IEnumerator IEMoveTo(Vector3 target, float speed) {
        
        float distance = (target - transform.position).magnitude;

        while(!Mathf.Approximately(0.0f, distance)) {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
            distance = (target - transform.position).magnitude;
        }

        transform.position = target;

    }

    public float Matched() {
        animator.SetTrigger("matched");
        animator.Update(0);

        return animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
    }

    public void Hint(bool start = true) {
        animator.SetBool("hinting", start);
    }

    public void TouchDown() {
        
    }

    public void TouchDrag() {
        if(Vector2.Distance(transform.position, TouchController.touchPosition) > 0.75f) {

            Vector2 delta = TouchController.touchPosition - transform.position;
            GemBase otherGem;

            if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {

                int swapX = (int) (position.x + Mathf.Sign(delta.x));
                otherGem = BoardController.GetGem(swapX, position.y);
            } else {
                
                int swapY = (int) (position.y + Mathf.Sign(delta.y));
                otherGem = BoardController.GetGem(position.x, swapY);
            }

            if(otherGem) {
                BoardController.TryMatch(this, otherGem);
            }

            TouchUp();
        }
            
    }

    public void TouchUp() {
        TouchController.ClearElementClicked();
    }

    public void DestroyGem() {
        Destroy(gameObject);
    }
}
