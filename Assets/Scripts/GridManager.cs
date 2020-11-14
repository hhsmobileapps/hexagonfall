using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public List<Color> hexColors = new List<Color>(){ Color.red, Color.green, Color.yellow, Color.blue, Color.magenta};
    public GameObject hexagonTile, bombTile;
    public int numOfRows = 9, numOfColumns = 8;
    public AudioClip rotateSfx, matchSfx;
    
    private Vector3[,] gridPositions;
    private List<GameObject> closestTriad;
    private Vector3 lastTouchPos;

    bool isMatchingContinues = false;
    
    void Start()
    {
        instance = GetComponent<GridManager>();
        // get the width - height of the hexagon sprite
        Vector2 offset = hexagonTile.GetComponent<SpriteRenderer>().bounds.size;
        // instantiate the grid elements
        CreateGrid(offset.x, offset.y);
    }

    // Instantiates the hexagons at their correct positions
    private void CreateGrid(float xOffset, float yOffset)
    {
        gridPositions = new Vector3[numOfColumns, numOfRows];
        closestTriad = new List<GameObject>();

        float startX = transform.position.x;
        float startY = transform.position.y;

        for (int x = 0; x < numOfColumns; x++)
        {
            for (int y = 0; y < numOfRows; y++)
            {
                // added an offset for odd columns to obtain the requested result
                Vector3 pos = new Vector3(startX + (x * xOffset * 1.5f / 2), startY + (y * yOffset) + ((x % 2 == 0) ? 0 : yOffset / 2), 0);
                pos = 1.1f * pos; // to leave a little space between hexagons
                gridPositions[x, y] = pos;
                // Get the only possible colors for that place and select one randomly
                List<Color> possibleColors = getPossibleColors(x, y);
                // Instantiate the hexagon, give its name and color
                GameObject newTile = Instantiate(hexagonTile, pos, hexagonTile.transform.rotation, transform);   
                newTile.name = x + "_" + y;
                newTile.GetComponent<SpriteRenderer>().color = possibleColors[Random.Range(0, possibleColors.Count)];
            }
        }
    }

    // This method filters the acceptable colors to avoid matching at initialization of the grid
    List<Color> getPossibleColors(int x, int y)
    {
        // keep all available colors in the list first
        List<Color> possibleColors = new List<Color>();
        possibleColors.AddRange(hexColors);

        Vector2[] adjacentDirs = // this is the search direction pairs
        new Vector2[] { new Vector2(0,1), new Vector2(1.25f,1), new Vector2(1.25f,-1),
                        new Vector2(0,-1), new Vector2(-1.25f,-1), new Vector2(-1.25f,1), new Vector2(0,1)};

        // send raycast around to check whether there exists two hexagon with same color
        for (int i = 0; i < adjacentDirs.Length - 1; i++)
        {
            RaycastHit2D hit1 = Physics2D.Raycast(gridPositions[x, y], adjacentDirs[i]);
            RaycastHit2D hit2 = Physics2D.Raycast(gridPositions[x, y], adjacentDirs[i + 1]);
            if (hit1.collider != null && hit2.collider != null)
            {
                Color color1 = hit1.collider.gameObject.GetComponent<SpriteRenderer>().color;
                Color color2 = hit2.collider.gameObject.GetComponent<SpriteRenderer>().color;
                // if same, then remove this color as we shouldn't use it for this position
                if (color1 == color2) 
                {
                    possibleColors.Remove(color1);
                }
            }
        }
        return possibleColors;
    }

    // Gets the most updated hexagon list
    List<GameObject> getCurrentHexagons()
    {
        List<GameObject> current = new List<GameObject>();
        foreach (Transform child in transform)
        {
            current.Add(child.gameObject);
        }
        return current;
    }

    // This method selects 3 hexagons that are closest to the point where user touches
    public void SelectTriad(Vector3 touchPos)
    {
        // Don't allow selection while matching continues or game is over
        if (isMatchingContinues || FindObjectOfType<GameManager>().gameOverPanel.activeSelf)
            return;

        lastTouchPos = touchPos;
        closestTriad = new List<GameObject>();
        DeselectAllHexagons(); // Remove selection from all at first

        // order the hexagons by their distance to the touch position
        List<GameObject> currentHexagons = getCurrentHexagons();
        currentHexagons = currentHexagons.OrderBy(g => (g.transform.position - touchPos).sqrMagnitude).ToList();

        // apply selection material to the selected ones (first 3)
        for (int i = 0; i < currentHexagons.Count; i++)
        {
            if (i < 3)
            {
                currentHexagons[i].GetComponent<Hexagon>().Select();
                closestTriad.Add(currentHexagons[i]);
            }
        }
        
        // sort the closest 3 by their name
        closestTriad = closestTriad
            .OrderBy(g => int.Parse(g.name.Split('_')[0]))
            .ThenBy(g => int.Parse(g.name.Split('_')[1]))
            .ToList();

        // This part is added to avoid selecting wrong triads (when user touches outside of the board)
        int x1 = int.Parse(closestTriad[0].name.Split('_')[0]);
        int y1 = int.Parse(closestTriad[0].name.Split('_')[1]);
        int x2 = int.Parse(closestTriad[2].name.Split('_')[0]);
        int y2 = int.Parse(closestTriad[2].name.Split('_')[1]);
        if (Mathf.Abs(x1 - x2) >= 2 || Mathf.Abs(y1 - y2) >= 2)
        {
            closestTriad = new List<GameObject>();
            DeselectAllHexagons();
        }
    }

    // Removes the selection indication from all hexagons
    public void DeselectAllHexagons()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<Hexagon>().DeSelect();
        }        
    }

    // Rotates the 3 hexagon with respect to the user's swipe movement
    public void RotateTriad(int direction) // direction: 1 => clockwise 2 => counter-clockwise
    {
        // Don't allow selection while matching continues or game is over
        if (isMatchingContinues || FindObjectOfType<GameManager>().gameOverPanel.activeSelf)
            return;

        if (closestTriad.Count == 3)
        {
            StartCoroutine(RotateAsync(direction));
        }
        
    }

    IEnumerator RotateAsync(int direction)
    {
        // Increment the main move counter by 1
        FindObjectOfType<GameManager>().IncrementMoves();

        for (int i = 0; i < closestTriad.Count; i++)
        {

            SoundManager.Instance.PlaySFX(rotateSfx);

            // sort the closest 3 by their name
            closestTriad = closestTriad
                .OrderBy(g => int.Parse(g.name.Split('_')[0]))
                .ThenBy(g => int.Parse(g.name.Split('_')[1]))
                .ToList();

            int x1 = int.Parse(closestTriad[0].name.Split('_')[0]);
            int y1 = int.Parse(closestTriad[0].name.Split('_')[1]);
            int x2 = int.Parse(closestTriad[1].name.Split('_')[0]);
            int y2 = int.Parse(closestTriad[1].name.Split('_')[1]);

            // Rotate the three hexagons (it depends on the positions of hexagons)
            if ((x1 == x2 && direction == 1) || (y1 == y2 && direction == 2) || (x2 > x1 && y1 > y2 && direction == 2))
            {
                string name = closestTriad[0].name;
                Vector3 pos = closestTriad[0].transform.position;
                closestTriad[0].name = closestTriad[1].name;
                closestTriad[0].transform.position = closestTriad[1].transform.position;
                closestTriad[1].name = closestTriad[2].name;
                closestTriad[1].transform.position = closestTriad[2].transform.position;
                closestTriad[2].name = name;
                closestTriad[2].transform.position = pos;
            }
            else
            {
                string name = closestTriad[0].name;
                Vector3 pos = closestTriad[0].transform.position;
                closestTriad[0].name = closestTriad[2].name;
                closestTriad[0].transform.position = closestTriad[2].transform.position;
                closestTriad[2].name = closestTriad[1].name;
                closestTriad[2].transform.position = closestTriad[1].transform.position;
                closestTriad[1].name = name;
                closestTriad[1].transform.position = pos;
            }

            yield return new WaitForSeconds(.25f);

            // check whether each hexagon (from 3) is matching in it's new rotated position
            foreach (var item in closestTriad)
            {
                List<GameObject> matchedHexagons = item.GetComponent<Hexagon>().GetMatchedHexagons();
                if (matchedHexagons.Count > 0) // means matching occured
                {
                    DeselectAllHexagons();
                    isMatchingContinues = true;
                    SoundManager.Instance.PlaySFX(matchSfx);
                    foreach (var hexagon in matchedHexagons)
                    {
                        // if it is the bomb that is matched, reset it's counter
                        if (hexagon.GetComponent<Bomb>())
                            hexagon.GetComponent<Bomb>().ResetRemainingCounter();

                        Destroy(hexagon);
                    }
                    FindObjectOfType<GameManager>().IncrementScore(matchedHexagons.Count);
                    CheckEmptySpaces();
                    yield break;
                }
            }
        }
    }
    
    void CheckEmptySpaces()
    {
        StartCoroutine(CheckEmptyAsync());
    }

    IEnumerator CheckEmptyAsync()
    {
        for (int x = 0; x < numOfColumns; x++)
        {
            int lastEmptyIndex = numOfRows;
            // Shift the positions of hexagons downwards
            for (int y = 0; y < numOfRows; y++)
            {
                RaycastHit2D hit1 = Physics2D.Raycast(gridPositions[x, y], Vector3.forward);
                if (hit1.collider == null) // Means this position is empty (ray didn't hit to any object)         
                {
                    RaycastHit2D hit2 = Physics2D.Raycast(gridPositions[x, y], Vector3.up);
                    if (hit2.collider != null) // means there is hexagon above (fall it)
                    {
                        hit2.collider.gameObject.transform.position = gridPositions[x, y];
                        hit2.collider.gameObject.name = x + "_" + y;

                        yield return new WaitForSeconds(.05f); // required for raycast
                    }
                    else // there is no hexagon above
                    {
                        lastEmptyIndex = y;
                        break;
                    }
                }
            }

            // fill the spaces
            for (int i = lastEmptyIndex; i < numOfRows; i++)
            {
                Vector3 pos = gridPositions[x, i];

                // Select available colors (To avoid the matching at initialization esp for the bomb)
                List<Color> possibleColors = getPossibleColors(x, i);

                GameObject newTile;
                // if user has reached to score of multiples of 1000 then Instantiate a single bomb, otherwise instantiate hexagon
                if (FindObjectOfType<GameManager>().IsBombTime())
                    newTile = Instantiate(bombTile, pos, bombTile.transform.rotation, transform);
                else 
                    newTile = Instantiate(hexagonTile, pos, hexagonTile.transform.rotation, transform);
                // give its name and color
                newTile.name = x + "_" + i;
                newTile.GetComponent<SpriteRenderer>().color = possibleColors[Random.Range(0, possibleColors.Count)];
            }
        }

        // check for each hexagon whether a matching occurs or not (as the colors on the board has changed)
        // Continue this check until no match occurs (recursive)
        foreach (Transform child in transform)
        {
            List<GameObject> matchedHexagons = child.GetComponent<Hexagon>().GetMatchedHexagons();
            if (matchedHexagons.Count > 0) // means matching occured
            {
                yield return new WaitForSeconds(.75f);

                SoundManager.Instance.PlaySFX(matchSfx);
                foreach (var hexagon in matchedHexagons)
                {
                    // if it is the bomb that is matched, reset it's counter
                    if (hexagon.GetComponent<Bomb>())
                        hexagon.GetComponent<Bomb>().ResetRemainingCounter();
                    Destroy(hexagon);
                }
                FindObjectOfType<GameManager>().IncrementScore(matchedHexagons.Count);
                CheckEmptySpaces();
                yield break;
            }
        }

        isMatchingContinues = false; // Now chained matches finished
        SelectTriad(lastTouchPos); // Select the last touched triad again

        // Decrement the remaining move counter of the bomb and check game over (if exists)
        Bomb bomb = FindObjectOfType<Bomb>();
        if (bomb)
            bomb.CheckGameOver();
    }   
}
