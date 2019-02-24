# VILE
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/title_screen.gif)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/room.png)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/attack.png)

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

**- B -**
- you can attack while sprinting (turn into a feature)
- Target is reset to null when target dies and doesn't get set while attacking (when W is pressed it should try to set a new target)
- robots can get stuck on corner barrier posts
- CR's animation places it at origin
- camera turning uses too much deltatime
- Unpossessing enemies whilst attacking with them was problematic and given a hasty fix
- sprinting out of an enemy still doesn't always align you with the direction you're facing

**- C -**
- If Tess's attack2 animation starts when her idle is about to loop, it will be interrupted by idle
- 4-way intersection (tunnel) does not have floor backfaces or sides
- Dead end (tunnel) does not have floor sides
- Fencer arms do not calculate bounds (disappear when main body is off camera)
