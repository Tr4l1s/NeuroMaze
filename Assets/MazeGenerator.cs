using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public GameObject wallPrefab;
    public int width = 20;
    public int height = 20;
    public float wallSize = 1f;

    private int[,] maze;
    private Vector2Int[] directions = {
        new Vector2Int(0, 1),  // yukarý
        new Vector2Int(1, 0),  // sað
        new Vector2Int(0, -1), // aþaðý
        new Vector2Int(-1, 0)  // sol
    };

    void Start()
    {
        GenerateMaze();
        DrawMaze();
    }

    void GenerateMaze()
    {
        int w = width * 2 + 1;
        int h = height * 2 + 1;
        maze = new int[w, h];

        // tüm alaný duvarla baþlat
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                maze[x, y] = 1;

        // baþlangýç noktasý
        Vector2Int current = new Vector2Int(1, 1);
        maze[current.x, current.y] = 0;

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(current);

        System.Random rand = new System.Random();

        // DFS algoritmasý
        while (stack.Count > 0)
        {
            current = stack.Pop();
            var shuffledDirs = new List<Vector2Int>(directions);
            shuffledDirs.Sort((a, b) => rand.Next(-1, 2));

            foreach (var dir in shuffledDirs)
            {
                Vector2Int next = current + dir * 2;

                if (next.x > 0 && next.y > 0 && next.x < w - 1 && next.y < h - 1 && maze[next.x, next.y] == 1)
                {
                    maze[next.x, next.y] = 0;
                    maze[current.x + dir.x, current.y + dir.y] = 0;
                    stack.Push(next);
                }
            }
        }

        // çýkýþ noktasý (sað alt köþe)
        maze[w - 2, h - 2] = 0;
    }

    void DrawMaze()
    {
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int z = 0; z < maze.GetLength(1); z++)
            {
                if (maze[x, z] == 1)
                {
                    Vector3 pos = new Vector3(x * wallSize, 0, z * wallSize);
                    Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }
}
