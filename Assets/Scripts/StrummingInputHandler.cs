using UnityEngine;
using System.Collections;

public class StrummingInputHandler : MonoBehaviour, IFingerTouchHandler
{ 
    private Vector2 currentTouchPos;  //current position of finger touch in screen coordinate
    private Vector2 lastTouchPos;   //the last recorded position of finger touch in screen coordinate
    private float velocity;   //velocity of finger swipe (in vertical direction i.e. y-axis) used for setting volume of string while strumming
                              //rougly equates to how much pixel per second travelled 

    private float updateInterval = 10f / 1000;  //checks for swipe movement every 10ms
    private float volumeFactor = 1.0f / 1000;  //normalize velocity to between 0 and 1 to set volume of string while strumming                                            

    public GameManager gameManager;

    public void OnBegin(Touch touch)
    {
        currentTouchPos = touch.position;
        lastTouchPos = touch.position;

        StartCoroutine(HandleSwipes());
    }

    public void OnStationary(Touch touch)
    {
        //do nothing
    }

    public void OnMove(Touch touch)
    {
        currentTouchPos = touch.position;
    }

    public void OnEnd(Touch touch)
    {
        StopCoroutine("HandleSwipes");
    }

    public void OnCancel(Touch touch)
    {
        //do nothing
    }


    private IEnumerator HandleSwipes()
    {//actually responsible for handling swiping motion and then playing sound of correponding string
        while(true)
        {
            if (currentTouchPos.y != lastTouchPos.y)
            {//if there is no vertical movement then no need to perform any operation
                velocity = (currentTouchPos.y - lastTouchPos.y) / updateInterval;
                float stringVolume = Mathf.Abs(velocity * volumeFactor);
                //Debug.Log("Volume: " + stringVolume);

                foreach (GameObject guitarStringObj in gameManager.guitarStrings)
                {
                    //touch points are in screen coordinate so for comparison they need to be in same coordinate space
                    Vector3 guitarStringScreenspacePos = Camera.main.WorldToScreenPoint(guitarStringObj.transform.position);

                    if ((guitarStringScreenspacePos.y <= currentTouchPos.y && guitarStringScreenspacePos.y >= lastTouchPos.y) ||
                        (guitarStringScreenspacePos.y >= currentTouchPos.y && guitarStringScreenspacePos.y <= lastTouchPos.y))
                    {//the swipe line crosses the string
                        GuitarString guitarString = guitarStringObj.GetComponent<GuitarString>();
                        guitarString.setVolume(stringVolume);
                        guitarString.playString();
                    }
                }
            }
            lastTouchPos = currentTouchPos;
            yield return new WaitForSeconds(updateInterval);
        }
    }
}
