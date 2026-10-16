# Game-window title and icon

The application display name and native window title are **Open Game Dev Story II**.
The in-game Early Access label still shows the build version for reporting.
The native title is applied directly as well as through the Window node, keeping
Godot's debug suffix out of the requested title.

The running window uses the locally imported
`Graphic/Embedded/WindowIcon-fGMGameKaishi.png`. Independent decoding of the
supported original EXE's separate PE application-icon resource matches this
existing 16×16 alias pixel for pixel. A new import format or bundled original
icon is unnecessary. Returning to the title after an import refreshes the icon;
subsequent launches use the imported icon immediately. Existing artwork-override
rules remain in force.

Before import, the authored `assets/application-icon.svg` computer icon replaces
the Godot logo. It is the project icon and is safe to distribute. The original
icon remains in the player's imported cache, outside the repository/package.
Headless or other backends without icon support skip the native icon call.

The managed assembly is explicitly pinned to **OpenGDS2** so the display-name
change does not make Godot look for a differently named C# project. Executable/
package filenames and the permanent **OpenGDS2** user-data folder stay unchanged.
This change concerns the running game window; it does not perform a Windows
executable resource-editing/export step.

Verification checks the real X11 `_NET_WM_NAME` and `_NET_WM_ICON` properties for
exact title/icon pixels before import, after the import/title refresh and after
restart. Bilingual title/scope/recovery checks keep the version visible in-game.
The supported-original domain check verifies the imported icon against the
independently decoded PE icon hash. Native Windows decoration remains part of
final-package acceptance. No desktop game export or release is created here.
