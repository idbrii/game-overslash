using Random = UnityEngine.Random;
using System.Collections;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerInput : MonoBehaviour {

    Rigidbody2D body;

    public Transform hand;
    public Transform sword;
    public float hand_distance = 0.1f;
    float hand_angle = 0.0f;
    Vector2 hand_pos;
    Vector2 hand_intent;
    static Vector2 HAND_NEUTRAL_POSITION = new Vector2(0f,1f);

    public float max_move_speed = 5.0f;
    public float move_acceleration = 5.0f;
    float move_speed = 5.0f;
    Vector2 movement_intent;
    Vector2 hold_intent = HAND_NEUTRAL_POSITION;
    Vector2 target_intent = HAND_NEUTRAL_POSITION;
    public float hold_tolerance = 0.005f;
    public float hold_required_time = 4.0f;
    public float swing_duration = 3.0f;
    bool has_started_move = false;
    float held_duration = 0.0f;
    float move_start_time = -1.0f;
    float time_spent_moving = 0.0f;

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
        if (Approximately(direction, Vector2.zero, hold_tolerance)) {
            hand_intent = HAND_NEUTRAL_POSITION;
        }
        else {
            hand_intent = direction.normalized;

            if (hand_intent == hold_intent) {
                held_duration += Time.deltaTime;
                bool was_moving = has_started_move;
                has_started_move = held_duration >= hold_required_time;
                if (was_moving != has_started_move) {
                    move_start_time = Time.time;
                    time_spent_moving = 0.0f;
                    target_intent = hold_intent;
                }
            }
            else {
                hold_intent = hand_intent;
                held_duration = 0.0f;
            }
        }
    }

    static bool Approximately(Vector2 lhs, Vector2 rhs, float tolerance_distance) {
        return Vector2.SqrMagnitude(lhs - rhs) < Mathf.Pow(tolerance_distance, 2.0f);
    }

    static float old_SignedAngle(Vector2 a, Vector2 b) {
        var angle = Vector2.Angle(a,b);
        var is_same = Vector2.Dot(Vector2.right, a) > 0 == Vector2.Dot(Vector2.right, b) > 0;
        if (is_same) {
            angle *= -1.0f;
        }
        return angle;
    }
    static float broken_SignedAngle(Vector2 a, Vector2 b) {
        // TODO(dbriscoe): This is supposed to animate the hand to the
        // desired destination. I couldn't get angles working so I just
        // used x position. That still doesn't work very well (but does
        // work a bit).
        if (b.x == 0.0f) {
            // Straight up. (Don't care about down because we only look up.)
            return 0.0f;
        }
        return Mathf.Atan(b.y/b.x);
    }
    static float SignedAngle(Vector2 a, Vector2 b) {
        return b.x;
    }

    void Hand_ApplyHandTransform() {
        // TODO(dbriscoe): Replace with Vector2.SignedAngle in newer unity
        float intent = SignedAngle(Vector2.up, target_intent);
        float current = SignedAngle(Vector2.up, hand_pos);
        if (has_started_move) {
            time_spent_moving += Time.deltaTime;
            float t = time_spent_moving / swing_duration;
            //t = (Time.time - move_start_time) / swing_duration;
            if (t > 1.0f) {
                return;
            }
            t = Mathf.SmoothStep(0, 1, t);
            float destination = Mathf.Lerp( //Angle
                    current, 
                    intent, 
                    t);
            hand_pos = new Vector2( Mathf.Sin(destination), Mathf.Cos(destination) );
            hand_pos = new Vector2(destination, 1.0f);
            hand_pos.Normalize();
            Debug.Log(string.Format("Progress: {0,4:0.00}    ", t),  this);
            //Debug.Log(string.Format("Progress. {0,4:0.00}    {1,4:000.00} {2,4:000.00}", t, intent, current ),  this);

            hand.localPosition = hand_pos * hand_distance;
            hand.rotation = Quaternion.FromToRotation(hand.up, hand_pos) * hand.rotation;
        }
        else {
            Debug.Log(string.Format("Countdown. {0,4:0.00}    {1,4:000.00} {2,4:000.00}", held_duration/hold_required_time, intent, current ),  this);
        }
    }

    void Movement_ApplyMovementIntent() {
        Vector2 pos = transform.position;
        body.MovePosition(pos + movement_intent * Time.deltaTime);
    }
}
