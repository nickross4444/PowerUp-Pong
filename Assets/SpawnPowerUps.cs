using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnPowerUps : MonoBehaviour
{
    public GameObject ball, speed, deSpeed, size, deSize, invis;
    public Toggle powerUpTog;


    // Update is called once per frame
    void Update()
    {
        int r = (int)Random.Range(0, 3000);
        //int r = (int)Random.Range(0, 30);

        if (Mathf.Abs(ball.GetComponent<Rigidbody2D>().velocity.x) < 1 || Time.timeScale == 0 || !powerUpTog.isOn)       //returns when paused or not started
        {
            return;
        }
        else if (r == 0 && !speed.activeSelf)
        {
            speed.transform.localPosition = new Vector3(0, Random.Range(-210, 210), 0);
            speed.SetActive(true);
        }
        else if (r == 1 && !deSpeed.activeSelf)
        {
            deSpeed.transform.localPosition = new Vector3(0, Random.Range(-210, 210), 0);
            deSpeed.SetActive(true);
        }
        else if (r == 2 && !size.activeSelf)
        {
            size.transform.localPosition = new Vector3(0, Random.Range(-210, 210), 0);
            size.SetActive(true);
        }
        else if (r == 3 && !deSize.activeSelf)
        {
            deSize.transform.localPosition = new Vector3(0, Random.Range(-210, 210), 0);
            deSize.SetActive(true);
        }
        else if (r == 4 && !invis.activeSelf)
        {
            invis.transform.localPosition = new Vector3(0, Random.Range(-210, 210), 0);
            invis.SetActive(true);
        }

    }
}
