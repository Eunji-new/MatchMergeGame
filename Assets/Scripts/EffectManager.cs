using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{

    public GameObject popping;
    public GameObject popping_special;
    public int maxCnt;
    Queue<GameObject> poppings = new Queue<GameObject>();
    Queue<GameObject> poppings_special = new Queue<GameObject>();

    public static EffectManager instance;

    private void Awake() {
        instance = this;
    }
    private void Start() {
        transform.SetAsLastSibling();
    }
    public IEnumerator StartPopping(bool isSpecial, Vector3 position)
    {   
        Queue<GameObject> popQueue;
        GameObject popObj;
        if(isSpecial)
        {
            popQueue = poppings_special;
            popObj = popping_special;
        }
        else
        {
            popQueue = poppings;
            popObj = popping;
        }
        
        GameObject g;
        if(popQueue.Count > 0)
        {
            g = popQueue.Dequeue();
            g.SetActive(true);  
            SetEffectPos(g, position);
            g.transform.SetParent(this.transform);
        }
        else
        {
            g = Instantiate(popObj, position, Quaternion.identity);
            g.transform.SetParent(this.transform);
        }

            g.GetComponent<ParticleSystem>().Play(); 
            yield return new WaitForSeconds(1);
            g.SetActive(false);
            popQueue.Enqueue(g);

        
    }

    public void SetEffectPos(GameObject particleObj, Vector3 position)
    {
        particleObj.transform.position = position;
    }
}
