using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;

namespace TerrariaAmbience.Common; 

/// <summary>
/// A struct representing a calculated path using the A* algorithm.
/// Can be iterated over directly to access path nodes.
/// </summary>
public struct AStarPath : IEnumerable<Point> {
    public Point Start { get; private set; }
    public Point End { get; private set; }
    public List<Point> Nodes { get; private set; }

    // pixel distance
    public readonly float Distance => (Nodes?.Count ?? 0) * 16f;

    public readonly bool Found => Nodes != null && Nodes.Count > 0;

    public AStarPath(Point start, Point end) {
        Start = start;
        End = end;
        Nodes = [];

        Nodes = FindPath(start, end);
    }

    public readonly IEnumerator<Point> GetEnumerator() => (Nodes ?? []).GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static List<Point> FindPath(Point start, Point end) {
        HashSet<Point> openSet = [];
        openSet.Add(start);

        // for node n, cameFrom[n] is the node immediately preceding it on the cheapest path from start
        Dictionary<Point, Point> cameFrom = [];

        // gScore[n] is the cost of the cheapest path from start to n
        Dictionary<Point, float> gScore = [];
        gScore[start] = 0;

        // fScore[n] = gScore[n] + h(n)
        // fScore[n] represents current best guess 
        Dictionary<Point, float> fScore = [];
        fScore[start] = Heuristic(start, end);

        int maxIterations = 4000;
        int iterations = 0;

        while (openSet.Count > 0) {
            iterations++;
            if (iterations > maxIterations) break;

            var current = GetLowestFScore(openSet, fScore);

            if (current == end)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current)) {
                // Cost from current to neighbor is 1 (assumes grid movement)
                float tentativeGScore = gScore[current] + 1;

                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue)) {
                    // neighbor is better than any previous
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);

                    openSet.Add(neighbor);
                }
            }
        }
        return [];
    }

    static float Heuristic(Point a, Point b) {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    static Point GetLowestFScore(HashSet<Point> openSet, Dictionary<Point, float> fScore) {
        Point bestNode = new(0, 0);
        float lowestScore = float.MaxValue;
        bool first = true;

        foreach (Point p in openSet) {
            float score = fScore.GetValueOrDefault(p, float.MaxValue);
            if (first || score < lowestScore) {
                lowestScore = score;
                bestNode = p;
                first = false;
            }
        }
        return bestNode;
    }

    static List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current) {
        List<Point> totalPath = [current];
        while (cameFrom.ContainsKey(current)) {
            current = cameFrom[current];
            totalPath.Add(current);
        }
        totalPath.Reverse(); // start -> end
        return totalPath;
    }

    static List<Point> GetNeighbors(Point p) {
        List<Point> neighbors = [];
        // maybe use diagonals at some point
        Point[] offsets = [new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0)];

        foreach (Point offset in offsets) {
            var next = new Point(p.X + offset.X, p.Y + offset.Y);

            if (next.X < 0 || next.Y < 0 || next.X >= Main.maxTilesX || next.Y >= Main.maxTilesY)
                continue;

            if (IsWalkable(next))
                neighbors.Add(next);
        }
        return neighbors;
    }

    static bool IsWalkable(Point p) {
        Tile tile = Main.tile[p.X, p.Y];

        // only traverse into non-solid tiles/non-permeable tiles
        if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) {
            return false;
        }

        return true;
    }
}