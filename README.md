# Pathfinding

A simulation comparing three high-level pathfinding techniques (i.e. built on top of raw A*)

* Hierarchical A*
* Navigation Mesh
* Flow-Fields

### Hierarchical Pathfinding
In hierarchical pathfinding, we assume the existence of a grid world. We then impose a coarse grid on top of the already existing grid. In this way, we can utilise much of the same architecture in terms of grid functions, just with a coarser grid.
We then define connections between each coarse grid cell. We define a connection as “an area where the two cells marking the border between two coarse grid cells are both passable”.
Once we have defined these connections, we then define which connections connect to other connections. We do this by running a flood-fill from each connection with floods within the entire coarse grid cell it occupies. Whichever other border cells the floodfill reaches defines the connections which the source connection can reach.

Once we’ve defined connections between connections, we can treat this as a normal nav-graph and run a standard search algorithm.
The advantages of this algorithm is that it considers only a fraction of the nodes as a normal A*. If the coarse grid is 4x as large as the fine grid, then in the best case we can consider 16x less nodes. The other advantage is that it’s very easy to perform early outs as to whether or not a path is possible. If the source position cannot reach any connections in its coarse grid cell; or the target position cannot reach any connections in its coarse grid cell; then a path is impossible and we can abandon the search.
The disadvantages are that this algorithm does not perform well in a dynamic environment. It relies heavily on pre-computing and, as such, if we have an environment which is regularly changing, computation time is eaten up recomputing the hierarchical network.

### Nav-Mesh Pathfinding
In this algorithm, we do not consider each individual cell as a node. Rather, we combine groups of cells which are passable into larger, more coarse convex polygons denoted as “walkable areas”. By definition, a convex polygon allows direct line of sight (and therefore movement) to any other point within the convex polygon. Therefore, a series of connected convex polygons (regardless of their size) can be treated like a standard nav-graph.
To generate the series of convex polygons, we run a flood-fill algorithm from a starting point. We attempt to expand the quadrilateral polygon in each direction until it is blocked on all directions. Once it is, we denote it as a polygon as part of the nav mesh and start construction of a new polygon from one of the border cells of the previously constructed polygon.
The advantages of this algorithm is that it considers considerably less nodes than a standard A*. 

By grouping together nodes, we do not care about each individual node, but only about the larger polygons created by grouping together nodes. Additionally, nav-mesh pathfinding produces more realistic paths on average. Standard, grid-based pathfinding tends to hug walls very tightly since, from an individual node-based score method, there is no incentive to deviate from a wall if following it yields the lowest score. In nav-mesh pathfinding, we can path between the centre point of larger nodes. This means that we are less likely to be right next to a wall since a larger node will have a centre further away from a wall than that of a strictly one cell-sized node.
The disadvantages of this technique is that it’s very expensive to calculate the nav mesh. Additionally, if we localize calculations on the fly as new obstacles are presented, we will only compromise the quality of our nav-mesh. If we only re-calculate locally then, by definition, If a new obstacle is added to a cell it will become a minimum of 4 smaller cells. Since the whole point of nav-mesh pathfinding is to reduce the number of nodes, allowing it to become progressively worse in a dynamic environment is unacceptable. The only other alternative is to recalculate the entire nav-mesh whenever it is changed, which is extremely expensive.

### Flow-Field Pathfining
Flow-Field pathfinding is unique in that it does not care about the source of the path request – only the destination. It runs a Dijkstra’s algorithm from the target node and assigns a “flow vector” to each node in the search which says “If I’m standing on this node, follow this vector to reach the target”.
The advantages of this algorithm are that it is extremely easy to implement. 

To implement, it is essentially a Dijkstra’s algorithm that doesn’t care about parents – easy! However, the major advantage is how it scales with entity numbers.  Regardless of the number of entities. Only one path will ever need to be calculated. The flow-field spreads throughout the entire map. Therefore, the process for every single entity to follow the path is the same. They need only look at the cell they’re standing on and move according to the “flow vector” stored in that cell
The disadvantages of this algorithm are two-fold. Firstly, it cannot be smoothed in any realistic sense since there is no knowledge of the source node. This can lead to unrealistic paths. Secondly, it scales extremely badly with map size. It relies on a Dijkstra’s algorithm which is very slow to calculate. In large maps, this is not feasible unless it is time-sliced. Even then, other approaches are more suitable



