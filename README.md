This package implements a basic quest system. 

In order for this system to work, you must create quest under the Resources Folder:
Resources/Quests/{YourQuests}

I sugest you define your custom quest steps there along with all the Scriptable objects and prefabs related.


In order to find this package in unity's package manager make sure to add the scoped registery to unity's ProjectSettings:
- click new scopedRegisteries (+) in ProjectSettings/Package manager
- set the following parameters:
	- name: jeanf
	- url: https://registry.npmjs.com
	- scope fr.jeanf


Credits:
- It is inspired by this <a href="https://github.com/shapedbyrainstudios/quest-system">this</a>.
- I simply implemented this code in my package registry and tweaked it to my liking. Overtime I will add quite a lot of functionalities but for now I focus on making it work with my custom Event System.

Contributors:
[Code] Felix Cotes-Charlebois <a href="https://github.com/Percevent13">