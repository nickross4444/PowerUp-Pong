using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ball : MonoBehaviour
{
    public Vector2 initialForce;
    public float ballLimit, speedMultiplier;
    public Text p1ScoreTxt, p2ScoreTxt, rallyTxt, maxRallyTxt;
    public Slider gravSlider;
    public GameObject paddle1, paddle2, winObj, speedObj, sizeObj, deSpeedObj, deSizeObj, invisObj, directionsObj;
    int p1Score = 0, p2Score = 0, rallyCount = 0;
    int lastPlayerToScore = 2;      //1 or 2 //by starting on two ball goes to the right
    public AudioClip wallClip, paddleClip, scoreClip;
    AudioSource aud;
    Rigidbody2D rb;
    Vector3 originalPaddleScale;
    Color32 originalColor;

    // Start is called before the first frame update
    void Start()
    {
        originalPaddleScale = paddle1.transform.localScale;
        originalColor = paddle1.GetComponent<SpriteRenderer>().color;
        rb = gameObject.GetComponent<Rigidbody2D>();
        aud = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameObject.transform.localPosition.x > ballLimit)
        {
            p1Score++;
            lastPlayerToScore = 1;
            setScore();
        }
        else if (gameObject.transform.localPosition.x < -ballLimit)
        {
            p2Score++;
            lastPlayerToScore = 2;
            setScore();
        }
        if(Mathf.Abs(gameObject.transform.localPosition.y) > 1000)
        {
            resetGame();        //resets with no score if the ball goes out of court vertically
        }
        if(p1Score+p2Score == 0 && rallyCount == 0)        //if the game is only just started
        {
            directionsObj.SetActive(true);
        }
        else
        {
            directionsObj.SetActive(false);
        }
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Player"))
        {
            aud.clip = paddleClip;
            aud.Play();
            rallyCount++;
            rallyTxt.text = rallyCount.ToString();
            if (int.Parse(maxRallyTxt.text) < rallyCount)
            {
                maxRallyTxt.text = rallyCount.ToString();
            }
            float forceOutput = 0.4f * coll.gameObject.GetComponent<paddleControl>().yVel + 200 * (coll.GetContact(0).point.y - coll.gameObject.transform.position.y);
            rb.AddForce(new Vector2(0, forceOutput));
            rb.velocity *= speedMultiplier;
        }
        else if (coll.collider.CompareTag("Wall"))
        {
            aud.clip = wallClip;
            aud.Play();
        }
    }
    void OnTriggerEnter2D(Collider2D cldr)            //triggers for powerups
    {
        if(rallyCount == 0)     //if it's  touching the ball when it spawns in
        {
            return;
        }
        cldr.gameObject.SetActive(false);
        GameObject activePaddle;
        GameObject inActivePaddle;
        if (rb.velocity.x > 0)       //if moving right, p1 is in possession
        {
            activePaddle = paddle1;
            inActivePaddle = paddle2;
        }
        else
        {
            activePaddle = paddle2;
            inActivePaddle = paddle1;
        }
        switch (cldr.gameObject.name)
        {
            case "Speed":
                StartCoroutine(applySpeed(activePaddle, 2));
                break;
            case "Size":
                StartCoroutine(applySize(activePaddle, 2));
                break;
            case "DeSpeed":
                StartCoroutine(applySpeed(activePaddle, .5f));
                break;
            case "DeSize":
                StartCoroutine(applySize(activePaddle, .5f));
                break;
            case "Invis":
                StartCoroutine(applyInvis(activePaddle));
                break;
        }

    }
    void setScore()
    {
        aud.clip = scoreClip;
        aud.Play();
        p1ScoreTxt.text = p1Score.ToString();
        p2ScoreTxt.text = p2Score.ToString();
        rallyCount = 0;
        rallyTxt.text = rallyCount.ToString();
        if (p1Score < 11 && p2Score < 11)
        {
            resetGame();
        }
        else
        {
            win();
        }
    }
    void resetGame()
    {
        rb.velocity = new Vector2(0, 0);
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        float paddleXPos = paddle1.transform.position.x;
        paddle1.transform.position = new Vector3(paddleXPos, 0, 0);
        paddle2.transform.position = new Vector3(-paddleXPos, 0, 0);
        if (lastPlayerToScore == 1)
        {
            rb.AddForce(-initialForce);
        }
        else if (lastPlayerToScore == 2)
        {
            rb.AddForce(initialForce);
        }
    }
    void win()
    {
        resetGame();
        gameObject.SetActive(false);
        winObj.SetActive(true);
    }
    public void startBtn()
    {
        if (Time.timeScale > 0)     //if game is running, start game
        {
            gameObject.SetActive(true);
            winObj.SetActive(false);
            //despawn powerups:
            speedObj.SetActive(false);
            sizeObj.SetActive(false);
            deSpeedObj.SetActive(false);
            deSizeObj.SetActive(false);
            invisObj.SetActive(false);
            //disable powerup effects
            paddle1.transform.localScale = originalPaddleScale;
            paddle2.transform.localScale = originalPaddleScale;
            paddle1.GetComponent<paddleControl>().resetSpeed();
            paddle2.GetComponent<paddleControl>().resetSpeed();
            paddle1.GetComponent<SpriteRenderer>().color = originalColor;
            paddle2.GetComponent<SpriteRenderer>().color = originalColor;
            StopAllCoroutines();
            lastPlayerToScore = 2;
            p1Score = 0;
            p2Score = 0;
            setScore();     //also resets game and rallycount
        }
        else                //if paused, play
        {
            Time.timeScale = 1;
        }
    }
    public void pauseBtn()
    {
        Time.timeScale = 0;
    }
    public void setGrav()
    {
        rb.gravityScale = gravSlider.value;
    }
    IEnumerator applySpeed(GameObject paddle, float multiplier)
    {
        paddle.GetComponent<paddleControl>().speed = paddle.GetComponent<paddleControl>().speed * multiplier;
        yield return new WaitForSeconds(15);
        paddle.GetComponent<paddleControl>().speed = paddle.GetComponent<paddleControl>().speed / multiplier;
    }
    IEnumerator applySize(GameObject paddle, float multiplier)
    {
        paddle.transform.localScale = paddle.transform.localScale * multiplier;
        preventClipping(paddle);
        yield return new WaitForSeconds(15);
        paddle.transform.localScale = paddle.transform.localScale / multiplier;
    }
    IEnumerator applyInvis(GameObject paddle)
    {
        Color32 newColor = originalColor;
        newColor.a = 20;
        paddle.GetComponent<SpriteRenderer>().color = newColor;
        paddle.GetComponent<paddleControl>().botDifficulty /= 2;
        yield return new WaitForSeconds(15);
        paddle.GetComponent<SpriteRenderer>().color = originalColor;
        paddle.GetComponent<paddleControl>().botDifficulty *= 2;
    }
    void preventClipping(GameObject paddle)
    {
        paddleControl pc = paddle.GetComponent<paddleControl>();
        if (pc.touchingWall)
        {
            float wallThickness = 12;
            float wallPos = 230;
            float paddleHeight = 65;
            float paddleScale = paddle.transform.localScale.y / 12f;
            int heightSign = paddle.transform.localPosition.y > 0 ? 1 : -1;
            float maxY = (float)heightSign*(wallPos - wallThickness/2 - paddleHeight*paddleScale / 2);
            paddle.transform.localPosition = new Vector3(paddle.transform.localPosition.x, maxY, paddle.transform.localPosition.z);
        }
    }
}
