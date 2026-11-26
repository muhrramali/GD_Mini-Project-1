# Desert Oasis Caravan Stop

Open the project with Unity 6000.6.10f1. In the Project panel, open Assets/Scenes/Caravan Stop.unity. Press Play and click the Game tab to give it keyboard focus.

| Control | Action |
| --- | --- |
| V | Switch between the free camera and the third-person traveller |
| W, A, S, D | Move |
| Shift | Run in third-person mode |
| Left and right arrows | Turn the camera |
| Q and E | Descend or rise in free-camera mode |
| 1, 2 and 3 | Return to free-camera mode and jump to the ridge, camp or shore |
| R | Return to the opening view |
| B | Switch to or from the imported third-person flight character |
| Mouse | Orbit the imported character |
| F | Toggle flight while controlling the imported character |
| Space | Jump with the imported character |
| Right mouse button / C | Aim / switch aiming shoulder |

In the imported mode, use **WASD** to move in the camera's direction and **Shift** to sprint or fly faster. Look up or down with the mouse and press **W** to climb or descend. Press **F** to return to ground movement under gravity.

The traveller uses a CharacterController for motion and collisions. The arm and leg animation is driven by distance walked. The camera follows the traveller, and the same scene supplies the terrain, camp, oasis and gateway. The traveller and controls were added with assistant help.
