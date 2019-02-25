# VILE
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/title_screen.gif)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/room.png)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/attack.png)

**NOTES**
- On dying enemy parts models, read/write must be enabled or collisions will not work in builds

## To Do
- average, everyday room designs
- advanced room designs
- reticle distance limit visualization
- other enemies
- combat
- win & lose conditions
- basic cutscenes
- saving data

**Optimization**
- replace some spotlights with decals
- remove objects that have fallen off the edge

**Post-release**
- minimap
- in-depth local scoreboard
- new rooms

## Complete
- movement
- random map generation
- graphical style
- enemy possession
- targeting reticle
- title screen
- doors

## Bugs

**- A -**
- Tess can sometimes get frozen when knocked back? (might be tied to one of the B bugs)

**- B -**
- Landing on enemies when stomping has unexpected results
	- Conclusion: GroundTest only tests for solid layer
- Tess can get knocked back too far sometimes
	- Observed when being attacked while attacking
		- Conclusion: velocity is not updated when attacking
		- Fix: knockback should break Tess from attack state
- you can attack while sprinting (turn into a feature)
- Target is reset to null when target dies and doesn't get set while attacking (when W is pressed it should try to set a new target)
- robots can get stuck on corner barrier posts
- CR's animation places it at origin
- camera turning uses too much deltatime
- Unpossessing enemies whilst attacking with them was problematic and given a hasty fix
- sprinting out of an enemy still doesn't always align you with the direction you're facing

**- C -**
- small parts of enemy deaths break when trying to add their colliders (only observed in builds)
- Enemy HP bars sometimes get stuck on the screen
- Fencer fences do not always disappear when their partner dies?
- 4-way intersection (tunnel) does not have floor backfaces or sides
- Dead end (tunnel) does not have floor sides
- Tess Claws show up for one frame at their previous position when enabled
