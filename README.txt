Why this version exists
=======================
I've been using mintty for a while now, but there's no good/free way for me to
get a feature that I want, tabs! I looked around and decided that this project
was the best candidate for me to sink my teeth in. I've added (hacked) my way
into getting tabbed mintty along with additional features.

Hotkeys
=======
This version of SuperPutty now hooks into WH_KEYBOARD_LL to detect hotkeys. At
first, I tried to use RegisterHotKeys, but that has certain limitations that
prevented me from using it. However, since I am too lazy to write my own code,
I've copied and pasted a hook I saw online for WH_KEYBOARD_LL. This required
me to upgrade the project to .NET 4.0. With all that said and done, SuperPUtty
now have hotkeys!

Alt + m : new mintty tab
Alt + left : previous tab
Alt + right : next tab
Ctrl + 1-8 : select tab in that position
Ctrl + 9 : last tab

Tab text changes
================
For the putty and mintty panel, I have hooked into a winapi event to detect
when the title gets changed. What this means is that if your mintty/putty
window title changes, the tab for that session will reflect the changes as
well.


For License information please read the License.txt included with the download

For issue tracking, discussion, documentation and downloads, please visit the 
support forums at http://superputty.vanillaforums.com/. Source is available at
GitHub at http://github.com/phendryx/superputty/tree/master/SuperPutty/.

For older development (which appears to be abandoned) issue tracking, 
documentation and downloads please visit the Google Code Project
http://code.google.com/p/superputty/
