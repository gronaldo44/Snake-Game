#authors Ronald Foster, Shem Snow

# Organization
------------------
We used the Model, View, Controller organization style to separate each of those three concerns into their own Folder.

-The model consists of a World class that represents the world at any given time by containing collections of 
all the objects that exist in the game (snakes, powerups, walls). Each one of those objects is defined by their own class
which describes their current state in the game (location, alive, etc..). In other words, the model is just all the world 
data at a particular instance in time. As the program runs, objects in the world will change.

-The controller abstracts all of the networking/communication concerns of our program. We used two controller classes in 
our program. The first is the network controller from PS7 that manages the sockets between the server and clients. The 
second contains multiple 'listeners'/methods which will be called any time the user performs an action that involves the server. 
This is done by giving the Game Controller a reference to the Network Controller then, in the Game Controller, setting a 
delegate within the Network Controller's socket state (OnNetworkAction) to one of Game Controller's own methods. 

- The view (which we called SnakeClient) handles the interface between the user and the game. It consists of a MAUI 
application and two important classes: "MainPage" which handled the actual view/panel/window that the game is played on 
and "WorldPanel" which simply drew pictures onto the MainPage.


# Design Decisions
------------------
We produced the program in multiple steps/stages formated as "version.stage.concern" and noted the start and completion
date of each one.
Each day we met, our goal would be to finish all of the concerns within one stage. 0.3.x for example was all of the Model's 
concerns. The completed program would be version 1.0.0 and any additional features could be added after completion.


	Vers	Description					Skeleton Written	Completed
	0.1.0: Connecting to a server				---				11/18
	0.2.0: Controller									11/26
	0.3.0: The World [model]				11/17				11/21
	0.3.1: Walls						11/17				11/25
	0.3.2: Snakes						11/17				11/19
	0.3.3: Power-ups					11/17				11/19
	0.4.0: Update On Each Frame				11/18				11/26			
	0.4.2: De-serialize					11/18				11/26
	0.4.4: Draw Powerups and Snakes				11/17				11/19
	1.0.0: Additional Features				11/26				11/27

# Design Notes
--------------
	# 0.1
		- The MainPage has a reference to the GameController as well as a button and text-entries for initiating
		the connnection. To initiate a connection, it simply uses its controller reference to call connect.

		- The GameController just takes the server name and playername then calls the Networking 
		Controller's connect method so it doesn't have to include any socket logic.

	# 0.2
		- The Game Controller has a reference to the Networking controller (which handles Socket connections) as
		well as several methods for requesting some action in the server.

		- In order to decide which method should be called and when in order to avoid concurrency problems.
		Our solution was for the Network Controller just to have a single delegate called "OnNetworkAction" 
		and each action would reset that delegate to call another method so that it was impossible for them to 
		happen out of order.

		- The order in which the delegates would be called is: Connect, OnConnect, GetPlayerIDAndWorldSize, GetWalls, 
		and OnFrame.

		- The GameController also kept track of the direction the snake was moving and would send it off to the 
		NetWorkController every time the OnFrame method was called (the server constantly calls itt indirectly 
		through the UpdateWorld method in the model).

	# 0.3

		- The Model is essentially just a World object containing a bunch of JSON-compatible objects and a single 
		method "UpdateWorld" which receives a string of the changes to be made.

		- The challenges we had to overcome was parsing the string of new data (which we did with a regular expression)
		and locking the world object every time an update was made.

		- The trickiest problem we overcame was removing old no-longer-useful data from a socket each time the 
		world was updated. Doing so greatly increased the game's performance.

	#0.4
		- Communications between the server and client are terminated with a "\n"
		- Snakes are drawn one segment at a time moving head-to-tail.
		- World 'wrap-around' of the snake is handled by checking to see if a current segment is at the world 
		border and if it is, then the next segment will not be drawn.

		- Walls are drawn by 'placing' a 'drawer' onto the panel at a given position and orientation then moving 
		forward to draw each wall segment.

		- The greatest challenges here were shifting the wall sprite by 25 units 
		and getting its orientation right so the wall's position in the game was accurate. We did these by comparing 
		x and y coordinates of the start and end points of each wall sprite. Whether or not the value of start was 
		less than the value of end would determine which direction to draw the sprites in.

Instructions for playing
------------------------
Each player in the snake game must communicate to the server using their own SnakeClient program (that's this program).
Separately, a SnakeServer program will be running the actual game. The player can run this program on its own but it will 
not connect to a game unless the server is already be running. To connect, enter a server name and player name then click
on "Connect". There is already a default server name of localhost for when the server is played on a single computer by 
only one person. After the connection is made, the player moves around using the WASD keys and as they collect powerups,
their snakes size (and score) will increase.

#PS9 NOTES
	#Project structure | 11/30
		# TODO: learn about reading xml files and start implementing the Server class by updating the model
			COMPLETE

	#What is left 12/6
		# TODO: Respawning snakes when they die
				COMPLETE
		# TODO: Refactoring code to separate concnerns (we have long blocks of logic that could be split into helper meth)
				COMPLETE (we should do this again before submitting)
		# TODO: Handling socketstate errors (should we even do this? [check instructions])
		# TODO: Verify that disconnected clients are being removed properly
		# TODO: Print into the console when the server is ready for clients
				COMPLETE
		# TODO: Receive move commands from the client
				COMPLETE
		# TODO: Wrap snakes around the world
		# TODO: snake parameters are being serialized in the wrong order
		# TODO: Debug/Fix segment collisions (self-collisions of snakes, wall collisions, and collisions with other snakes)