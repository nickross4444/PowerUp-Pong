using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class paddleControl : MonoBehaviour
{
    public bool botEnabled;
    public float botDifficulty = 0.5f, yVel;     //difficulty should be between 0 and 1
    [Range(0.0f, 25.0f)]
    public float time = 1f;
    Vector3 prevPos;
    public GameObject ball, border;
    public Toggle botTog;
    public Slider diffSlider;
    public Text diffTxt;
    //public float bounds;
    public bool touchingWall = false;
    public KeyCode up;
    public KeyCode down;
    public float speed;
    float originalSpeed;
    Rigidbody2D ballRb;
    float serveOffset;
    float initialScale;
    float bounceDifThresh = 0.5f;       //bounce difficulty threshhold

    void Start()
    {
        initialScale = gameObject.transform.localScale.magnitude;
        originalSpeed = speed;
        ballRb = ball.GetComponent<Rigidbody2D>();
        serveOffset = Random.Range(-25, 25);
    }
    public void resetSpeed()
    {
        speed = originalSpeed;
    }
    void FixedUpdate()
    {
        Time.timeScale = time;
        if (botTog.isOn && botEnabled)        //if bot control
        {
            //if(movingThisWay() && withinRange())       // only calculates if the ball is moving the right way  //maybe do difficulty here by changing where it starts calculating
            if(withinRange())       // only calculates if the ball is moving the right way  //maybe do difficulty here by changing where it starts calculating
            {
                float predictedYPos;
                if (movingThisWay())
                {
                    //pos + number of increments * y increments
                    predictedYPos = ball.transform.localPosition.y + Mathf.Abs((ball.transform.localPosition.x - gameObject.transform.localPosition.x) / ballRb.velocity.x) * ballRb.velocity.y;
                }
                else if(botDifficulty > bounceDifThresh)//this prediscts where the ball will go before the opposing player hits it if difficulty is above threshold and moving away
                {
                    //Debug.Print();
                    //if travelling away, = current pos + number of increments * y increments. Number of increments is x courtSize + distance from other paddle, which is xcourtsize-distance, 2*xcourtsize - distance
                    predictedYPos = ball.transform.localPosition.y + Mathf.Abs((1920 - Mathf.Abs(ball.transform.localPosition.x - gameObject.transform.localPosition.x)) / ballRb.velocity.x) * ballRb.velocity.y;
                }
                else        //if moving away and not above difficulty
                {
                    return;
                }
                //print(predictedYPos);
                if (botDifficulty > bounceDifThresh)      //calculats bounces only if the difficulty is above
                {
                    //wall thickness: 11
                    //ball radius: 9
                    //ball turns around 14 units before the wall position
                    float courtSize = 2 * border.transform.localPosition.y - 28;        //subtract  for the width of ball and border
                    bool oddReflections = false;        //true if bounces is odd
                    if (ballRb.velocity.y > 0)       //if moving vertically
                    {
                        //convert variables to court coordinates: bottom of court is zero
                        //predictedYPos += border.transform.localPosition.y;              //bottom of court is now zero
                        predictedYPos += courtSize / 2;              //bottom of court is now zero
                        int bounceCount = (int)(predictedYPos / courtSize);
                        if (bounceCount % 2 == 1)
                        {
                            oddReflections = true;
                        }
                        predictedYPos -= bounceCount * courtSize;
                        if (oddReflections)
                        {
                            predictedYPos = courtSize - predictedYPos;
                        }
                        //convert back from court coordinates into local coordinates:
                        predictedYPos -= border.transform.localPosition.y;
                    }
                    else if (ballRb.velocity.y < 0)        //if moving downward, same as up but the math is flipped then unflipped
                    {
                        //convert variables to court coordinates: bottom of court is zero
                        predictedYPos -= courtSize / 2;              //top of court is now zero
                        predictedYPos *= -1;    //flip everything vertically
                        int bounceCount = (int)(predictedYPos / courtSize);
                        if (bounceCount % 2 == 1)
                        {
                            oddReflections = true;
                        }
                        predictedYPos -= bounceCount * courtSize;
                        if (oddReflections)
                        {
                            predictedYPos = courtSize - predictedYPos;
                        }
                        //convert back from court coordinates into local coordinates:
                        predictedYPos -= border.transform.localPosition.y;
                        predictedYPos *= -1;    //flip everything vertically
                    }
                }
                if(ballRb.velocity.y == 0 || botDifficulty > 0.75)       //if moving straight ex. if served to, or at high difficulty
                {
                    predictedYPos += serveOffset * gameObject.transform.localScale.magnitude / initialScale;        //random as to not serve straight. Scales with size of paddle.
                }
                //move based on predicted pos:
                if (predictedYPos - gameObject.transform.localPosition.y > 5 && !(touchingWall && gameObject.transform.position.y > 0))    //only moves if more than x units off
                {
                    moveUp();
                }
                else if(predictedYPos - gameObject.transform.localPosition.y < -5 && !(touchingWall && gameObject.transform.position.y < 0))
                {
                    moveDown();
                }
            }
            else
            {
                //this is now set when the ball is hit.
                //serveOffset = Random.Range(-30, 30) * gameObject.transform.localScale.magnitude / initialScale;        //changes the serve offset whenever the ball is moving away. scales with the size of the game object for buffs and debuffs
            }
        }
        else if(Input.touchSupported && Mathf.Sign(Input.GetTouch(0).position.x - Screen.width/2) == Mathf.Sign(gameObject.transform.localPosition.x))      //uses short circuting  //checks if touch and paddle are on the same side.
        {
            if (Input.GetTouch(0).position.y > Screen.height / 2 && !(touchingWall && gameObject.transform.position.y > 0))
            {
                moveUp();
            }
            else if (Input.GetTouch(0).position.y < Screen.height / 2 && !(touchingWall && gameObject.transform.position.y < 0))
            {
                moveDown();
            }
        }
        else        //if not bot and not touch
        {
            if (Input.GetKey(up) && !(touchingWall && gameObject.transform.position.y > 0))
            {
                moveUp();
            }
            else if (Input.GetKey(down) && !(touchingWall && gameObject.transform.position.y < 0))
            {
                moveDown();
            }
        }

        yVel = (transform.localPosition.y - prevPos.y) / Time.deltaTime;
        prevPos = transform.localPosition;
    }
    public void diffChange()        //called upon value change;
    {
        botDifficulty = diffSlider.value;
        string diffString = (Mathf.Round(botDifficulty * 100)).ToString() + "%";
        diffTxt.text = diffString;
    }
    float map(float i, float x, float y, float a, float b)
    {
        return (i - x) * (b - a) / (y - x) + a;
    }
    void moveUp()
    {
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y + speed, gameObject.transform.localPosition.z);
    }
    void moveDown()
    {
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y - speed, gameObject.transform.localPosition.z);
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Wall"))
        {
            touchingWall = true;
        }
        else        //hit ball
        {
            serveOffset = Random.Range(-30, 30);        //changes the serve offset whenever the ball is hit.
        }
    }
    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Wall"))
        {
            touchingWall = false;
        }
    }
    bool movingThisWay()
    {
        if(gameObject.transform.position.x < 0 && ballRb.velocity.x < 0)
        {
            return true;
        }
        else if(gameObject.transform.position.x > 0 && ballRb.velocity.x > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    bool withinRange()
    {
        if (gameObject.transform.position.x < 0 && ball.transform.position.x < map(botDifficulty, 0, 1, gameObject.transform.position.x, -gameObject.transform.position.x))
        {
            return true;
        }
        else if (gameObject.transform.position.x > 0 && ball.transform.position.x > map(botDifficulty, 0, 1, gameObject.transform.position.x, -gameObject.transform.position.x))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
