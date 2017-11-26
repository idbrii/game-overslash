using Random = UnityEngine.Random;
using System.Collections;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
/*~ 
using UnityStandardAssets.Utility;
*/

public class PlayerInput : MonoBehaviour {

    Rigidbody2D body;

    public Transform hand;
    public Transform sword;
    public float hand_distance = 0.1f;
    float hand_angle = 0.0f;
    Vector2 hand_pos;
    Vector2 HAND_NEUTRAL_POSITION = new Vector2(0f,1f);

    public float max_move_speed = 5.0f;
    public float move_acceleration = 5.0f;
    float move_speed = 5.0f;
    Vector2 movement_intent;

    void Start() {
        body = GetComponent<Rigidbody2D>();
    }

    void Update() {
        Hand_ReadInput();
        Movement_ReadInput();
    }

    void FixedUpdate() {
        Hand_ApplyHandTransform();
        Movement_ApplyMovementIntent();
    }

    void Movement_ReadInput() {
        var direction = new Vector2(
                CrossPlatformInputManager.GetAxis("Move-LeftRight"),
                CrossPlatformInputManager.GetAxis("Move-ForwardBack"));
        if (direction == Vector2.zero) {
            movement_intent = Vector2.zero;
        }
        else {
            movement_intent = direction.normalized;
        }
    }

    void Hand_ReadInput() {
        var direction = new Vector2(
                CrossPlatformInputManager.GetAxis("Hand-LeftRight"),
                CrossPlatformInputManager.GetAxis("Hand-ForwardBack"));
        direction.y = Mathf.Clamp(direction.y, 0.0f, 1.0f);
        if (direction == Vector2.zero) {
            hand_pos = HAND_NEUTRAL_POSITION;
        }
        else {
            hand_pos = direction.normalized;
        }
    }

    void Hand_ApplyHandTransform() {
        hand.localPosition = hand_pos * hand_distance;
        hand.rotation = Quaternion.FromToRotation(hand.up, hand_pos) * hand.rotation;
    }

    void Movement_ApplyMovementIntent() {
        Vector2 pos = transform.position;
        body.MovePosition(pos + movement_intent * Time.deltaTime);
    }
}
