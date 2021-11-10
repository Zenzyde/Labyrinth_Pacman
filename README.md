# Labyrinth_Pacman
A re-imagined mini-project version of the classic "Pac-man". The player moves through a procedurally generated labyrinth in search of powerups while trying to avoid enemy AI and survive for as long as possible.<br/>
The labyrinth is generated based on the BSP-algorithm with slight modification.<br/>
The enemy AI uses a small variety of pathfinding algorithms, including but not limited to:
* A*
* Drunken Walk
* "Straight Line" to player (not an algorithm, just always going straight towards the player without going through walls....yes it gets stuck a lot but that's the point of it)
* "Seek Powerup" uses the A* algorithm but pathfinds to random powerups on the map instead of the player, meant as an optional nuisanse more than an active threat
