Why this version exists
=======================
I've been using mintty for a while now, but there's no good/free way for me to
get a feature that I want, tabs! I looked around and decided that this project
was the best candidate for me to sink my teeth in. I've added (hacked) my way
into getting tabbed mintty along with a few additional features.

A few of these features required me to modify both DockPanel Suite and mintty.
I have already submitted a (simple) patch to the maintainers of mintty, I really
hope it becomes part of the distribution. For DockPanel Suite, my changes are
made locally. I will eventually put it up in case anyone else want to take a
stab at fixing additional problems with it.

Hotkeys
=======
This version of SuperPutty now hooks into WH_KEYBOARD_LL to detect hotkeys. At
first, I tried to use RegisterHotKeys, but that has certain limitations that
prevented me from using it. However, since I am too lazy to write my own code,
I've copied and pasted a hook I saw online for WH_KEYBOARD_LL. This required
me to upgrade the project to .NET 4.0. With all that said and done, SuperPutty
now have hotkey support! I don't think I will make this configurable, at least
not in the near future. What I have added (read hardcoded) into the app now:

Alt + m : new mintty tab
Alt + left : previous tab
Alt + right : next tab
Alt + h : hide/show the menu at the top
Ctrl + 1-8 : select tab in that position
Ctrl + 9 : last tab

Tab text changes
================
For the putty and mintty panel, I have hooked into a winapi event to detect
when the title gets changed. What this means is that if your mintty/putty
window title changes, the tab for that session will reflect the changes as
well.

Patched mintty tab text color support
=====================================
I have submitted a patch to mintty at http://code.google.com/p/mintty/issues/detail?id=337.
What this patch does is that it "clones" the console's output to stdout.
This version of SuperPutty will detect the output on stdout and sets the color
of the text on the tab. This lets you know if there are output waiting to
be seen in another tab.

Various hacks for focus
=======================
I have tried to capture a lot of various windows event to allow the child window
to become focused. Many of these took hours, but it is still not complete. There
are still a few bugs here and there :(. I am not a win API guy, so I am just
cobbling these hacks together at a slow pace. It's in a usable state now (I hope).

Removed more border!
=====================
On win7, I am seeing an additional border when putty/mintty is embedded. I also
removed these. I am not sure how this will affect other version of windows :(.

Added checkboxes into view items
================================
Under the view menu, when you select an option to show/hide, it now has a checkbox
by it.

Hide quick connect bar
======================
There's now an option (under View) to hide the quick connect bar. Since I am mainly
using mintty, I really don't have a use for this. I am also a big fan of the minimal
look and feel. I am using this with session tree view, quick connect, and menu bar
completely hidden.


Old README content
==================
For License information please read the License.txt included with the download

For issue tracking, discussion, documentation and downloads, please visit the 
support forums at http://superputty.vanillaforums.com/. Source is available at
GitHub at http://github.com/phendryx/superputty/tree/master/SuperPutty/.

For older development (which appears to be abandoned) issue tracking, 
documentation and downloads please visit the Google Code Project
http://code.google.com/p/superputty/
