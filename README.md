# VILE
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/title_screen.gif)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/room.png)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/attack.png)

**NOTES**
- On dying enemy parts models, read/write must be enabled or collisions will not work in builds

## To Do
- midair versions of both of Tess's attacks
- average, everyday room designs
- advanced room designs
- reticle distance limit visualization
- other enemies
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
- combat

## Bugs

**- A -**

**- B -**
- Sprinting can sometimes fuse you with a wall
- If Tess is attacked while rising into the air (attack1d), she goes flying
- you can attack while sprinting (turn into a feature)
- Target is reset to null when target dies and doesn't get set while attacking (when W is pressed it should try to set a new target)
- robots can get stuck on corner barrier posts
- CR's animation places it at origin
- camera turning uses too much deltatime
- Unpossessing enemies whilst attacking with them was problematic and given a hasty fix
- Unpossessing an enemy still doesn't always align you with the direction you're facing

**- C -**
- SetHandPosition() in Fencer uses tracker.playerPosition as reference when it should use target
- Enemy HP bars sometimes get stuck on the screen
- 4-way intersection (tunnel) does not have floor backfaces or sides
- Dead end (tunnel) does not have floor sides
- Tess Claws show up for one frame at their previous position when enabled
