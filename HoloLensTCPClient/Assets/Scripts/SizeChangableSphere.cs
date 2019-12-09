using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeChangableSphere : MonoBehaviour {

    private Renderer renderer;
	// Use this for initialization
	void Start () {
        renderer = this.GetComponent<Renderer>();

        renderer.material.color = new Color(1.0f, 0.92f, 0.016f, 1.0f);
        this.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

    }

    // Update is called once per frame
    void Update () {
        var headPosition = Camera.main.transform.position;
        var distance = Vector3.Distance(this.transform.position , headPosition);

        print("DISTANCE:    " + distance.ToString());



        if (distance <= 0.2)
        {
            renderer.material.color = new Color(1.0f, 0.92f, 0.016f, 0.0f);

        }
        else if (distance <= 0.3)
        {
            this.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            renderer.material.color = new Color(1.0f, 0.92f, 0.016f, distance * 5.0f);

        } else if (distance < 3)
        {
            var size = (distance - 0.3f) * (distance - 0.3f) * 0.13f + 0.01f;
            print("SIZEEE    " + size.ToString());

            this.transform.localScale = new Vector3(size, size, size);
            renderer.material.color = new Color(1.0f, 0.92f, 0.016f, 1.0f);

        }
        


    }
}
