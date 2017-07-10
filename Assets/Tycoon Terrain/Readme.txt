To the new user:

If this is your first time using this asset then it is recommend you start out by checking out the dem scene. Load it and start generating some worlds.

The magic happens in the game object TycoonTerrain -> World 
You'll notice that you can set world size in the game view. There are a few more settings to play around with that are hidden from the user though.
You find these by looking in the World Behaviour-component of World and expanding the field "World Data". 

The value that affects the heights of the world map most is:
"Height Random Type"
Linear - A very hilly map without flat areas
Gaussian - A good middle ground
Square root - A bit flatter
Two rands - Average of two random values for every height calculated using linear, so again, less hilly
Three rands - Even less hilly, many flat areas

"Hillyness"
Gives further control over how much hills you would like

"Treeyness"
How many trees a world has
0 gives no trees
1 makes trees grow everywhere they can
any other value in between will affect the cahance of a tree growing in a tile

"Wateriness"
How high the water surface is
0 makes water surface be at the lowest point of the map
1 makes it be at the highest point ie "waterworld"

"Time to regrow"
Number of seconds for grass in a tile to grow back after terraforming



After having played around with these values and generating a few worlds you should check the code in the class "WorldBehaviour" and read up a bit on the 
class description and read some of the comments to get a feel for how it works, depending on what you want to do.

If you want to get started making new buildings (most likely I guess..) you should at least read the description comment of the field "buildingTypes" to
get an understanding of how to make more types of buildings.

There's a content creator who calls himself "eracoon" who has made some great free assets that will go great with this engine (roads and buildings are actually 
made by him). Here are som links to his creations:

Rail assets:
http://opengameart.org/content/rail-basic-assets-v1

Suburb assets:
http://opengameart.org/content/suburb-asset-pt1

Landscape assets:
http://opengameart.org/content/landscape-asset-v2

He also has some stuff up on asset store if these are not enough:
https://www.assetstore.unity3d.com/en/#!/publisher/9258

I'd also like to mention Kenney, some of his free assets have been used in this asset (trees and GUI)
http://opengameart.org/users/kenney


When you've successfully managed to add some buildings to the game you're pretty much set to go! 

Have as much fun using this as I had making it!
Cheers, 
Mike
Viking Development Crew