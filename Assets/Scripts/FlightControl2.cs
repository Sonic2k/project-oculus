﻿using UnityEngine;
using System.Collections;

public class FlightControl2 : MonoBehaviour
{
    public Player playerRef;

    public float directionalForce;
    public ForceMode directionalForceMode;

    public float upwardForce;

    public float maxDirectionalVelocity;
    public float currentDirectionalVelocity;

    private Transform audioChild;
    private AudioSource audio;

    #region Y-Rotate Variables
    public bool useDirectYawInput;
    private float yawVelTarget = 0f; // Desired Velocity for Y-Rotation
    private float yawVelValue = 0f; // Current Velocity for Y-Rotation
    private float yawVelMax = 1.8f; // Maximum Velocity for Y-Rotation
    private float yawAcc = 3.6f; // Acceleration for Y-Rotation
    #endregion

    #region Z-Rotate Variables
    public bool useRolling;
    // local Z-Rotation is applied when flying in steep curves with high velocity
    private float rollAngleTarget = 0;
    private float rollAngleValue = 0;
    private float rollAngleMax = 10; // Maximum Z-Rotation (0 for oculus. prevents simulator sickness)
    private float rollAcc = 20;
    #endregion

    private float lStickV;
    private float lStickH;
    private float shoulder;
    private float rStickV;
    private float rStickH;

    private float moveVol = 0.4f;
    private float movePitch = 0.36f;

    // Use this for initialization
    void Start()
    {
        audioChild = transform.Find("AudioSource");
        audio = audioChild.GetComponent<AudioSource>();

        maxDirectionalVelocity = 0;
        currentDirectionalVelocity = 0;

        playerRef = FindObjectOfType<Player>();
    }

    void FixedUpdate()
    {
        lStickV = 0;
        lStickH = 0;
        shoulder = 0;
        rStickV = 0;
        rStickH = 0;

        audio.volume = 0.3f;
        audio.pitch = 0.3f;

        // == Get all Joystick Positions =================================
        if (playerRef.FlightControlEnabled)
        {
            if (playerRef.useGamepad)
            {
                lStickV = Input.GetAxis("LStickV");
                lStickH = Input.GetAxis("LStickH");

                shoulder = Input.GetAxis("Shoulder");

                rStickV = Input.GetAxis("RStickV");
                rStickH = Input.GetAxis("RStickH");
            }
            else
            {
                lStickV = Input.GetAxis("ZAxis");
                lStickH = Input.GetAxis("XAxis");

                shoulder = Input.GetAxis("YAxis");

                rStickV = Input.GetAxis("RStickV");
                rStickH = Input.GetAxis("YAngular");
            }
        }
        // ===============================================================

        // ====== Movement on xz Axis ======================================
        if (Mathf.Abs(lStickV) > 0.3f || Mathf.Abs(lStickH) > 0.3f)
        {
            Vector3 forceDir = this.transform.forward * lStickV + this.transform.right * lStickH;

            this.rigidbody.AddForce(forceDir * directionalForce, directionalForceMode);

            audio.volume = moveVol;
            audio.pitch = movePitch;
        }

        // ====== Movement on y Axis =======================================
        Vector3 upForce = (-1 * Physics.gravity * this.rigidbody.mass) + (new Vector3(0, 1, 0) * shoulder * upwardForce);

        this.rigidbody.AddForce(upForce, ForceMode.Force);

        currentDirectionalVelocity = Mathf.Abs(this.rigidbody.velocity.x) + Mathf.Abs(this.rigidbody.velocity.z);

        if (shoulder != 0)
        {
            audio.volume = moveVol;
            audio.pitch = movePitch;
        }

        // ===== Angular Movement ==========================================
        if (currentDirectionalVelocity > maxDirectionalVelocity)
            maxDirectionalVelocity = currentDirectionalVelocity;

        if (Mathf.Abs(rStickH) > 0.3f)
        {
            yawVelTarget = rStickH * yawVelMax;
        }
        else
        {
            yawVelTarget = 0;
        }

        if (useDirectYawInput)
            yawVelValue = yawVelTarget;
        else
            yawVelValue = calcValue(yawVelValue, yawAcc, yawVelTarget, yawAcc * Time.deltaTime);

        this.transform.Rotate(0, yawVelValue, 0, Space.World);

        if (useRolling)
        {
            float rollAngleFactor = (yawVelValue / yawVelMax) * (currentDirectionalVelocity / maxDirectionalVelocity);

            print(rollAngleFactor);

            rollAngleTarget = -1 * rollAngleFactor * rollAngleMax; //multiplied with -1 for correct roll direction

            rollAngleValue = calcValue(rollAngleValue, rollAcc, rollAngleTarget, rollAcc * Time.deltaTime);

            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, rollAngleValue);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private float calcValue(float val, float acc, float target, float tolerance)
    {
        float difference = target - val;

        if (difference > tolerance)
        {
            val += acc * Time.deltaTime;
        }
        else if (difference < -tolerance)
        {
            val -= acc * Time.deltaTime;
        }
        else
        {
            val = target;
        }

        return val;
    }

    void OnCollisionEnter(Collision c)
    {
        //this.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnCollisionExit(Collision c)
    {
        //this.rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
}
