What is this? 🤔
This project is about exploring different Search Space representation and Pathfinding based on Chapter 5 and 8 of Mat Buckland's book

Given
1. There is a Kinematic and a Dynamic version of the motor that do not support jumping, sliding, and moving platforms 
2. Kinematic and Dynamic versions of Seek and Arrive
3. Different Weebles with various preexisting properties
    - Blue Weeble: Kinematic Motor and uses Kinematic Steering
    - Red Weeble: Dynamic Motor and uses Dynamic Steering
        * Note: You want to enable the Target Selector to experiment
4. Map Generators (Maze & ConnectedRooms)
    * Note: You can vary the cell sizes or even shut-off map generation so you can use your own map.
5. Search Space representations
    - Grid (with/without diagonals)
    - PointsOfVisibility 
    * Note: Try turning on the node and edge displays to see the differences.
6. PathManager -Attached to Weeble to manage steering behaviour
    - You can manually request a path by checking the PathManager’s Request Path checkbox
    - You can disable the Target Selector and enable the Goal Selector - which will let you request paths to the selected objects 

TODO
1. [Documentation] Document your work. Explain what parts you attempted, how your approach is supposed to work, which parts of the code you modified, removed or added. 

2. [Bug Fixing] 
    a) 
        When: switching paths
        What: two issues
            1. When you are following a path and select a new goal in the Goal Selector, the path is found from the current location but the Weeble continues traversing the current edge before switching.
            Solution: You need to provide a way to gracefully switch (cancel traversal of the current edge). 
            2. If the path is being shown when you switch paths, the initial markers are not correctly cleaned up. This is related to the first part.
    b) 
        When: travelling in path
        What: Path sometimes goes through a wall :( 
    c) 
        When: To be determined.
        What: Deal with situations where the Weebles get stuck or take too long to complete an edge or the path.

3. [Implementation ideas]
    b) Over and under:
        There is a half-bridge provided. Modify or create at least one search space representation that allows Weebles to correctly path over and under the bridge. The bridge is deliberately one way! This may require using a directed graph (which is an option in the underlying sparse graph implementation …).
    c) Create new PathManager and replace individual PathManger attached to Weeble:
        iii) Implement Quick Path.
    d) Implement path smoothing.

4. Find ways to make it awesome!
    a) Implement Manhattan distance 
    b) Make corner graph work with mazes 
In progress:
3c ii)

Completed: 
3a) Implement and test the Corner Graph search space representations on the Connected Rooms search space.
3c) i) Design, implement and test a new path manager (attach it to “Game”). This single path manger should receive path requests from all the Weebles. It should queue the requests and process them. It should then send a path request completed (or failed) event to the requestor.
 ii) Now redesign your PathManager to time-slice processing requests (like an operating system does with jobs). To do this, you will need to replace the A* implementation with a time-sliced version (this is easier than it sounds). Basically, each update, you want to do one cycle of A* for each requestor. One cycle means remove the next node, process it, add its children, and return.
    - Except it won't switch goals - which make the implementation a little troublesome

