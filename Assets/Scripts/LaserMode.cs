﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserMode : MonoBehaviour 
{
    private Player playerRef;
    private bool oculusConnected;

    public List<GameObject> cutable;
    public GameObject target;

    private float timer = 0;
    private float timerMax = 2;

    public enum LaserState {WAITING ,TARGETING, LASERING};
    public LaserState currentState = LaserState.WAITING;
    public LaserState desiredState = LaserState.WAITING;
    private LineRenderer laser;

    // === Targeting values
    private Ray playerLook;

    public enum TargetingMethods { LIGHT, CROSSHAIR};
    public TargetingMethods targetingSystem;

    private GameObject lightTargeter;
    private bool laserCanHitTarget;
    public Vector3 laserPos;

    private GameObject crosshairGUIObject;
    private RenderTexture crosshairRenderTexture;
    public Texture crosshairImg;

    // === Debug Variables when Oculus is not connected ===
    private LineRenderer pointer;
    private Ray laserTargeter;
    private Ray lookingDirection;
    private float alpha = 0;
    private float beta = 0;

	// Use this for initialization
	void Start ()
    {
        playerRef = this.transform.parent.GetComponent<Player>();
        oculusConnected = playerRef.useOculus;

        if (oculusConnected)
        {
            switch (targetingSystem)
            {
                case TargetingMethods.LIGHT:
                    {            
                        break;
                    }
                case TargetingMethods.CROSSHAIR:
                    {
                        crosshairRenderTexture = new RenderTexture(Screen.width, Screen.height, 0);

                        break;
                    }
            }
        }
        else
        {
            
        }
	}

    void OnGUI()
    {
        if (currentState == LaserState.TARGETING || currentState == LaserState.LASERING)
        {
            crosshairGUIObject.SetActive(true);

            RenderTexture previous = RenderTexture.active;

            RenderTexture.active = crosshairRenderTexture;
            GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));

            GUI.DrawTexture(new Rect(0, 0, crosshairRenderTexture.width, crosshairRenderTexture.height), crosshairImg);

            RenderTexture.active = previous;
        }
    }

	
	// Update is called once per frame
	void Update () 
    {
        // === Waiting State ===
        if (currentState == LaserState.WAITING)
        {
            if (Input.GetButtonDown("Use") && cutable.Count != 0)
            {
                foreach (GameObject g in cutable)
                {
                    RaycastHit hit;
                    Physics.Raycast(this.transform.position, g.transform.position - this.transform.position, out hit);

                    if (hit.collider.transform.parent.gameObject.GetInstanceID() == g.GetInstanceID())
                    {
                        desiredState = LaserState.TARGETING;
                    }
                }
            }
        }

        // === Targeting State ===
        if (currentState == LaserState.TARGETING)
        {
            if (Input.GetButtonDown("Use"))
            {
                desiredState = LaserState.WAITING;
            }

            RaycastHit hit;

            if (oculusConnected)
            {
                playerLook = new Ray();
                playerLook.origin = playerRef.eyeCenter.position;
                playerLook.direction = playerRef.eyeCenter.forward;

                Physics.Raycast(playerLook, out hit, 20);                

            }
            else
            {
                float rStickH = Input.GetAxis("RStickH");
                float rStickV = Input.GetAxis("RStickV");

                if (Mathf.Abs(rStickH) > 0.3f || Mathf.Abs(rStickH) > 0.3f)
                {
                    alpha = alpha + (rStickH * 1f);
                }

                if (Mathf.Abs(rStickV) > 0.3f || Mathf.Abs(rStickV) > 0.3f)
                {
                    beta = beta + (rStickV * 1f);

                    if (beta < 0)
                        beta = 0;

                    if (beta > 180)
                        beta = 180;
                }

                float radAlpha = alpha * Mathf.Deg2Rad;
                float radBeta = beta * Mathf.Deg2Rad;

                laserTargeter.direction = new Vector3(Mathf.Sin(radAlpha) * Mathf.Sin(radBeta), Mathf.Cos(radBeta), Mathf.Cos(radAlpha) * Mathf.Sin(radBeta));

                Physics.Raycast(laserTargeter, out hit, 10);

                pointer.SetPosition(1, hit.point);

                Debug.DrawRay(laserTargeter.origin, laserTargeter.direction, Color.cyan);
            }

            foreach (GameObject g in cutable)
            {
                if (hit.collider.transform.parent.gameObject.GetInstanceID() == g.GetInstanceID())
                {
                    target = g;
                    desiredState = LaserState.LASERING;
                    break;
                }
            }
        }

        // === Lasering State ===
        if (currentState == LaserState.LASERING)
        {
            if (Input.GetButtonDown("Use"))
            {
                desiredState = LaserState.WAITING;
            }

            if (oculusConnected)
            {
                switch (targetingSystem)
                {
                    case TargetingMethods.LIGHT:
                        {
                            lightTargeter.transform.LookAt(target.transform.position);
                            break;
                        }
                    case TargetingMethods.CROSSHAIR:
                        {

                            break;
                        }
                }
            }

            if (Input.GetButton("Action"))
            {
                laser.enabled = true;

                if(laserCanHitTarget)
                    timer += Time.deltaTime;
            }
            else
            {
                laser.enabled = false;
            }

            if (timer > timerMax)
            {
                cutable.Remove(target);
                Destroy(target.GetComponent<Lock>());
                Destroy(target.transform.parent.GetComponentInChildren<FixedJoint>());
                target = null;

                desiredState = LaserState.WAITING;
            }
        }

        if (currentState != desiredState)
        {
            changeStateTo(desiredState);
        }

        if (target != null)
        {
            Debug.DrawRay(playerRef.eyeCenter.position, target.transform.position - playerRef.eyeCenter.position, Color.red);
        }
	}

    private void changeStateTo(LaserState targetState)
    {
        // === On-Exit Operations ======================================================================================
        switch (currentState)
        {
            case LaserState.WAITING:
                {
                    this.transform.parent.GetComponent<FlightControl>().ControlsActivated = false;
                   
                    break;
                }
            case LaserState.TARGETING:
                {

                    break;
                }
            case LaserState.LASERING:
                {
                    if (oculusConnected)
                    {
                        switch (targetingSystem)
                        {
                            case TargetingMethods.LIGHT:
                                {
                                    break;
                                }
                            case TargetingMethods.CROSSHAIR:
                                {
                                    break;
                                }
                        }
                    }
                    else
                    {
                        
                    }

                    Destroy(laser.gameObject);

                    break;
                }
        }
        // =============================================================================================================

        // === On-Enter Operations ======================================================================================
        switch (targetState)
        {
            case LaserState.WAITING:
                {
                    playerRef.laserModeActive = false;

                    if (oculusConnected)
                    {
                        switch (targetingSystem)
                        {
                            case TargetingMethods.LIGHT:
                                {
                                    Destroy(lightTargeter);

                                    break;
                                }
                            case TargetingMethods.CROSSHAIR:
                                {
                                    Destroy(crosshairGUIObject);

                                    break;
                                }
                        }
                    }
                    else
                    {
                        Destroy(pointer);
                    }

                    this.transform.parent.GetComponent<FlightControl>().ControlsActivated = true;

                    break;
                }
            case LaserState.TARGETING:
                {
                    playerRef.laserModeActive = true;

                    if (oculusConnected)
                    {
                        switch (targetingSystem)
                        {
                            case TargetingMethods.LIGHT:
                                {
                                    lightTargeter = GameObject.Instantiate(Resources.Load("LightTargeter")) as GameObject;
                                    lightTargeter.transform.position = playerRef.eyeCenter.position;
                                    lightTargeter.transform.rotation = playerRef.eyeCenter.rotation;
                                    lightTargeter.transform.parent = playerRef.eyeCenter;
                                    
                                    break;
                                }
                            case TargetingMethods.CROSSHAIR:
                                {
                                    crosshairGUIObject = GameObject.Instantiate(Resources.Load("CrosshairGUIObject")) as GameObject;

                                    crosshairGUIObject.renderer.material.mainTexture = crosshairRenderTexture;
                                    //crosshairGUIObject.SetActive(false);

                                    Vector3 localPos = crosshairGUIObject.transform.localPosition;
                                    Vector3 localRot = crosshairGUIObject.transform.localEulerAngles;

                                    crosshairGUIObject.transform.parent = playerRef.eyeCenter;
                                    
                                    crosshairGUIObject.transform.localPosition = localPos;
                                    crosshairGUIObject.transform.localEulerAngles = localRot;

                                    break;
                                }
                        }
                    }
                    else
                    {
                        lookingDirection = laserTargeter = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                        alpha = Vector3.Angle(new Vector3(0, 0, 1), laserTargeter.direction);
                        beta = Vector3.Angle(new Vector3(0, 1, 0), laserTargeter.direction);

                        lookingDirection.direction.Normalize();
                        laserTargeter.direction.Normalize();

                        pointer = this.gameObject.AddComponent<LineRenderer>();
                        pointer.SetPosition(0, playerRef.eyeCenter.position + new Vector3(0, -0.05f, 0));
                        pointer.SetWidth(0.005f, 0.005f);
                    }

                    break;
                }
            case LaserState.LASERING:
                {
                    playerRef.laserModeActive = true;

                    GameObject laserGO = GameObject.Instantiate(Resources.Load("Laser")) as GameObject;
                    laser = laserGO.GetComponent<LineRenderer>();
                    laserGO.transform.parent = playerRef.eyeCenter;

                    Vector3 laserStartPos = playerRef.eyeCenter.transform.position + laserPos;

                    Ray r = new Ray(laserStartPos, target.transform.position - laserStartPos);
                    RaycastHit hit;

                    if (Physics.Raycast(r, out hit, 10))
                    {
                        if (hit.collider.transform.parent.gameObject.GetInstanceID() == target.GetInstanceID())
                            laserCanHitTarget = true;
                        else
                            laserCanHitTarget = false;
                    }

                    laser.SetPosition(0, laserStartPos);
                    laser.SetPosition(1, hit.point);

                    if (oculusConnected)
                    {
                        switch (targetingSystem)
                        {
                            case TargetingMethods.LIGHT:
                                {

                                    break;
                                }
                            case TargetingMethods.CROSSHAIR:
                                {

                                    Vector3 eyeCenterToTarget = playerRef.eyeCenter.position + (target.transform.position - playerRef.eyeCenter.position);

                                    crosshairGUIObject.transform.parent = playerRef.transform;

                                    crosshairGUIObject.transform.position = new Vector3(eyeCenterToTarget.x, eyeCenterToTarget.y, crosshairGUIObject.transform.position.z);

                                    crosshairGUIObject.transform.LookAt(playerRef.eyeCenter);
                                    crosshairGUIObject.transform.Rotate(0, 180, 0);
                                    crosshairGUIObject.transform.Rotate(270, 0, 0);
                                    break;
                                }
                        }
                    }
                    else
                    {
                        pointer.SetPosition(1, target.transform.position);
                    }


                    timer = 0;
                    timerMax = target.GetComponent<Lock>().duration;

                    break;
                }
        }
        // =============================================================================================================

        currentState = targetState;
    }
}
