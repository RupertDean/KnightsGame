################
# Source Files #
################

 - The raw .cs file is named Program.cs and is available in this folder, this file may not run on its own due to certain build files not being present - it has been included to allow for inspection of the code 

 - All files needed for the compiled file to run are present in the folder but some may be hidden, these files are library files and config files created by the IDE, however that can be shown by showing hidden files:
	- On Windows 10:
		- Open File Explorer from the taskbar. 

		- Select View > Options > Change folder and search options.

		- Select the View tab and, in Advanced settings, select Show hidden files, folders, and drives and OK.

 - Github repository is available at https://github.com/RupertDean/KnightsGame

#####################
# Using the program #
#####################

 - In order to run the application, there must be a few additional files present:

	- items.txt 	- This contains the items on the board, this file has been provided but can be editied if desired
			- Each item on a distinct line
			- The format is a single character for the name, one digit for the attack modifier, one digit for the defence modifier, one digit for the X-Coordinate and one digit for the Y-Coordinate
			- No spaces between the digits and characters and no extra blank lines

	- knights.txt	- This contains the knights on the board, again, the file is provided but can be edited
			- Each item on a distinct line
			- The format is a single character for the name, one digit for the X-Coordinate and one digit for the Y-Coordinate
			- No spaces between the digits and characters and no extra blank lines

	- moves.txt	- This contains the moves that the knights will perform
			- Each item on a distinct line
			- The first line must be "GAME-START"
			- The last line must be "GAME-END"
			- The format for the moves is a single character, the same as the name of the knight, a colon and the cardinal direction of the movement : North (N)  (UP)
																				   East  (E)  (RIGHT)
																				   South (S)  (DOWN)
																				   West  (W)  (LEFT)

 - If these files are present then the application can be run, double click KnightGame_RichPharm.exe
	- A command window will open and close while the program is running
	- The resulting JSON file will be available in the same folder as the .exe and will be names final_state.json


########## EOF ##########
