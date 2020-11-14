using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    private Vector3 fp;   //First touch position
    private Vector3 lp;   //Last touch position
    private float dragDistance;  //minimum distance for a swipe to be registered

    void Start()
    {
        dragDistance = Screen.height * 10 / 100; //dragDistance is ...% height of the screen
    }

    void Update()
    {
        if (Input.touchCount == 1) 
        {
            Touch touch = Input.GetTouch(0);            
            if (touch.phase == TouchPhase.Began) 
            {
                fp = touch.position;
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) 
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                lp = touch.position;

                //if drag is acceptable, detect whether it is up or down
                if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
                {
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   
                        if ((lp.x > fp.x))  //Right swipe
                        {   
                            FindObjectOfType<GridManager>().RotateTriad(1);
                        }
                        else //Left swipe
                        {   
                            FindObjectOfType<GridManager>().RotateTriad(2);
                        }
                    }
                    else
                    {   
                        if (lp.y > fp.y)  //Up swipe
                        {   
                            FindObjectOfType<GridManager>().RotateTriad(1);
                        }
                        else //Down swipe
                        {   
                            FindObjectOfType<GridManager>().RotateTriad(2);
                        }
                    }                    
                }
                else // If drag distance is not acceptable then it is a TAP - So use this for selection
                {
                    var pos = Camera.main.ScreenToWorldPoint(fp);
                    pos.z = transform.position.z;
                    FindObjectOfType<GridManager>().SelectTriad(pos);
                }
            }
        }
    }
}
