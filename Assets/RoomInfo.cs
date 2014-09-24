﻿using UnityEngine;
using System.Collections;

public class RoomInfo : MonoBehaviour {

    public GameObject spawnPoint;

    public static int ROOM_COLLIDER_LAYER_MASK = 1 << 2;

	public ArrayList climbableWalls = new ArrayList();

	public GameObject ceiling;

	SpiderControl control;

    public Transform DEBUG_startPos;

	// Use this for initialization
	void Start () {
		// fill ArrayList with climbable walls in the room this script is attached to
		GameObject[] allClimbableWalls = GameObject.FindGameObjectsWithTag("ClimbableWallCollider");
		foreach (GameObject climbableWall in allClimbableWalls)
		{
			if(climbableWall.transform.parent.gameObject.Equals(transform.gameObject))
			{
				climbableWalls.Add(climbableWall);
			}
		}
        
		control = GameObject.FindGameObjectWithTag("Spider").GetComponent<SpiderControl>();
		//roomHeight = ceiling.transform.position.y - bottom.transform.position.y - 1.0f;


	}

	void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.tag.Equals("Spider"))
		{
			control.generalBehaviour.setCurrentRoom(this);
            control.generalBehaviour.DEBUG_startPos = DEBUG_startPos.position;

            // DEBUG
            //for (int i = 0; i <= 9; i++)
            //{
            //    if (i >= 3 && transform.root.gameObject.name.Equals("Room_" + i))
            //    {
            //        control.DEBUG_ExtendedBehaviourActive = false;
            //        break;
            //    }
            //    else if (transform.root.gameObject.name.Equals("Room_Office") || transform.root.gameObject.name.Equals("Room_Storage"))
            //    {
            //        control.DEBUG_ExtendedBehaviourActive = true;
            //        break;
            //    }
            //}
		}
        else if (spawnPoint != null && c.gameObject.tag.Equals("Player") && control.generalBehaviour.currentRoom != this && !control.generalBehaviour.behaviourChangeLocked && !control.WaitingOnCheckpoint)
        {
            if (control.generalBehaviour.currentBehaviour is WanderBehaviour)
            {
                control.generalBehaviour.spawnSpider(spawnPoint.transform.position, this, false);
            }

        }
	}

}
