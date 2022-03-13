using System;
using System.Collections.Generic;
using Keybored.BackendServer.GameLevel;
using Keybored.BackendServer.GameEngine;


namespace Keybored.BackendServer.AI {

    public class PriorityQueue<T>{
        List<Tuple<T, float>> list;

        public PriorityQueue(){
            list = new List<Tuple<T, float>>();
        }

        //Use later for speed...
        public PriorityQueue(int Capacity){
            list = new List<Tuple<T, float>>();
            list.Capacity = Capacity;
        }

        public void Enqueue(T item, float priority){
            list.Add( Tuple.Create(item, priority) );
        }

        public T Dequeue(){
            int bestIndex = 0;
            int len = list.Count;
            for(int i=0; i<len; i++){
                if(list[i].Item2 < list[bestIndex].Item2){
                    bestIndex = i;
                }
            }
            
            T item = list[bestIndex].Item1;
            list.RemoveAt(bestIndex);
            return item;
        }

        public int Count{
            get{
                return list.Count;
            }
        }
    }

    public static class AStar {
        private static List<Vector3Thin> processPath(Vector3Thin start, Vector3Thin goal, Dictionary<Vector3Thin, Vector3Thin> came_from) {
            Vector3Thin current = goal;
            List<Vector3Thin> path = new List<Vector3Thin>();

            while (!Vector3Thin.Same(current, start)){
                path.Add(current);
                current = came_from[current];
            }
            
            path.Reverse(); //Quick fix for now. This changes the path from goal-to-start to start-to-goal!
            return path;
        }

        private static Vector3Thin[] getNeighbors(Vector3Thin gridPos){ //"gridPos" should be rounded to ints!
            Vector3Thin[] neighbors = new Vector3Thin[4]{
                new Vector3Thin(gridPos.x + 1, gridPos.y),
                new Vector3Thin(gridPos.x - 1, gridPos.y),
                new Vector3Thin(gridPos.x, gridPos.y + 1),
                new Vector3Thin(gridPos.x, gridPos.y - 1)
            };
            return neighbors;
        }

        private static float heuristic(Vector3Thin pos1, Vector3Thin pos2){
            // Manhattan distance on a square grid. Also called "Taxicab geometry": https://en.wikipedia.org/wiki/Taxicab_geometry
            return Math.Abs(pos2.x - pos1.x) + Math.Abs(pos2.y - pos1.y);
        }

        public static List<Vector3Thin> FindPath(Vector3Thin start, Vector3Thin goal, MapData mapData){
            start = start.RoundToInt(); //Round the first pos so all neighbors are ints!
            goal = goal.RoundToInt(); //Round the last pos so we can tell when we reached the goal.

            PriorityQueue<Vector3Thin> frontier = new PriorityQueue<Vector3Thin>();
            frontier.Enqueue(start, 0);

            Vector3ThinEqualityComparer EqlCom = new Vector3ThinEqualityComparer();
            Dictionary<Vector3Thin, Vector3Thin> came_from = new Dictionary<Vector3Thin, Vector3Thin>(EqlCom);
            Dictionary<Vector3Thin, float> cost_so_far = new Dictionary<Vector3Thin, float>(EqlCom);
            came_from[start] = null;
            cost_so_far[start] = 0f;

            int counter = 0;
            int counterMax = 500; //Limit how long the bot can hog the CPU looking for a path to travel on.

            while(frontier.Count > 0) {
                counter++;
                Vector3Thin current = frontier.Dequeue();

                if(current == goal || counter >= counterMax){ return processPath(start, current, came_from); } //I added this one line!

                Vector3Thin[] neighbors = getNeighbors(current);
                foreach(Vector3Thin next in neighbors){
                    float new_cost = cost_so_far[current] + 1; //Right now each grid/graph block only costs +1 to move there... (Movement cost is the samething as weight)
                    if(!mapData.posHitsWall(next) && (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next]) ){
                        cost_so_far[next] = new_cost;
                        float priority = new_cost + heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        came_from[next] = current;
                        //goes_to[came_from[current]] = current; //This will be re-writen for each different neighbor, si that okay? Will the last neighbor to re-write it be the one need for the final path?
                    }
                }
            }

            return new List<Vector3Thin>(); //This should never happend!
        }
    }
}













/*

NOTES:

* For priority queue, try using sortedList<int, vector3Thin> for binary heap: https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.sortedlist-2?view=netframework-4.8
(Also see: https://stackoverflow.com/questions/2231796/heap-class-in-net)

* 


*/