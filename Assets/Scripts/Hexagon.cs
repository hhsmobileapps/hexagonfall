using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hexagon : MonoBehaviour
{
    public Material defaultMat, selectedMat;
      

    public void Select()
    {
        gameObject.GetComponent<SpriteRenderer>().material = selectedMat;
    }

    public void DeSelect()
    {
        gameObject.GetComponent<SpriteRenderer>().material = defaultMat;
    }

    // Finds the matched hexagons around this gameobject and returns them
    public List<GameObject> GetMatchedHexagons()
    {
        List<GameObject> matchedObjects = new List<GameObject>();

        Vector2[] adjacentDirs = // this is the search direction pairs
        new Vector2[] { new Vector2(0,1), new Vector2(1.25f,1), new Vector2(1.25f,-1),
                        new Vector2(0,-1), new Vector2(-1.25f,-1), new Vector2(-1.25f,1), new Vector2(0,1)};

        gameObject.GetComponent<Collider2D>().enabled = false;

        for (int i = 0; i < adjacentDirs.Length - 1; i++)
        {
            RaycastHit2D hit1 = Physics2D.Raycast(transform.position, adjacentDirs[i]);
            RaycastHit2D hit2 = Physics2D.Raycast(transform.position, adjacentDirs[i + 1]);
            if (hit1.collider != null && hit2.collider != null)
            {
                Color color1 = gameObject.GetComponent<SpriteRenderer>().color;
                Color color2 = hit1.collider.gameObject.GetComponent<SpriteRenderer>().color;
                Color color3 = hit2.collider.gameObject.GetComponent<SpriteRenderer>().color;

                if (color1 == color2 && color2 == color3) // match occurs
                {
                    matchedObjects
                        .AddRange(new List<GameObject>() { gameObject, hit1.collider.gameObject, hit2.collider.gameObject });
                }
            }
        }
        gameObject.GetComponent<Collider2D>().enabled = true;

        matchedObjects = matchedObjects.Distinct().ToList(); // remove dublicates

        return matchedObjects;
    }
}