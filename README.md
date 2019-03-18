# VILE
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/title_screen.gif)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/room.png)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/attack.png)

**NOTES**
- On dying enemy parts models, read/write must be enabled or collisions will not work in builds

## To Do
- close-up version of attack2 if the target is close enough
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
- Seed 646 is broken - no fitting room exists

**- B -**
- Traveling into a tunnel dead end via Conductor will slam dunk you into oblivion.
- Sliding and then flying will continue your slide when you start falling again
- Pausing while going through a conductor resets the animation
- You can sometimes jump while air attacking
- Sprinting can sometimes fuse you with a wall
- If Tess is attacked while rising into the air (attack1d), she goes flying
- you can attack while sprinting (turn into a feature)
- robots can get stuck on corner barrier posts
- CR's animation places it at origin
- camera turning uses too much deltatime
- Unpossessing enemies whilst attacking with them was problematic and given a hasty fix
- Unpossessing an enemy still doesn't always align you with the direction you're facing

**- C -**
- Reticle fades in when targeting even when it's already faded in
- Enemy HP bars sometimes get stuck on the screen
- 4-way intersection (tunnel) does not have floor backfaces or sides
- Dead end (tunnel) does not have floor sides
- Tess Claws show up for one frame at their previous position when enabled
