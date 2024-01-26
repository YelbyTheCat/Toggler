# Toggler

Creates animations for Bool and int types.</br>
Adds to parameters based on object name</br>
Adds to FX layer</br>

## Simple
### Standard
Boolean ON/OFF mode for objects</br>
<a href="https://imgur.com/V2IEmLi"><img src="https://i.imgur.com/V2IEmLi.png" title="source: imgur.com" /></a></br>
 - Avatar: Your avatar and it MUST have a VRC Avatar Descriptor</br>
 - Object: Object to be toggled on and off</br>

**Options**</br>
<a href="https://imgur.com/djlOMSd"><img src="https://i.imgur.com/djlOMSd.png" title="source: imgur.com" /></a></br>
 - Swap mode: Go to Swap Mode section</br>
 - Saved: Animation saves state when changing worlds</br>
 - Write Defaults: Creates animation nodes with write defaults enabled</br>
 - Add Layer Mask: Add a generated layer mask to the layer generated</br>
 - Default ON: Start state of the animation (IE if your avatar animations are NOT shown these animations will not play on start up)</br>
 - Unity ON: Enabled in unity (IE If your avatar is NOT shown this object will be ON already)</br>

### Swap Mode</br>
Boolean swap mode for objects (One will be on always)</br>
<a href="https://imgur.com/FjDj9O2"><img src="https://i.imgur.com/FjDj9O2.png" title="source: imgur.com" /></a>
 - Avatar: Your avatar and it MUST have a VRC Avatar Descriptor</br>
 - Object: Starting object</br>
 - Object2: Object to be swapped to</br>

**Options**</br>
<a href="https://imgur.com/sHUy0UF"><img src="https://i.imgur.com/sHUy0UF.png" title="source: imgur.com" /></a>
 - Swap mode: Turning this mode on</br>
 - Saved: Animation saves state when changing worlds</br>
 - Write Defaults: Creates animation nodes with write defaults enabled</br>
 - Add Layer Mask: Add a generated layer mask to the layer generated</br>
 
### Make Toggle
For this button to be 'live' you must have the following:
 - Avatar in the avatar section
 - Object in the object section
 - Parameters in the expression section
 - FX layer in Playable Layers (Humanoid)

## Any
Integer swaping for multiple toggles (IE multiple weapons in 1 hand, 1 at a time)</br>
![image](https://user-images.githubusercontent.com/41715570/154866275-c234ba76-e9dd-4947-a2f5-283ec0fdd65a.png)
 - Parameter Name: Name for parameter, do not use special characters such as / </br>

**Buttons**</br>
 - Add Object: Adds 1 slot for objects to be put into
 - Clear: Removes the entire list
 - Remove Last: Removes the last box from the list
 - Clear Null: Removes all empty boxes</br>

**Options**</br>
![image](https://user-images.githubusercontent.com/41715570/154866379-114d256b-8e28-41e8-bf36-f5336ca13688.png)
 - Saved: Animation saves state when changing worlds</br>
 - Write Defaults: Creates animation nodes with write defaults enabled</br>
 - Add Layer Mask: Add a generated layer mask to the layer generated</br>

**Objects List**</br>
![image](https://user-images.githubusercontent.com/41715570/154866547-df5de298-e09f-41ba-b342-cf4b10fcc6f1.png)
 - Objects: All the objects
 - Default ON: This animation will play when the avatar loads if shown (Only select 1)
 - Unity ON: Will be on for those who do NOT see your animations

**Create Toggles Button**
For this button to be 'live' the following is required:
 - Avatar in the avatar section
 - Parameter Name in the parameter name section
 - Objects in the list (Suggestion: At least 2)
## Known Issues
 - Having an extra space on the end of your AVATAR name will cause it to fail (IE |yelby | will fail |yelby| will work, ignore |)
 - Having any of the following will cause creation to fail / \ . ? * : " < >
