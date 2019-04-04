# VILE
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/title_screen.gif)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/room.png)
![alt text](https://github.com/zgoad1/VILE/blob/master/Images/attack.png)

**NOTES**
- On dying enemy parts models, read/write must be enabled or collisions will not work in builds

## To Do
- Press W while sprinting to home in on target?
- Make collisions with SmallParts exert forces on them
- Change the items of GameController's object pool to each have their own amounts
- Tess's attack2 needs colliders still
- close-up version of attack2 if the target is close enough
- sprint attacks
- average, everyday room designs
- advanced room designs
- other enemies
- win & lose conditions
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
- Seed 2192, 931: no fitting room error
- Observed once: Tess gets stuck after killing a FlashEye in mid-air

**- B -**
- Pods can be knocked through walls by stomping
- Pods need a proper Restart as PooledObjects
- FlashEye is too fast as AI and too slow as Player, and also seems to eventually just choose a direction and go that way forever, and also never attacks
- Pod dispensers should call CreateNewPod based on how close the player is
	- If player is within 3 * radius and we don't already have a pod, do it
- Pods should not be destroyed on death because they're pooled objects
- Killing a pod while standing on it (via stomping) causes Tess's gravity to be removed???
	- Might be because destroying a gameobject doesn't call OnTriggerExit (which messes up GroundTest)
- Fencers no longer go after Tess
- Fencers can't raycast angled downward
	- When they're in the pod, raised up a bit, they can't see Tess until she starts climbing up
- It's easy for one area type to gain a monopoly over the map
- Stamina replenishes too fast (leave it for now for debugging)
- Traveling into a tunnel dead end via Conductor will slam dunk you into oblivion.
- Sprinting can sometimes fuse you with a wall
- If Tess is attacked while rising into the air (attack1d), she goes flying
- robots can get stuck on corner barrier posts
- camera turning uses too much deltatime
- Unpossessing enemies whilst attacking with them was problematic and given a hasty fix

**- C -**
- The object pool items no longer get parented to their folder
- Because the camera raycasts from a Controllable's LookAt object, the camera can go through the ceiling when the player's head is touching the ceiling
- Reticle wraps around the screen if you look behind while your target is in front
- Pods don't calculate bounds correctly after their doors animate
- Some AreaIntersections only have one Conductor and should be replaced with that area type's dead end
- Tess's behavior when emerging from a Conductor is sometimes erratic
- Enemy HP bars sometimes get stuck on the screen
- Tess Claws show up for one frame at their previous position when enabled
- Unpossessing an enemy still doesn't always align you with the direction you're facing
