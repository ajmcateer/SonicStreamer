### 2.3 Build 6239 (2017-01-30)

* [Changed] Updated links on info view for the new domain axelander.net

### 2.3 Build 6235 (2017-01-27)

* [Fixed] Unable to connect to server in the local network

### 2.3 Build 6224 (2017-01-16)

* [Fixed] Broken views due to missing cover for Madsonic server

### 2.3 Build 6213 (2017-01-10)

* [New]: Podcast Support
* [New]: Support for other server types beside Subsonic (starting with Madsonic)
* [New]: Added section “newest albums” on Home page
* [New]: Added section “top songs” on the detailed album view
* [New]: Added SonicStreamer Twitter profile on the Info view
* [Changed]: Switched sections “albums” and “artist info” on the detailed album view
* [Changed]: Improved security for password handling using PasswordVault
* [Changed]: AppBarButton labels can now be displayed
* [Changed]: Correct server API version is now used for all requests
* [Changed]: App version also now use increasing Build numbers
* [Fixed]: Incresed size for bit rates; in the past text was truncated for large numbers especially for FLAC files

### 2.2.0 (2016-09-28)

* [New] Support for Xbox One
* [New] Added Volume Control (known from the Windows 8.1 version)
* [Changed] Reworked Background Media Playback with the new Single Process Model

### 2.1.0 (2016-07-10)

* [New]: Offline Playback: It’s now possible to download tracks on the device and to save bandwidth or your data allowance. At the moment internet connection is still required to login on the server
* [New]: SSL login with self-signed certificates is now possible
* [New]: Hamburger Menu is now scrollable
* [New]: Track list on Playback Page now scrolls automatically
* [Fixed]: Crashes on Login Page with empty fields
* [Fixed]: Section title for ‘random albums’ on Home Page corrected
* [Fixed]: After playlist selection the Command Bar handling was fixed on Playlist Page
* [Fixed]: Tracks will be loaded again on Folder Page
* [Fixed]: Minor visual changes while adding tracks to a playlist

### 2.0.0 (2016-02-22)

* [New]: Almost all pages have a new design to met the new Windows 10 look & feel
* [New]: Artist / Album Page: new interaction menu for a single artist/ album item
* [New]: Tracks of an album can be now extended separately
* [New]: Hamburger Menu now available on all systems and pages
* [New]: Semantic Zoom also now on Folder Page
* [New]: Added new Playback Page which also shows last.fm data of the current track
* [New]: Transparent Tile
* [Changed]: Folder icon on Folder Page
* [Fixed]: Crashes (especially) on mobile phones with low memory while loading a large amount of albums

Known issue: UVC (Universal Volume Control)  is not available on desktop platform

### 1.3.0 (2015-05-30)

WinRT / Phone:

* [New]: Semantic Zoom for Album and Artist lists
* [New]: Play Button added on Artist/Album Covers
* [New]: Artist/Album Page shows now a progress bar to see the status of the data sync for large music libraries
* [New]: https:// or http:// not longer required for login
* [Changed]: Set a maximum width for the TextBox on the Search Page
* [Fixed]: Restoring session data after App-Suspend should now work again

Phone:

* [New]: CheckBoxes enabled to select single tracks of an album
* [New]: Added a Button on Login Page to simplify the navigation between the input fields (on low screen resolutions the keyboard probably hide one of the input fields)
* [Changed]: Start Page redesigned. Artist and Album sections were moved to separate pages. On the Start Page you will now see Random, Recent and Frequent Albums like on the WinRT App.
* [Fixed]: In some cases the wrong icon for the Folder/Track AppBarButton was shown
* [Fixed]: Cutted PivotHeader on low screen resolutions

### 1.2.2 / 1.2.3 (2015-05-11)

* [Fixed] Crashes due to invalid XML data

### 1.2.1 (2015-03-27)

WinRT / Windows Phone:

* [Fixed] App initialization will no longer overwrite roaming settings. Scrobbling should now work after app restart
* [Fixed] Logout should now clear app data accordingly. It may happened that you saw old data after switching to another server.
* [Changed] Improved synchronization of existing playlists

Windows Phone:

* [Changed] Scrobbling should also work while app runs in background
* [Fixed] Further corrections of playback after app suspension
* [Fixed] Universal Volume Control will be reset after background task has been shut down

### 1.2.0 (2015-03-22)

* [New] LastFM scrobble
* [Fixed] Fixed current track on playback view for Windows Phone

### 1.1.0 (2015-03-02)

* [New] Start Page has now two new sections (most played, frequent played)
* [New] LastFM Section which shows some information about the artist
* [New] Login Page: Pressing Enter in password box starts connection process
* [New] Seach Page: Pressing Enter in search box starts search
* [Changed] Optimized Views for dynamic screen resolution
* [Changed] New Icon in Folder Browser
* [Changed] Snapped Player in expanded mode is now integrated in views (Player is no longer an overlay)
* [Changed] Snapped Player is now a bit wider and track list has now more space
* [Fixed] Privacy Policy Menu Duplicate after logout

### 1.0.0 (2015-02-23)

* Initial release on Windows Phone 8.1

### 1.0.7 (2015-02-09)

* [New] SuspensionManager – If you resume the App after suspension some settings will be restored (e.g. playback and current track).
* [New] Improved Playback handling – After switching from shuffled play mode to the normal mode the original track order of the playback will be restored.
* [New] Improved Cover Download – Cover images are downloaded in required sizes and not in the original size (reduces traffic).
* [New] Shuffle and Repeat Mode are also part of Roaming Settings.
* [Changed] Font color of selected items on Album / Artist Page changed to white.
* [Fixed] AppBar on Folder Page should work now during track selection.

### 1.0.0 (2014-11-30)

* Initial release on Windows 8.1