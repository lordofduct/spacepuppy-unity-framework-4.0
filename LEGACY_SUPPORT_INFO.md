# spacepuppy-unity-framework-4.0 legacy upgrade flow

Spacepuppy carries the baggage of some old technical debt that is being pulled out slowly. Here is defined the approach taken to slowly do that.

1) com.spacepuppy.obsolete

A package created to house completely obsolete scripts that no modern equivalent for exists. These scripts should be generally considered obsolete and not to be used. This project exists in case a pre-existing project needs any of those scripts. One can piece-meal important the scripts as necessary. Note a strict knowledge of those scripts will be needed to know what to important and not if going a piece-meal approach (especially the editor scripts).

2) LEGACY_SERIALIZATION

A compiler define of LEGACY_SUPPORT_X can be added to a project to support the old serialization. Some old serialization methods for scripts can't be upgraded (not just name changes, but full on reorganizing), this ensures your older project doesn't break.