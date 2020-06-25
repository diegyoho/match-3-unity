using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;

public class TouchController : SingletonMonoBehaviour<TouchController> {

	public static Vector3 touchPosition;
	public static bool cancel = false;
	ITouchable elementClicked;
	
	void Update() {
        if (cancel) return;

    #if UNITY_EDITOR || UNITY_WEBGL

		HandleInputEditor();

    #elif UNITY_ANDROID || UNITY_IOS

		HandleInput();

    #endif
	}

	void HandleInput() {
		if (Input.touchCount == 1) {
			Touch touch = Input.GetTouch(0);
			touchPosition = (Vector2) Camera.main.ScreenToWorldPoint(
				touch.position
			);

			RaycastHit2D raycastHit = Physics2D.Raycast(
				touchPosition, Vector3.forward, Mathf.Infinity
			);

			if (elementClicked != null) {
				switch (touch.phase) {
					case TouchPhase.Moved:
						elementClicked.TouchDrag();
						break;
					case TouchPhase.Ended:
						elementClicked.TouchUp();
						elementClicked = null;
						break;
				}
			} else if (touch.phase == TouchPhase.Began) {
				if (raycastHit) {
					elementClicked = raycastHit.collider
									.GetComponent<ITouchable>();
				}
				if (elementClicked != null)
					elementClicked.TouchDown();
			}
		} else {
			ClearElementClicked();
		}
	}

#if UNITY_EDITOR || UNITY_WEBGL
	Vector3 lastPosition;
	void HandleInputEditor() {
		lastPosition = touchPosition;
		touchPosition = (Vector2) Camera.main
						.ScreenToWorldPoint(Input.mousePosition);

		RaycastHit2D raycastHit = Physics2D.Raycast(
			touchPosition, Vector3.forward, Mathf.Infinity
		);

		if (elementClicked != null) {
			if (Input.GetMouseButton(0)) {
				if (lastPosition != touchPosition)
					elementClicked.TouchDrag();
			} else {
				elementClicked.TouchUp();
				elementClicked = null;
			}
		} else if (Input.GetMouseButtonDown(0)) {
			if (raycastHit) {
				elementClicked = raycastHit.collider
								.GetComponent<ITouchable>();
			}

			if (elementClicked != null)
				elementClicked.TouchDown();

		}
	}

#endif

	public static void ClearElementClicked(ITouchable other) {
		if (instance.elementClicked == other) {
			ClearElementClicked();
		}
	}

	public static void ClearElementClicked() {
		instance.elementClicked = null;
	}
}